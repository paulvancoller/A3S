/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using AutoMapper;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Extensions;
using System.Linq;

namespace za.co.grindrodbank.a3s.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly IRoleRepository roleRepository;
        private readonly ITeamRepository teamRepository;
        private readonly ILdapAuthenticationModeRepository ldapAuthenticationModeRepository;
        private readonly ISubRealmRepository subRealmRepository;
        private readonly IMapper mapper;
        private readonly ILdapConnectionService ldapConnectionService;

        public UserService(IUserRepository userRepository, IRoleRepository roleRepository, ITeamRepository teamRepository, ILdapAuthenticationModeRepository ldapAuthenticationModeRepository,
            ISubRealmRepository subRealmRepository, IMapper mapper, ILdapConnectionService ldapConnectionService)
        {
            this.userRepository = userRepository;
            this.roleRepository = roleRepository;
            this.teamRepository = teamRepository;
            this.ldapAuthenticationModeRepository = ldapAuthenticationModeRepository;
            this.subRealmRepository = subRealmRepository;
            this.mapper = mapper;
            this.ldapConnectionService = ldapConnectionService;
        }

        public async Task<User> CreateAsync(UserSubmit userSubmit, Guid createdById)
        {
            // Start transactions to allow complete rollback in case of an error
            InitSharedTransaction();

            try
            {
                userSubmit.Username = userSubmit.Username.Trim();
                userSubmit.Email = userSubmit.Email.Trim();

                if (string.IsNullOrWhiteSpace(userSubmit.Password))
                    throw new ItemNotProcessableException($"Password field required for user create.");

                if (userSubmit.Avatar != null && !userSubmit.Avatar.IsBase64String())
                    throw new ItemNotProcessableException($"Avatar field is not a valid base64 string.");

                UserModel checkUserModel = await userRepository.GetByUsernameAsync(userSubmit.Username, false);
                if (checkUserModel != null)
                    throw new ItemNotProcessableException($"User with user name '{userSubmit.Username}' already exists.");

                var userModel = mapper.Map<UserModel>(userSubmit);
                // Check that the mapper did not assign a default GUID (happens if no GUID supplied).
                if(userModel.Id == Guid.Empty.ToString())
                    userModel.Id = null;
            
                userModel.ChangedBy = createdById;

                // Validate Authentication Mode
                if (userModel.LdapAuthenticationModeId == Guid.Empty)
                    userModel.LdapAuthenticationModeId = null;

                if (userModel.LdapAuthenticationModeId != null)
                {
                    var authMode = await ldapAuthenticationModeRepository.GetByIdAsync((Guid)userModel.LdapAuthenticationModeId, true);
                    if (authMode == null)
                        throw new ItemNotFoundException($"Ldap Authentication Mode '{userModel.LdapAuthenticationModeId}' not found.");

                    if (!(await ldapConnectionService.CheckIfUserExist(userSubmit.Username, (Guid)userSubmit.LdapAuthenticationModeId)))
                        throw new ItemNotFoundException($"User with username '{userSubmit.Username}' was not found on Ldap authentication mode '{userModel.LdapAuthenticationModeId}'.");
                }

                userModel.NormalizedEmail = userSubmit.Email.ToUpper();
                userModel.NormalizedUserName = userSubmit.Username.ToUpper();
                userModel.EmailConfirmed = true;
                userModel.PhoneNumberConfirmed = (userModel.PhoneNumber != null);

                UserModel createdUser = await userRepository.CreateAsync(userModel, userSubmit.Password, isPlainTextPassword: true);

                await AssignRolesToUserFromRoleIdList(createdUser, userSubmit.RoleIds);
                await AssignTeamsToUserFromTeamIdList(createdUser, userSubmit.TeamIds);
                createdUser = await userRepository.UpdateAsync(createdUser);

                // All successful
                CommitTransaction();

                return mapper.Map<User>(createdUser);
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }

        public async Task<User> GetByIdAsync(Guid userId, bool includeRelations = false)
        {
            return mapper.Map<User>(await userRepository.GetByIdAsync(userId, includeRelations));
        }

        public async Task<List<User>> GetListAsync()
        {
            return mapper.Map<List<User>>(await userRepository.GetListAsync());
        }

        public async Task<User> UpdateAsync(UserSubmit userSubmit, Guid updatedById)
        {
            // Start transactions to allow complete rollback in case of an error
            InitSharedTransaction();

            try
            {
                userSubmit.Username = userSubmit.Username.Trim();
                userSubmit.Email = userSubmit.Email.Trim();

                if (userSubmit.Avatar != null && !userSubmit.Avatar.IsBase64String())
                    throw new ItemNotProcessableException($"Avatar field is not a valid base64 string.");

                UserModel userModel = await userRepository.GetByIdAsync(userSubmit.Uuid, true);
                if (userModel == null)
                    throw new ItemNotFoundException($"User with ID '{userSubmit.Uuid}' not found when attempting to update a user using this ID!");

                await ConfirmUniqueUserName(userModel, userSubmit);
                await ConfirmLdapLink(userModel, userSubmit);

                if (userSubmit.Avatar != null)
                    userModel.Avatar = Convert.FromBase64String(userSubmit.Avatar);
                else
                    userModel.Avatar = null;

                if (userSubmit.LdapAuthenticationModeId == Guid.Empty)
                    userModel.LdapAuthenticationModeId = null;
                else
                    userModel.LdapAuthenticationModeId = userSubmit.LdapAuthenticationModeId;

                userModel.Email = userSubmit.Email;
                userModel.NormalizedEmail = userSubmit.Email.ToUpper();
                userModel.FirstName = userSubmit.Name;
                userModel.PhoneNumber = userSubmit.PhoneNumber;
                userModel.PhoneNumberConfirmed = (userModel.PhoneNumber != null);
                userModel.Surname = userSubmit.Surname;
                userModel.UserName = userSubmit.Username;
                userModel.NormalizedUserName = userSubmit.Username.ToUpper();
                userModel.ChangedBy = updatedById;

                await AssignRolesToUserFromRoleIdList(userModel, userSubmit.RoleIds);
                await AssignTeamsToUserFromTeamIdList(userModel, userSubmit.TeamIds);
                UserModel updatedUser = await userRepository.UpdateAsync(userModel);

                // All successful
                CommitTransaction();

                return mapper.Map<User>(updatedUser);
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }

        private async Task ConfirmUniqueUserName(UserModel userModel, UserSubmit userSubmit)
        {
            if (userModel.UserName != userSubmit.Username)
            {
                // Confirm the new username is available
                UserModel checkUserModel = await userRepository.GetByUsernameAsync(userSubmit.Username, false);
                if (checkUserModel != null)
                    throw new ItemNotProcessableException($"User with user name '{userSubmit.Username}' already exists.");
            }
        }

        private async Task ConfirmLdapLink(UserModel userModel, UserSubmit userSubmit)
        {
            if (userSubmit.LdapAuthenticationModeId != null && userSubmit.LdapAuthenticationModeId != Guid.Empty)
            {
                var authMode = await ldapAuthenticationModeRepository.GetByIdAsync((Guid)userSubmit.LdapAuthenticationModeId, true);
                if (authMode == null)
                    throw new ItemNotFoundException($"Ldap Authentication Mode '{userModel.LdapAuthenticationModeId}' not found.");

                if (userSubmit.LdapAuthenticationModeId != userModel.LdapAuthenticationModeId
                    && !(await ldapConnectionService.CheckIfUserExist(userSubmit.Username, (Guid)userSubmit.LdapAuthenticationModeId)))
                    throw new ItemNotFoundException($"User with username '{userSubmit.Username}' was not found on Ldap authentication mode '{userSubmit.LdapAuthenticationModeId}'.");
            }
        }

        public async Task DeleteAsync(Guid userId)
        {
            // Start transactions to allow complete rollback in case of an error
            InitSharedTransaction();

            try
            {
                UserModel userModel = await userRepository.GetByIdAsync(userId, true);

                if (userModel == null)
                    throw new ItemNotFoundException($"User with ID '{userId}' not found when attempting to update a user using this ID!");

                await userRepository.DeleteAsync(userModel);

                // All successful
                CommitTransaction();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }

        public async Task ChangePasswordAsync(UserPasswordChangeSubmit changeSubmit)
        {
            if (string.Compare(changeSubmit.NewPassword, changeSubmit.NewPasswordConfirmed, StringComparison.Ordinal) != 0)
                throw new ItemNotProcessableException("New password and confirm new password fields do not match.");

            if (string.IsNullOrEmpty(changeSubmit.OldPassword))
                throw new ItemNotProcessableException("Old password must be specified.");

            await userRepository.ChangePassword(changeSubmit.Uuid, changeSubmit.OldPassword, changeSubmit.NewPassword);
        }

        private async Task AssignTeamsToUserFromTeamIdList(UserModel user, List<Guid> teamIds)
        {
            // The user associations for this user are going to be created or overwritten, its easier to rebuild it that apply a diff.
            user.UserTeams = new List<UserTeamModel>();

            if (teamIds != null && teamIds.Count > 0)
            {
                foreach (var teamId in teamIds)
                {
                    var team = await teamRepository.GetByIdAsync(teamId, true);

                    if (team == null)
                    {
                        throw new ItemNotFoundException($"Unable to find a team with ID: '{teamId}' when attempting to assign the team to a user.");
                    }

                    // If this team is a parent team, prevent it from being added to the user, as there is a business rule that prevents this!
                    if(team.ChildTeams.Any())
                    {
                        throw new ItemNotProcessableException($"Unable to add team {team.Name} to user '{user.NormalizedUserName}', as it is a compound team. Users cannot be direct members of compound teams!");
                    }

                    user.UserTeams.Add(new UserTeamModel
                    {
                        Team = team,
                        User = user
                    });
                }
            }
        }

        private async Task AssignRolesToUserFromRoleIdList(UserModel user, List<Guid> roleIds)
        {
            // The user associations for this user are going to be created or overwritten, its easier to rebuild it that apply a diff.
            user.UserRoles = new List<UserRoleModel>();

            if (roleIds != null && roleIds.Count > 0)
            {
                foreach (var roleId in roleIds)
                {
                    var role = await roleRepository.GetByIdAsync(roleId);

                    if (role == null)
                    {
                        throw new ItemNotFoundException($"Unable to find a role with ID: '{roleId}' when attempting to assign the role to a user.");
                    }

                    user.UserRoles.Add(new UserRoleModel
                    {
                        Role = role,
                        User = user
                    });
                }
            }
        }

        public async Task<UserProfile> CreateUserProfileAsync(Guid userId, UserProfileSubmit userProfileSubmit, Guid createdById)
        {
            InitSharedTransaction();

            try
            {
                var existingUser = await userRepository.GetByIdAsync(userId, true);

                if (existingUser == null)
                {
                    throw new ItemNotFoundException($"User with ID '{userId}' not found.");
                }

                var existingUserProfile = existingUser.Profiles.Where(up => up.Name == userProfileSubmit.Name).FirstOrDefault();

                if(existingUserProfile != null)
                {
                    throw new ItemNotProcessableException($"User profile with name '{userProfileSubmit.Name}' already exists for user.");
                }

                await AddNewProfileToUser(existingUser, userProfileSubmit, createdById);

                await userRepository.UpdateAsync(existingUser);
                CommitTransaction();

                return mapper.Map<UserProfile>(existingUser.Profiles.Where(up => up.Name == userProfileSubmit.Name).FirstOrDefault());
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }

        private async Task AddNewProfileToUser(UserModel user, UserProfileSubmit userProfileSubmit, Guid createdById)
        {
            ProfileModel newUserProfile = new ProfileModel
            {
                Name = userProfileSubmit.Name,
                Description = userProfileSubmit.Description,
                ChangedBy = createdById
            };

            // Ensure that the sub realm ID actually corresponds to an existing sub-realm.
            var existingSubRealm = await subRealmRepository.GetByIdAsync(userProfileSubmit.SubRealmId, false);

            if(existingSubRealm == null)
            {
                throw new ItemNotFoundException($"Sub-realm with ID '{userProfileSubmit.SubRealmId}' not found when attempting to associate it with a user profile.");
            }

            newUserProfile.SubRealm = existingSubRealm;
            // Ensure we have a defacto blank ProfileRoles and ProfileTeams associations.
            newUserProfile.ProfileRoles = new List<ProfileRoleModel>();
            newUserProfile.ProfileTeams = new List<ProfileTeamModel>();
            await AssignRolesToUserProfileFromRoleIdList(newUserProfile, userProfileSubmit.RoleIds, createdById);
            await AssignTeamsToUserProfileFromRoleIdList(newUserProfile, userProfileSubmit.TeamIds, createdById);
        }

        private async Task AssignRolesToUserProfileFromRoleIdList(ProfileModel userProfile, List<Guid> roleIds, Guid changedById)
        {
            List<ProfileRoleModel> newProfileRolesState = new List<ProfileRoleModel>();

            foreach(var roleIdToAdd in roleIds)
            {
                // Search the existing user profile roles state for the role.
                var existingProfileRole = userProfile.ProfileRoles.Where(upr => upr.RoleId == roleIdToAdd).FirstOrDefault();

                if(existingProfileRole != null)
                {
                    // Role was already assigned to the profile, so add it to the new state list and move on.
                    newProfileRolesState.Add(existingProfileRole);
                    continue;
                }

                // If this point of the execution is reached, we know we are adding a new profile role.
                var roleToAdd = await roleRepository.GetByIdAsync(roleIdToAdd);

                if(roleToAdd == null)
                {
                    throw new ItemNotFoundException($"Role with ID '{roleIdToAdd}' not found when attempting to add the role to a profile.");
                }

                if(roleToAdd.SubRealm.Id != userProfile.SubRealm.Id)
                {
                    throw new ItemNotProcessableException($"Cannot assign role to a profile as they are in different sub-realms.");
                }

                userProfile.ProfileRoles.Add(new ProfileRoleModel
                {
                    Profile = userProfile,
                    Role = roleToAdd,
                    ChangedBy = changedById
                });

                userProfile.ChangedBy = changedById;
            }

            userProfile.ProfileRoles = newProfileRolesState;
        }

        private async Task AssignTeamsToUserProfileFromRoleIdList(ProfileModel userProfile, List<Guid> teamIds, Guid changedById)
        {
            List<ProfileTeamModel> newProfileTeamState = new List<ProfileTeamModel>();

            foreach (var teamIdToAdd in teamIds)
            {
                // Search the existing user profile roles state for the role.
                var existingProfileTeam = userProfile.ProfileTeams.Where(upt => upt.TeamId == teamIdToAdd).FirstOrDefault();

                if (existingProfileTeam != null)
                {
                    // Role was already assigned to the profile, so add it to the new state list and move on.
                    newProfileTeamState.Add(existingProfileTeam);
                    continue;
                }

                // If this point of the execution is reached, we know we are adding a new profile role.
                var teamToAdd = await teamRepository.GetByIdAsync(teamIdToAdd, true);

                if (teamToAdd == null)
                {
                    throw new ItemNotFoundException($"Team with ID '{teamIdToAdd}' not found when attempting to add the team to a profile.");
                }

                if (teamToAdd.SubRealm.Id != userProfile.SubRealm.Id)
                {
                    throw new ItemNotProcessableException($"Cannot assign role to a profile as they are in different sub-realms.");
                }

                userProfile.ProfileTeams.Add(new ProfileTeamModel
                {
                    Profile = userProfile,
                    Team = teamToAdd,
                    ChangedBy = changedById
                });

                userProfile.ChangedBy = changedById;
            }

            userProfile.ProfileTeams = newProfileTeamState;
        }

        public Task<UserProfile> UpdateUserProfileAsync(Guid userId, UserProfileSubmit userProfileSubmit, Guid upddatedById)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserProfile>> GetUserProfileListAsync()
        {
            throw new NotImplementedException();
        }

        public Task<UserProfile> GetUserProfileByIdAsync(Guid userProfileId)
        {
            throw new NotImplementedException();
        }

        public Task<UserProfile> GetUserProfileByNameAsync(Guid userId, string userProfileName)
        {
            throw new NotImplementedException();
        }

        public Task DeleteUserProfileAsync()
        {
            throw new NotImplementedException();
        }

        public void InitSharedTransaction()
        {
            userRepository.InitSharedTransaction();
            roleRepository.InitSharedTransaction();
            ldapAuthenticationModeRepository.InitSharedTransaction();
        }

        public void CommitTransaction()
        {
            userRepository.CommitTransaction();
            roleRepository.CommitTransaction();
            ldapAuthenticationModeRepository.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            userRepository.RollbackTransaction();
            roleRepository.RollbackTransaction();
            ldapAuthenticationModeRepository.RollbackTransaction();
        }
    }
}
