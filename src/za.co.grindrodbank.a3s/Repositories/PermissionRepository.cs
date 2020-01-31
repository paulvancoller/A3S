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

        public async Task<List<PermissionModel>> GetListAsync(Guid userId)
        {
            return await a3SContext.Permission
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
                          "WHERE application_user.id = {0}", userId.ToString())
                          .OrderBy(p => p.Name)
                          .ToListAsync();
        }
    }
}
