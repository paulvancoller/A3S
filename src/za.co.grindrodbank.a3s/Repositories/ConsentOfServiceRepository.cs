/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Repositories
{
    public class ConsentOfServiceRepository : IConsentOfServiceRepository
    {
        private readonly A3SContext a3SContext;

        public ConsentOfServiceRepository(A3SContext a3SContext)
        {
            this.a3SContext = a3SContext;
        }

        public async Task<ConsentOfServiceModel> GetCurrentConsentAsync()
        {
            return await a3SContext.ConsentOfService.FirstOrDefaultAsync();
        }

        public async Task<ConsentOfServiceModel> UpdateCurrentConsentAsync(ConsentOfServiceModel consentOfService)
        {
            var currentConsent = await a3SContext.ConsentOfService.FirstOrDefaultAsync();
            if (currentConsent == null)
            {
                var addResult = await a3SContext.ConsentOfService.AddAsync(consentOfService);
                currentConsent = addResult.Entity;
            }
            else
            {
                currentConsent.ConsentFile = consentOfService.ConsentFile;
                currentConsent.ChangedBy = consentOfService.ChangedBy;
                a3SContext.Entry(currentConsent).State = EntityState.Modified;
            }

            var affected = await a3SContext.SaveChangesAsync();
            return affected != 1 ? null : currentConsent;
        }

        public async Task<List<PermissionModel>> GetListOfPermissionsToConsentAsync(string userId)
        {
            // get user by id
            var user = await a3SContext.User.Where(x => x.Id == userId).Include(y => y.UserRoles).FirstOrDefaultAsync();

            if (user == null)
                throw new ArgumentException("User Id not defined");

            // get user roles
            var userRoleIds = user.UserRoles.Select(x => x.RoleId).Distinct();
            var userRoles = a3SContext.Role.Where(x => userRoleIds.Contains(x.Id)).Include(x => x.RoleFunctions).ToList();

            // get role functions
            var userRoleFunctionsIds = userRoles.SelectMany(x => x.RoleFunctions).Select(x => x.FunctionId).Distinct();
            var userRoleFunctions = a3SContext.Function.Where(x => userRoleFunctionsIds.Contains(x.Id))
                .Include(x => x.FunctionPermissions).ThenInclude(x => x.Permission).ToList();
            
            // function permissions
            var functionPermissions = userRoleFunctions.SelectMany(x => x.FunctionPermissions);
            var userPermissions = functionPermissions.Select(x => x.Permission).ToList();

            // get already accepted consents
            var userConsentAcceptance = a3SContext.ConsentOfServiceUserAcceptance.Where(x => x.UserId == userId)
                .Include(x => x.ConsentOfServiceAcceptancePermissions).ThenInclude(x => x.Permission).ToList();

            // merge permissions
            foreach (var acceptance in userConsentAcceptance)
            {
                // get all already accepted permissions
                var acceptedPermissionsIds = acceptance.ConsentOfServiceAcceptancePermissions.Select(x => x.PermissionId)
                    .Distinct().ToList();

                // remove all already accepted permissions
                userPermissions.RemoveAll(x => acceptedPermissionsIds.Contains(x.Id));
            }
            
            return new List<PermissionModel>(userPermissions);
        }

        public async Task<ConsentOfServiceUserAcceptanceModel> ConsentRegistration(UserModel userModel, List<PermissionModel> permissions)
        {
            // create consent
            var consent = new ConsentOfServiceUserAcceptanceModel()
            {
                Email = userModel.Email,
                FirstName = userModel.FirstName,
                Surname = userModel.Surname,
                UserId = userModel.Id,
                UserName = userModel.UserName,
                User = userModel,
                ConsentOfServiceAcceptancePermissions = new List<ConsentOfServiceUserAcceptancePermissionsModel>()
            };
            
            // add consent permissions
            foreach (var permission in permissions)
            {
                consent.ConsentOfServiceAcceptancePermissions.Add(new ConsentOfServiceUserAcceptancePermissionsModel()
                {
                    ConsentAcceptance = consent,
                    Permission = permission,
                    PermissionId = permission.Id
                });
            }

            // add
            var addResult = await a3SContext.ConsentOfServiceUserAcceptance.AddAsync(consent);
            var changes = await a3SContext.SaveChangesAsync();

            if (changes != 1)
            {
                throw new NpgsqlException("Save consent failed");
            }

            return addResult.Entity;
        }
    }
}