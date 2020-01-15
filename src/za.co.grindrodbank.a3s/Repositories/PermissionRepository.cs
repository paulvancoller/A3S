/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Models;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace za.co.grindrodbank.a3s.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly A3SContext a3SContext;
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public PermissionRepository(A3SContext a3SContex)
        {
            this.a3SContext = a3SContex;
        }

        public void InitSharedTransaction()
        {
            if (a3SContext.Database.CurrentTransaction == null)
                a3SContext.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (a3SContext.Database.CurrentTransaction != null)
                a3SContext.Database.CurrentTransaction.Commit();
        }

        public void RollbackTransaction()
        {
            if (a3SContext.Database.CurrentTransaction != null)
                a3SContext.Database.CurrentTransaction.Rollback();
        }

        public async Task<PermissionModel> CreateAsync(PermissionModel permission)
        {
            a3SContext.Permission.Add(permission);
            await a3SContext.SaveChangesAsync();

            return permission;
        }

        public async Task Delete(PermissionModel permission)
        {
            a3SContext.Permission.Remove(permission);
            await a3SContext.SaveChangesAsync();
        }

        public async Task DeletePermissionsNotAssignedToApplicationFunctionsAsync()
        {
            List<PermissionModel> permissions = await a3SContext.Permission.Where(p => p.ApplicationFunctionPermissions.Count == 0).Include(p => p.FunctionPermissions).ToListAsync();

            foreach(var permission in permissions)
            {
                logger.Debug($"Permission '{permission.Name}' is not assigned to any functions. Removing!");
                a3SContext.Permission.Remove(permission);
            }

            await a3SContext.SaveChangesAsync();
        }

        public PermissionModel GetByName(string name, bool includeRelations = false)
        {
            if (includeRelations)
            {
                return a3SContext.Permission.Where(p => p.Name == name)
                                            .Include(p => p.ApplicationFunctionPermissions)
                                             .ThenInclude(afp => afp.ApplicationFunction)
                                             .ThenInclude(af => af.Application)
                                            .Include(p => p.SubRealmPermissions)
                                             .ThenInclude(psrp => psrp.SubRealm)
                                            .FirstOrDefault();
            }

            return a3SContext.Permission.Where(p => p.Name == name).FirstOrDefault();
        }

        public async Task<PermissionModel> GetByNameAsync(string name, bool includeRelations = false)
        {
            if (includeRelations)
            {
                return await a3SContext.Permission.Where(p => p.Name == name)
                                            .Include(p => p.ApplicationFunctionPermissions)
                                             .ThenInclude(afp => afp.ApplicationFunction)
                                             .ThenInclude(af => af.Application)
                                            .Include(p => p.SubRealmPermissions)
                                             .ThenInclude(psrp => psrp.SubRealm)
                                            .FirstOrDefaultAsync();
            }
            return await a3SContext.Permission.Where(p => p.Name == name).FirstOrDefaultAsync();
        }

        public async Task<PermissionModel> UpdateAsync(PermissionModel permission)
        {
            a3SContext.Entry(permission).State = EntityState.Modified;
            await a3SContext.SaveChangesAsync();

            return permission;
        }

        public async Task<PermissionModel> GetByIdAsync(Guid permissionId)
        {
            return await a3SContext.Permission.FindAsync(permissionId);
        }

        public async Task<List<PermissionModel>> GetListAsync()
        {
            return await a3SContext.Permission.ToListAsync();
        }

        public async Task<PermissionModel> GetByIdWithApplicationAsync(Guid permissionId)
        {
            return await a3SContext.Permission.Where(p => p.Id == permissionId)
                                              .Include(p => p.ApplicationFunctionPermissions)
                                               .ThenInclude(afp => afp.ApplicationFunction)
                                               .ThenInclude(af => af.Application)
                                              .Include(p => p.SubRealmPermissions)
                                               .ThenInclude(psrp => psrp.SubRealm)
                                              .FirstOrDefaultAsync();
        }
    }
}
