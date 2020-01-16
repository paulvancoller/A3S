using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;

namespace za.co.grindrodbank.a3s.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository userRepository;
        private readonly ITeamRepository teamRepository;
        private readonly IRoleRepository roleRepository;
        private readonly ISubRealmRepository subRealmRepository;
        private readonly IMapper mapper;

        public ProfileService(IUserRepository userRepository, ITeamRepository teamRepository, IRoleRepository roleRepository, ISubRealmRepository subRealmRepository, IMapper mapper)
        {
            this.userRepository = userRepository;
            this.teamRepository = teamRepository;
            this.roleRepository = roleRepository;
            this.subRealmRepository = subRealmRepository;
            this.mapper = mapper;
        }

        public void InitSharedTransaction()
        {
            userRepository.InitSharedTransaction();
            teamRepository.InitSharedTransaction();
            roleRepository.InitSharedTransaction();
            subRealmRepository.InitSharedTransaction();
        }

        public void CommitTransaction()
        {
            userRepository.CommitTransaction();
            teamRepository.CommitTransaction();
            roleRepository.CommitTransaction();
            subRealmRepository.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            userRepository.RollbackTransaction();
            teamRepository.RollbackTransaction();
            roleRepository.RollbackTransaction();
            subRealmRepository.RollbackTransaction();
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

                if (existingUserProfile != null)
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

            await CheckForSubRealmAndAssignToProfileIfExists(newUserProfile, userProfileSubmit);
            // Ensure we have a defacto blank ProfileRoles and ProfileTeams associations.
            newUserProfile.ProfileRoles = new List<ProfileRoleModel>();
            newUserProfile.ProfileTeams = new List<ProfileTeamModel>();
            await AssignRolesToUserProfileFromRoleIdList(newUserProfile, userProfileSubmit.RoleIds, createdById);
            await AssignTeamsToUserProfileFromTeamIdList(newUserProfile, userProfileSubmit.TeamIds, createdById);

            user.Profiles.Add(newUserProfile);
        }

        private async Task CheckForSubRealmAndAssignToProfileIfExists(ProfileModel profile, UserProfileSubmit userProfileSubmit)
        {
            // Recall that submit models with empty GUIDs will not be null but rather Guid.Empty.
            if (userProfileSubmit.SubRealmId == null || userProfileSubmit.SubRealmId == Guid.Empty)
            {
                throw new InvalidFormatException("Profiles must contain a 'sub_realm_id'.");
            }

            var existingSubRealm = await subRealmRepository.GetByIdAsync(userProfileSubmit.SubRealmId, false);
            profile.SubRealm = existingSubRealm ?? throw new ItemNotFoundException($"Sub-realm with ID '{userProfileSubmit.SubRealmId}' does not exist.");
        }

        private async Task AssignRolesToUserProfileFromRoleIdList(ProfileModel userProfile, List<Guid> roleIds, Guid changedById)
        {
            List<ProfileRoleModel> newProfileRolesState = new List<ProfileRoleModel>();

            foreach (var roleIdToAdd in roleIds)
            {
                // Search the existing user profile roles state for the role.
                var existingProfileRole = userProfile.ProfileRoles.Where(upr => upr.RoleId == roleIdToAdd).FirstOrDefault();

                if (existingProfileRole != null)
                {
                    // Role was already assigned to the profile, so add it to the new state list and move on.
                    newProfileRolesState.Add(existingProfileRole);
                    continue;
                }

                // If this point of the execution is reached, we know we are adding a new profile role.
                var roleToAdd = await roleRepository.GetByIdAsync(roleIdToAdd);

                if (roleToAdd == null)
                {
                    throw new ItemNotFoundException($"Role with ID '{roleIdToAdd}' not found when attempting to add the role to a profile.");
                }

                if (roleToAdd.SubRealm == null || roleToAdd.SubRealm.Id != userProfile.SubRealm.Id)
                {
                    throw new ItemNotProcessableException($"Cannot assign role with ID '{roleIdToAdd}' to the '{userProfile.Name}' profile as they are in different sub-realms.");
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

        private async Task AssignTeamsToUserProfileFromTeamIdList(ProfileModel userProfile, List<Guid> teamIds, Guid changedById)
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

                // If this point of the execution is reached, we know we are adding a new team.
                var teamToAdd = await teamRepository.GetByIdAsync(teamIdToAdd, true);

                if (teamToAdd == null)
                {
                    throw new ItemNotFoundException($"Team with ID '{teamIdToAdd}' not found when attempting to add the team to a profile.");
                }

                if (teamToAdd.SubRealm == null || teamToAdd.SubRealm.Id != userProfile.SubRealm.Id)
                {
                    throw new ItemNotProcessableException($"Cannot assign team to a profile as they are in different sub-realms.");
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

        public Task DeleteUserProfileAsync()
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

        public Task<List<UserProfile>> GetUserProfileListForUserAsync(Guid userId)
        {
            throw new NotImplementedException();
        }


        public Task<UserProfile> UpdateUserProfileAsync(Guid userId, UserProfileSubmit userProfileSubmit, Guid upddatedById)
        {
            throw new NotImplementedException();
        }
    }
}
