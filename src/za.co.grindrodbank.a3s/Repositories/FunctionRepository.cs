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
    public class FunctionRepository : PaginatedRepository<FunctionModel>, IFunctionRepository
    {
        private readonly A3SContext a3SContext;

        public FunctionRepository(A3SContext a3SContext)
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

        public async Task<FunctionModel> CreateAsync(FunctionModel function)
        {
            a3SContext.Function.Add(function);
            await a3SContext.SaveChangesAsync();

            return function;
        }

        public async Task DeleteAsync(FunctionModel function)
        {
            a3SContext.Function.Remove(function);
            await a3SContext.SaveChangesAsync();
        }

        public async Task<FunctionModel> GetByIdAsync(Guid functionId)
        {
            IQueryable<FunctionModel> query = a3SContext.Function.Where(f => f.Id == functionId);
            query = IncludeRelations(query);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<FunctionModel> GetByNameAsync(string name)
        {
            IQueryable<FunctionModel> query = a3SContext.Function.Where(f => f.Name == name);
            query = IncludeRelations(query);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<FunctionModel>> GetListAsync()
        {
            IQueryable<FunctionModel> query = a3SContext.Function;
            query = IncludeRelations(query);

            return await query.ToListAsync();
        }

        public async Task<FunctionModel> UpdateAsync(FunctionModel function)
        {
            a3SContext.Entry(function).State = EntityState.Modified;
            await a3SContext.SaveChangesAsync();

            return function;
        }

        private IQueryable<FunctionModel> IncludeRelations(IQueryable<FunctionModel> query)
        {
            return query.Include(f => f.FunctionPermissions)
                          .ThenInclude(fp => fp.Permission)
                        .Include(f => f.Application)
                        .Include(f => f.SubRealm);
        }

        public async Task<PaginatedResult<FunctionModel>> GetPaginatedListAsync(int page, int pageSize, bool includeRelations, string filterName, List<KeyValuePair<string, string>> orderBy)
        {
            IQueryable<FunctionModel> query = a3SContext.Function;
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
    }
}
