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
    public class ApplicationRepository : PaginatedRepository, IApplicationRepository
    {
        private readonly A3SContext a3SContext;

        public ApplicationRepository(A3SContext a3SContext)
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

        public async Task<ApplicationModel> CreateAsync(ApplicationModel application)
        {
            a3SContext.Application.Add(application);
            await a3SContext.SaveChangesAsync();

            return application;
        }

        public async Task<ApplicationModel> GetByIdAsync(Guid applicationId)
        {
            return await a3SContext.Application.Where(a => a.Id == applicationId)
                                                .Include(a => a.Functions)
                                                  .ThenInclude(f => f.FunctionPermissions)
                                                  .ThenInclude(fp => fp.Permission)
                                                .Include(a => a.ApplicationFunctions)
                                                  .ThenInclude(f => f.ApplicationFunctionPermissions)
                                                  .ThenInclude(fp => fp.Permission)
                                                .Include(a => a.ApplicationDataPolicies)
                                               .FirstOrDefaultAsync();
        }

        public async Task<ApplicationModel> GetByNameAsync(string name)
        {
            return await a3SContext.Application.Where(a => a.Name == name)
                                                 .Include(a => a.Functions)
                                                   .ThenInclude(f => f.FunctionPermissions)
                                                   .ThenInclude(fp => fp.Permission)
                                                  .Include(a => a.ApplicationFunctions)
                                                   .ThenInclude(f => f.ApplicationFunctionPermissions)
                                                   .ThenInclude(fp => fp.Permission)
                                                  .Include(a => a.ApplicationDataPolicies)
                                                .FirstOrDefaultAsync();
        }

        public async Task<List<ApplicationModel>> GetListAsync()
        {
            return await a3SContext.Application.Include(a => a.Functions)
                                                 .ThenInclude(f => f.FunctionPermissions)
                                                 .ThenInclude(fp => fp.Permission)
                                                .Include(a => a.ApplicationFunctions)
                                                 .ThenInclude(f => f.ApplicationFunctionPermissions)
                                                 .ThenInclude(fp => fp.Permission)
                                                .Include(a => a.ApplicationDataPolicies)
                                               .ToListAsync();
        }

        public async Task<ApplicationModel> UpdateAsync(ApplicationModel application)
        {
            a3SContext.Entry(application).State = EntityState.Modified;
            await a3SContext.SaveChangesAsync();

            return application;
        }

        public Task<PaginatedResult> GetPaginatedListAsync(int page, int pageSize, string filterName, List<string> orderBy)
        {
            IQueryable<ApplicationModel> query = a3SContext.Application.Include(a => a.Functions)
                                                 .ThenInclude(f => f.FunctionPermissions)
                                                 .ThenInclude(fp => fp.Permission)
                                                .Include(a => a.ApplicationFunctions)
                                                 .ThenInclude(f => f.ApplicationFunctionPermissions)
                                                 .ThenInclude(fp => fp.Permission)
                                                .Include(a => a.ApplicationDataPolicies);

            if (!string.IsNullOrWhiteSpace(filterName))
            {
                query = query.Where(a => a.Name == filterName);
            }

            return GetPaginatedListAsync(query, page, pageSize);
        }
    }
}
