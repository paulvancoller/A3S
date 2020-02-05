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
using za.co.grindrodbank.a3s.Extensions;

namespace za.co.grindrodbank.a3s.Repositories
{
    public class RoleRepository : PaginatedRepository<RoleModel>, IRoleRepository
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
            IQueryable<RoleModel> query = a3SContext.Role.Where(r => r.Id == roleId);
            query = IncludeRelations(query);

            return await query.FirstOrDefaultAsync();
        }

        public RoleModel GetByName(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<RoleModel> GetByNameAsync(string name)
        {
            IQueryable<RoleModel> query = a3SContext.Role.Where(r => r.Name == name);
            query = IncludeRelations(query);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<RoleModel>> GetListAsync()
        {
            IQueryable<RoleModel> query = a3SContext.Role;
            query = IncludeRelations(query);

            return await query.ToListAsync();
        }

        public async Task<RoleModel> UpdateAsync(RoleModel role)
        {
            a3SContext.Entry(role).State = EntityState.Modified;
            await a3SContext.SaveChangesAsync();

            return role;
        }

        public async Task<PaginatedResult<RoleModel>> GetPaginatedListAsync(int page, int pageSize, bool includeRelations, string filterName, List<KeyValuePair<string, string>> orderBy)
        {
            IQueryable<RoleModel> query = a3SContext.Role;

            query = includeRelations ? IncludeRelations(query) : query;

            if (!string.IsNullOrWhiteSpace(filterName))
            {
                query = query.Where(r => r.Name == filterName);
            }

            foreach (var orderByComponent in orderBy)
            {
                switch (orderByComponent.Key)
                {
                    case "name":
                        query = query.AppendOrderBy(a => a.Name, orderByComponent.Value == "asc" ? true : false);
                        break;
                }
            }

            return await GetPaginatedListFromQueryAsync(query, page, pageSize);
        }

        private IQueryable<RoleModel> IncludeRelations(IQueryable<RoleModel> query)
        {
            return query.Include(r => r.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .Include(r => r.UserRoles)
                        .ThenInclude(ur => ur.User)
                    .Include(r => r.RoleFunctions)
                        .ThenInclude(rf => rf.Function)
                    .Include(r => r.ChildRoles)
                        .ThenInclude(cr => cr.ChildRole)
                    .Include(r => r.SubRealm);
        }
    }
}
