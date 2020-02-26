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

            // get user permissions
            var userRoles = user.UserRoles.Select(x => x.RoleId);
            var roles = a3SContext.Role.Where(x => userRoles.Contains(x.Id)).Include(x => x.RoleFunctions).ToList();
            
            // role functions
            var roleFunctions = a3SContext.RoleFunction.Select(x => x.FunctionId).ToList();//a3SContext.RoleFunction.Where(x => roles.Contains(x.RoleId)).ToList();
            var functions = a3SContext.Function.Where(x => roleFunctions.Contains(x.Id)).ToList();

            // function permission
            var functionPermissions = a3SContext.FunctionPermission.Include(x => x.Function).Include(x => x.Permission)
                .ToList(); //
            var permissions = functionPermissions.Select(x => x.Permission).ToList();

            // get already accepted consents
            a3SContext.ConsentOfServiceUserAcceptance.Where(x => x.UserId == userId)
                .Include(x => x.ConsentOfServiceAcceptancePermissions);

            // merge permissions
            return new List<PermissionModel>(permissions);
        }
    }
}