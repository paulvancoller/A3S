/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace za.co.grindrodbank.a3sidentityserver.Services
{
    using IdentityServer4;
    using za.co.grindrodbank.a3s.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using za.co.grindrodbank.a3s.Repositories;

    public class IdentityWithAdditionalClaimsProfileService : IProfileService
    {
        private readonly IUserClaimsPrincipalFactory<UserModel> _claimsFactory;
        private readonly UserManager<UserModel> _userManager;
        private readonly A3SContext a3SContext;
        private readonly IProfileRepository profileRepository;
        protected readonly ILogger Logger;

        public IdentityWithAdditionalClaimsProfileService(UserManager<UserModel> userManager,  IUserClaimsPrincipalFactory<UserModel> claimsFactory, ILogger<IdentityWithAdditionalClaimsProfileService> logger, A3SContext a3SContext, IProfileRepository profileRepository)
        {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
            Logger = logger;
            this.a3SContext = a3SContext;
            this.profileRepository = profileRepository;
        }
 
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            try
            {
                context.LogProfileRequest(Logger);
                var sub = context.Subject.GetSubjectId();
                var user = await _userManager.FindByIdAsync(sub);
                var principal = await _claimsFactory.CreateAsync(user);
                var claims = principal.Claims.ToList();
                claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();
                // Attempt to obtain a 'profile_name' from the request to see if the user is obtaining a token for a profile.
                var profileName = context.ValidatedRequest.Raw["profile_name"];

                if(profileName == null)
                {
                   await GenerateClaimsMapForUser(claims, context, user);
                }
                else
                {
                    await GenerateClaimsMapForUserProfile(profileName, claims, user);
                }

                // Generate the user Identity related claims, which are independent of user or user-profile claims.
                GenerateBaseUserClaimsMap(user, claims);
                
                context.IssuedClaims = claims;
                context.LogIssuedClaims(Logger);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        private async Task GenerateClaimsMapForUser(List<Claim> claims, ProfileDataRequestContext context, UserModel user)
        {
            await GeneratePermissionsClaimMapFromSubject(claims, context, user);
            await GenerateDataPolicyClaimMapFromSubject(claims, context, user);
            await GenerateTeamsClaimMapFromSubject(claims, context, user);
        }

        private async Task GenerateClaimsMapForUserProfile(string profileName, List<Claim> claims, UserModel user)
        {
            // Determine whether the profile legitimately belongs to the user.
            var userProfile = await profileRepository.GetByNameAsync(Guid.Parse(user.Id), profileName, false);

            // If there is no profile associated with the user, return empty claims. Alternatively, maybe an error should be thrown?
            if(userProfile == null)
            {
                Logger.LogWarning($"User profile not found! User profile name: '{profileName}'. User ID: '{user.Id}'.");
                return;
            }

            await GeneratePermissionsClaimMapForUserProfile(claims, user, userProfile);
            await GenerateDataPolicyClaimMapForUserProfile(claims, user, userProfile);
            await GenerateTeamsClaimMapForUserProfile(claims, user, userProfile);
        }

        private void GenerateBaseUserClaimsMap(UserModel user, List<Claim> claims)
        {
            if (user.Email != null)
            {
                claims.Add(new Claim(IdentityServerConstants.StandardScopes.Email, user.Email));
            }

            if (user.UserName != null)
            {
                claims.Add(new Claim("username", user.UserName));
            }
        }

        /// <summary>
        /// Generates a permissions claims map from a user, which looks up roles and the corresponding permissions connected to
        /// a user directly, as opposed to any of the user's profile.
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="context"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task GeneratePermissionsClaimMapFromSubject(List<Claim> claims, ProfileDataRequestContext context, UserModel user)
        {
            // Get the permissions for the Subject from the A3S database.
            var permissions = await a3SContext.Permission
                 .FromSqlRaw("select \"ParentRolePermission\".* " +
                          "FROM _a3s.application_user " +
                          "JOIN _a3s.user_role ON application_user.id = user_role.user_id " +
                          "JOIN _a3s.role ON role.id = user_role.role_id " +
                          "JOIN _a3s.role_function ON role.id = role_function.role_id " +
                          "JOIN _a3s.function ON role_function.function_id = function.id " +
                          "JOIN _a3s.function_permission ON function.id = function_permission.function_id " +
                          "JOIN _a3s.permission AS \"ParentRolePermission\" ON function_permission.permission_id = \"ParentRolePermission\".id " +
                          "WHERE application_user.id = {0} " +
                          "UNION " +
                          "select \"ChildRoleFunctionPermissions\".* " +
                          "FROM _a3s.application_user " +
                          "JOIN _a3s.user_role ON application_user.id = user_role.user_id " +
                          "JOIN _a3s.role AS \"ParentRole\" ON \"ParentRole\".id = user_role.role_id " +
                          "JOIN _a3s.role_role ON \"ParentRole\".id = role_role.parent_role_id " +
                          "JOIN _a3s.role AS \"ChildRole\" ON \"ChildRole\".id = role_role.child_role_id " +
                          "JOIN _a3s.role_function AS \"ChildRoleFunctionMap\" ON \"ChildRole\".id = \"ChildRoleFunctionMap\".role_id " +
                          "JOIN _a3s.function AS \"ChildRoleFunctions\" ON \"ChildRoleFunctionMap\".function_id = \"ChildRoleFunctions\".id " +
                          "JOIN _a3s.function_permission AS \"ChildRoleFunctionPermissionsMap\" ON \"ChildRoleFunctions\".id = \"ChildRoleFunctionPermissionsMap\".function_id " +
                          "JOIN _a3s.permission AS \"ChildRoleFunctionPermissions\" ON \"ChildRoleFunctionPermissionsMap\".permission_id = \"ChildRoleFunctionPermissions\".id " +
                          "WHERE application_user.id = {0}", context.Subject.GetSubjectId())
                          .OrderBy(p => p.Name)
                          .ToListAsync();

            if (permissions != null)
            {
                foreach (var permission in permissions)
                {
                    Logger.LogDebug($"Permission from A3S for User: '{user.UserName}'. Permission: '{permission.Name}'");
                    // Ensure only a distinct set of permissions gets mapped into tokens.
                    if (!claims.Exists(uc => uc.Type == "permission" && uc.Value == permission.Name))
                    {
                        claims.Add(new Claim("permission", permission.Name));
                    }
                }
            }
        }

        /// <summary>
        /// Generates a data policiy claim map from teams (and the data policies linked to them) directly associated to the user, as oppossed to any of the user's profiles.
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="context"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task GenerateDataPolicyClaimMapFromSubject(List<Claim> claims, ProfileDataRequestContext context, UserModel user)
        {
            // Get the effective data policies for the acccesing user.
            // The first portion of the query (up to the first UNION) obtains all data policies associated with teams that users are a member of.
            // The second portion of the query (up to the second UNION) fetches data policies of teams that the user is not directly a member of, but where that team is the parent
            // team of one or more child teams that the user is a member of. The user inherits the data policies of the parent team that contains one or more child teams that
            // the user is a member of.
            // The third portion of the query fetches the data policies of child teams of a given parent team, where the user is a member of the parent team, but
            // not a direct member of any of the child teams.
            var dataPolicies = await a3SContext.ApplicationDataPolicy
                 .FromSqlRaw("select \"application_data_policy\".* " +
                          // Get data policies associated with teams that the user is directly a member of.
                          "FROM _a3s.application_user " +
                          "JOIN _a3s.user_team ON application_user.id = user_team.user_id " +
                          "JOIN _a3s.team ON team.id = user_team.team_id " +
                          "JOIN _a3s.team_application_data_policy ON team.id = team_application_data_policy.team_id " +
                          "JOIN _a3s.application_data_policy ON team_application_data_policy.application_data_policy_id = application_data_policy.id " +
                          "WHERE application_user.id = {0} " +
                          // Get parent team data policies, where the user is in a child team of the parent team.
                          "UNION " +
                          "select \"application_data_policy\".* " +
                          "FROM _a3s.application_user " +
                          "JOIN _a3s.user_team ON application_user.id = user_team.user_id " +
                          "JOIN _a3s.team AS \"ChildTeam\" ON \"ChildTeam\".id = user_team.team_id " +
                          "JOIN _a3s.team_team ON team_team.child_team_id = \"ChildTeam\".id " +
                          "JOIN _a3s.team AS \"ParentTeam\" ON team_team.parent_team_id = \"ParentTeam\".id " +
                          "JOIN _a3s.team_application_data_policy ON \"ParentTeam\".id = team_application_data_policy.team_id " +
                          "JOIN _a3s.application_data_policy ON team_application_data_policy.application_data_policy_id = application_data_policy.id " +
                          "WHERE application_user.id = {0} "
                          , context.Subject.GetSubjectId()).ToListAsync();

            if (dataPolicies != null)
            {
                foreach (var dataPolicy in dataPolicies)
                {
                    Logger.LogDebug($"DataPolicy from A3S for User: {user.UserName}. DataPolicy: '{dataPolicy.Name}'");
                    // Ensure only a distinct set of permissions gets mapped into tokens.
                    if (!claims.Exists(uc => uc.Type == "dataPolicy" && uc.Value == dataPolicy.Name))
                    {
                        claims.Add(new Claim("dataPolicy", dataPolicy.Name));
                    }
                }
            }
        }

        /// <summary>
        /// Generates a teams claim map from teams directly associated to the user, as oppossed to any of the user's profiles.
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="context"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task GenerateTeamsClaimMapFromSubject(List<Claim> claims, ProfileDataRequestContext context, UserModel user)
        {
            var userTeams = await a3SContext.Team.FromSqlRaw("select team.* " +
                          // Select the teams that users are directly in.
                          "FROM _a3s.application_user " +
                          "JOIN _a3s.user_team ON application_user.id = user_team.user_id " +
                          "JOIN _a3s.team ON team.id = user_team.team_id " +
                          "WHERE application_user.id = {0} " +
                          // Select the parent teams where the user is in a child team of the parent.
                          "UNION " +
                          "select \"ParentTeam\".* " +
                          "FROM _a3s.application_user " +
                          "JOIN _a3s.user_team ON application_user.id = user_team.user_id " +
                          "JOIN _a3s.team AS \"ChildTeam\" ON \"ChildTeam\".id = user_team.team_id " +
                          "JOIN _a3s.team_team ON team_team.child_team_id = \"ChildTeam\".id " +
                          "JOIN _a3s.team AS \"ParentTeam\" ON team_team.parent_team_id = \"ParentTeam\".id " +
                          "WHERE application_user.id = {0} "
                          , context.Subject.GetSubjectId()).ToListAsync();

            if (userTeams != null)
            {
                foreach (var userTeam in userTeams)
                {
                    Logger.LogDebug($"User Teams from A3S for User: '{user.UserName}'. Team: {userTeam.Name}");
                    // Ensure only a distinct set of permissions gets mapped into tokens.
                    if (!claims.Exists(uc => uc.Type == "team" && uc.Value == userTeam.Name))
                    {
                        claims.Add(new Claim("team", userTeam.Id.ToString()));
                    }
                }
            }
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            context.IsActive = user != null;
        }

        /// <summary>
        /// Generates a permissions claims map from a user's profile, which looks up roles and the corresponding permissions connected to
        /// a user profile, rather than to the user directly.
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="user"></param>
        /// <param name="userProfile"></param>
        /// <returns></returns>
        private async Task GeneratePermissionsClaimMapForUserProfile(List<Claim> claims, UserModel user, ProfileModel userProfile)
        {
            // Get the permissions for the Subject from the A3S database.
            var permissions = await a3SContext.Permission
                 .FromSqlRaw("select \"ParentRolePermission\".* " +
                          "FROM _a3s.profile " +
                          "JOIN _a3s.profile_role ON profile.id = profile_role.profile_id " +
                          "JOIN _a3s.role ON role.id = profile_role.role_id " +
                          "JOIN _a3s.role_function ON role.id = role_function.role_id " +
                          "JOIN _a3s.function ON role_function.function_id = function.id " +
                          "JOIN _a3s.function_permission ON function.id = function_permission.function_id " +
                          "JOIN _a3s.permission AS \"ParentRolePermission\" ON function_permission.permission_id = \"ParentRolePermission\".id " +
                          "WHERE profile.id = {0} " +
                          "UNION " +
                          "select \"ChildRoleFunctionPermissions\".* " +
                          "FROM _a3s.profile " +
                          "JOIN _a3s.profile_role ON profile.id = profile_role.profile_id " +
                          "JOIN _a3s.role AS \"ParentRole\" ON \"ParentRole\".id = profile_role.role_id " +
                          "JOIN _a3s.role_role ON \"ParentRole\".id = role_role.parent_role_id " +
                          "JOIN _a3s.role AS \"ChildRole\" ON \"ChildRole\".id = role_role.child_role_id " +
                          "JOIN _a3s.role_function AS \"ChildRoleFunctionMap\" ON \"ChildRole\".id = \"ChildRoleFunctionMap\".role_id " +
                          "JOIN _a3s.function AS \"ChildRoleFunctions\" ON \"ChildRoleFunctionMap\".function_id = \"ChildRoleFunctions\".id " +
                          "JOIN _a3s.function_permission AS \"ChildRoleFunctionPermissionsMap\" ON \"ChildRoleFunctions\".id = \"ChildRoleFunctionPermissionsMap\".function_id " +
                          "JOIN _a3s.permission AS \"ChildRoleFunctionPermissions\" ON \"ChildRoleFunctionPermissionsMap\".permission_id = \"ChildRoleFunctionPermissions\".id " +
                          "WHERE profile.id = {0}", userProfile.Id)
                          .OrderBy(p => p.Name)
                          .ToListAsync();

            if (permissions != null)
            {
                foreach (var permission in permissions)
                {
                    Logger.LogDebug($"Permission from A3S for User: '{user.UserName}' using Profile: '{userProfile.Name}'. Permission: '{permission.Name}'");
                    // Ensure only a distinct set of permissions gets mapped into tokens.
                    if (!claims.Exists(uc => uc.Type == "permission" && uc.Value == permission.Name))
                    {
                        claims.Add(new Claim("permission", permission.Name));
                    }
                }
            }
        }

        private async Task GenerateDataPolicyClaimMapForUserProfile(List<Claim> claims, UserModel user, ProfileModel userProfile)
        {
            // Get the effective data policies for the acccesing user profile.
            // The first portion of the query (up to the first UNION) obtains all data policies associated with teams that the user profile is associated with.
            // The second portion of the query (up to the second UNION) fetches data policies of teams that the user profile is not directly a member of, but where that team is the parent
            // team of one or more child teams that the user profile is a member of. The user profile inherits the data policies of the parent team that contains one or more child teams that
            // the user profile is a member of.
            // The third portion of the query fetches the data policies of child teams of a given parent team, where the user profile is a member of the parent team, but
            // not a direct member of any of the child teams.
            var dataPolicies = await a3SContext.ApplicationDataPolicy
                 .FromSqlRaw("select \"application_data_policy\".* " +
                          // Get data policies associated with teams that the user is directly a member of.
                          "FROM _a3s.profile " +
                          "JOIN _a3s.profile_team ON profile.id = profile_team.profile_id " +
                          "JOIN _a3s.team ON team.id = profile_team.team_id " +
                          "JOIN _a3s.team_application_data_policy ON team.id = team_application_data_policy.team_id " +
                          "JOIN _a3s.application_data_policy ON team_application_data_policy.application_data_policy_id = application_data_policy.id " +
                          "WHERE profile.id = {0} " +
                          // Get parent team data policies, where the user is in a child team of the parent team.
                          "UNION " +
                          "select \"application_data_policy\".* " +
                          "FROM _a3s.profile " +
                          "JOIN _a3s.profile_team ON profile.id = profile_team.profile_id " +
                          "JOIN _a3s.team AS \"ChildTeam\" ON \"ChildTeam\".id = profile_team.team_id " +
                          "JOIN _a3s.team_team ON team_team.child_team_id = \"ChildTeam\".id " +
                          "JOIN _a3s.team AS \"ParentTeam\" ON team_team.parent_team_id = \"ParentTeam\".id " +
                          "JOIN _a3s.team_application_data_policy ON \"ParentTeam\".id = team_application_data_policy.team_id " +
                          "JOIN _a3s.application_data_policy ON team_application_data_policy.application_data_policy_id = application_data_policy.id " +
                          "WHERE profile.id = {0} "
                          , userProfile.Id).ToListAsync();

            if (dataPolicies != null)
            {
                foreach (var dataPolicy in dataPolicies)
                {
                    Logger.LogDebug($"DataPolicy from A3S for User: {user.UserName} using Profile: '{userProfile.Name}'. DataPolicy: '{dataPolicy.Name}'");
                    // Ensure only a distinct set of permissions gets mapped into tokens.
                    if (!claims.Exists(uc => uc.Type == "dataPolicy" && uc.Value == dataPolicy.Name))
                    {
                        claims.Add(new Claim("dataPolicy", dataPolicy.Name));
                    }
                }
            }
        }

        private async Task GenerateTeamsClaimMapForUserProfile(List<Claim> claims, UserModel user, ProfileModel userProfile)
        {
            var userProfileTeams = await a3SContext.Team.FromSqlRaw("select team.* " +
                          // Select the teams that users are directly in.
                          "FROM _a3s.profile " +
                          "JOIN _a3s.profile_team ON profile.id = profile_team.profile_id " +
                          "JOIN _a3s.team ON team.id = profile_team.team_id " +
                          "WHERE profile.id = {0} " +
                          // Select the parent teams where the user is in a child team of the parent.
                          "UNION " +
                          "select \"ParentTeam\".* " +
                          "FROM _a3s.profile " +
                          "JOIN _a3s.profile_team ON profile.id = profile_team.profile_id " +
                          "JOIN _a3s.team AS \"ChildTeam\" ON \"ChildTeam\".id = profile_team.team_id " +
                          "JOIN _a3s.team_team ON team_team.child_team_id = \"ChildTeam\".id " +
                          "JOIN _a3s.team AS \"ParentTeam\" ON team_team.parent_team_id = \"ParentTeam\".id " +
                          "WHERE profile.id = {0} "
                          , userProfile.Id).ToListAsync();

            if (userProfileTeams != null)
            {
                foreach (var userProfileTeam in userProfileTeams)
                {
                    Logger.LogDebug($"User Teams from A3S for User: '{user.UserName}' using Profile: '{userProfile.Name}'. Team: {userProfileTeam.Name}");
                    // Ensure only a distinct set of permissions gets mapped into tokens.
                    if (!claims.Exists(uc => uc.Type == "team" && uc.Value == userProfileTeam.Name))
                    {
                        claims.Add(new Claim("team", userProfileTeam.Id.ToString()));
                    }
                }
            }
        }
    }
}