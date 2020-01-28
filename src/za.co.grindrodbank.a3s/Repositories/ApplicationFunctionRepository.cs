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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using za.co.grindrodbank.a3s.Extensions;

namespace za.co.grindrodbank.a3s.Repositories
{
    public class ApplicationFunctionRepository : PaginatedRepository<ApplicationFunctionModel>, IApplicationFunctionRepository
    {
        private readonly A3SContext a3SContext;

        public ApplicationFunctionRepository(A3SContext a3SContext)
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

        public async Task<ApplicationFunctionModel> CreateAsync(ApplicationFunctionModel applicationFunction)
        {
            a3SContext.ApplicationFunction.Add(applicationFunction);
            await a3SContext.SaveChangesAsync();

            return applicationFunction;
        }

        public async Task DeleteAsync(ApplicationFunctionModel applicationFunction)
        {
            a3SContext.ApplicationFunction.Remove(applicationFunction);
            await a3SContext.SaveChangesAsync();
        }

        public async Task<ApplicationFunctionModel> GetByIdAsync(Guid functionId)
        {
            IQueryable<ApplicationFunctionModel> query = a3SContext.ApplicationFunction.Where(f => f.Id == functionId);
            query = IncludeRelations(query);

            return await query.FirstOrDefaultAsync();
        }

        public ApplicationFunctionModel GetByName(string name)
        {
            IQueryable<ApplicationFunctionModel> query = a3SContext.ApplicationFunction.Where(f => f.Name == name);
            query = IncludeRelations(query);

            return query.FirstOrDefault();
        }

        public async Task<ApplicationFunctionModel> GetByNameAsync(string name)
        {
            IQueryable<ApplicationFunctionModel> query = a3SContext.ApplicationFunction.Where(f => f.Name == name);
            query = IncludeRelations(query);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<ApplicationFunctionModel>> GetListAsync()
        {
            IQueryable<ApplicationFunctionModel> query = a3SContext.ApplicationFunction;
            query = IncludeRelations(query);

            return await query.ToListAsync();
        }

        public async Task<ApplicationFunctionModel> UpdateAsync(ApplicationFunctionModel applicationFunction)
        {
            a3SContext.Entry(applicationFunction).State = EntityState.Modified;
            await a3SContext.SaveChangesAsync();

            return applicationFunction;
        }

        public async Task<PaginatedResult<ApplicationFunctionModel>> GetPaginatedListAsync(int page, int pageSize, bool includeRelations, string filterName, List<KeyValuePair<string, string>> orderBy)
        {
            IQueryable<ApplicationFunctionModel> query = a3SContext.ApplicationFunction;

            query = includeRelations ? IncludeRelations(query) : query;

            if (!string.IsNullOrWhiteSpace(filterName))
            {
                query = query.Where(p => p.Name == filterName);
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

        IQueryable<ApplicationFunctionModel> IncludeRelations(IQueryable<ApplicationFunctionModel> query)
        {
            return query.Include(f => f.ApplicationFunctionPermissions)
                          .ThenInclude(fp => fp.Permission);
        }
    }
}
