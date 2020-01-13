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

namespace za.co.grindrodbank.a3s.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly A3SContext a3SContext;

        public RoleRepository(A3SContext a3SContext)
        {
            this.a3SContext = a3SContext;
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

        public async Task<RoleModel> CreateAsync(RoleModel role)
        {
            a3SContext.Role.Add(role);
            await a3SContext.SaveChangesAsync();

            return role;
        }

        public async Task DeleteAsync(RoleModel role)
        {
            a3SContext.Role.Remove(role);
            await a3SContext.SaveChangesAsync();
        }

        public async Task<RoleModel> GetByIdAsync(Guid roleId)
        {
            return await a3SContext.Role.Where(r => r.Id == roleId)
                                        .Include(r => r.UserRoles)
                                          .ThenInclude(ur => ur.Role)
                                        .Include(r => r.UserRoles)
                                          .ThenInclude(ur => ur.User)
                                        .Include(r => r.RoleFunctions)
                                          .ThenInclude(rf => rf.Function)
                                        .Include(r => r.ChildRoles)
                                          .ThenInclude(cr => cr.ChildRole)
                                        .Include(r => r.SubRealm)
                                        .FirstOrDefaultAsync();
        }

        public RoleModel GetByName(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<RoleModel> GetByNameAsync(string name)
        {
            return await a3SContext.Role.Where(r => r.Name == name)
                                        .Include(r => r.UserRoles)
                                          .ThenInclude(ur => ur.Role)
                                        .Include(r => r.UserRoles)
                                          .ThenInclude(ur => ur.User)
                                        .Include(r => r.RoleFunctions)
                                          .ThenInclude(rf => rf.Function)
                                        .Include(r => r.ChildRoles)
                                          .ThenInclude(cr => cr.ChildRole)
                                        .Include(r => r.SubRealm)
                                        .FirstOrDefaultAsync();
        }

        public async Task<List<RoleModel>> GetListAsync()
        {
            return await a3SContext.Role.Include(r => r.UserRoles)
                                          .ThenInclude(ur => ur.Role)
                                        .Include(r => r.UserRoles)
                                          .ThenInclude(ur => ur.User)
                                        .Include(r => r.RoleFunctions)
                                          .ThenInclude(rf => rf.Function)
                                        .Include(r => r.ChildRoles)
                                          .ThenInclude(cr => cr.ChildRole)
                                        .Include(r => r.SubRealm)
                                        .ToListAsync();
        }

        public async Task<RoleModel> UpdateAsync(RoleModel role)
        {
            a3SContext.Entry(role).State = EntityState.Modified;
            await a3SContext.SaveChangesAsync();

            return role;
        }
    }
}
