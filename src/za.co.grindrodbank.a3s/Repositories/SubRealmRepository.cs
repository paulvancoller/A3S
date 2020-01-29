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
using Microsoft.EntityFrameworkCore;
using za.co.grindrodbank.a3s.Extensions;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Repositories
{
    public class SubRealmRepository : PaginatedRepository<SubRealmModel>, ISubRealmRepository
    {
        private readonly A3SContext a3SContext;

        public SubRealmRepository(A3SContext a3SContext)
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

        public async Task<SubRealmModel> CreateAsync(SubRealmModel subRealm)
        {
            a3SContext.Add(subRealm);
            await a3SContext.SaveChangesAsync();

            return subRealm;
        }

        public async Task DeleteAsync(SubRealmModel subRealm)
        {
            a3SContext.Remove(subRealm);
            await a3SContext.SaveChangesAsync();
        }

        public async Task<SubRealmModel> GetByNameAsync(string name, bool includeRelations)
        {
            IQueryable<SubRealmModel> query = a3SContext.SubRealm.Where(sr => sr.Name == name);
            query = includeRelations ? IncludeRelations(query) : query;

            return await query.FirstOrDefaultAsync();
        }

        public async Task<SubRealmModel> GetByIdAsync(Guid subRealmId, bool includeRelations)
        {
            IQueryable<SubRealmModel> query = a3SContext.SubRealm.Where(sr => sr.Id == subRealmId);
            query = includeRelations ? IncludeRelations(query) : query;

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<SubRealmModel>> GetListAsync(bool includeRelations = false)
        {
            IQueryable<SubRealmModel> query = a3SContext.SubRealm;
            query = includeRelations ? IncludeRelations(query) : query;

            return await query.ToListAsync();
        }

        public async Task<SubRealmModel> UpdateAsync(SubRealmModel subRealm)
        {
            a3SContext.Entry(subRealm).State = EntityState.Modified;
            await a3SContext.SaveChangesAsync();

            return subRealm;
        }

        private IQueryable<SubRealmModel> IncludeRelations(IQueryable<SubRealmModel> query)
        {
            return query.Include(sr => sr.Profiles)
                        .Include(sr => sr.Roles)
                        .Include(sr => sr.Functions)
                        .Include(sr => sr.Teams)
                        .Include(sr => sr.SubRealmPermissions)
                            .ThenInclude(srp => srp.Permission)
                        .Include(sr => sr.SubRealmApplicationDataPolicies)
                            .ThenInclude(sradp => sradp.ApplicationDataPolicy);
        }

        public async Task<PaginatedResult<SubRealmModel>> GetPaginatedListAsync(int page, int pageSize, bool includeRelations, string filterName, List<KeyValuePair<string, string>> orderBy)
        {
            IQueryable<SubRealmModel> query = a3SContext.SubRealm;
            query = includeRelations ? IncludeRelations(query) : query;

            if (!string.IsNullOrWhiteSpace(filterName))
            {
                query = query.Where(sr => sr.Name == filterName);
            }

            foreach (var orderByComponent in orderBy)
            {
                switch (orderByComponent.Key)
                {
                    case "name":
                        query = query.AppendOrderBy(sr => sr.Name, orderByComponent.Value == "asc" ? true : false);
                        break;
                }
            }

            return await GetPaginatedListFromQueryAsync(query, page, pageSize);
        }
    }
}
