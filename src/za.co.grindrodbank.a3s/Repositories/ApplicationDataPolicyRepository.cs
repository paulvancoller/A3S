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
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Repositories
{
    public class ApplicationDataPolicyRepository : IApplicationDataPolicyRepository
    {
        private readonly A3SContext a3SContext;

        public ApplicationDataPolicyRepository(A3SContext a3SContext)
        {
            this.a3SContext = a3SContext;
        }

        public async Task<ApplicationDataPolicyModel> CreateAsync(ApplicationDataPolicyModel applicationDataPolicy)
        {
            a3SContext.ApplicationDataPolicy.Add(applicationDataPolicy);
            await a3SContext.SaveChangesAsync();

            return applicationDataPolicy;
        }

        public async Task DeleteAsync(ApplicationDataPolicyModel applicationDataPolicy)
        {
            a3SContext.ApplicationDataPolicy.Remove(applicationDataPolicy);
            await a3SContext.SaveChangesAsync();
        }

        public async Task<ApplicationDataPolicyModel> GetByIdAsync(Guid applicationDataPolicyId)
        {
            return await a3SContext.ApplicationDataPolicy.Where(adp => adp.Id == applicationDataPolicyId)
                                                         .Include(adp => adp.SubRealmApplicationDataPolicies)
                                                           .ThenInclude(sradp => sradp.SubRealm)
                                                         .FirstOrDefaultAsync();
        }

        public async Task<ApplicationDataPolicyModel> GetByNameAsync(string name)
        {
            return await a3SContext.ApplicationDataPolicy.Where(adp => adp.Name == name)
                                                         .Include(adp => adp.SubRealmApplicationDataPolicies)
                                                           .ThenInclude(sradp => sradp.SubRealm)
                                                         .FirstOrDefaultAsync();
        }

        public async Task<List<ApplicationDataPolicyModel>> GetListAsync()
        {
            return await a3SContext.ApplicationDataPolicy.ToListAsync();
        }

        public async Task<ApplicationDataPolicyModel> UpdateAsync(ApplicationDataPolicyModel applicationDataPolicy)
        {
            a3SContext.Entry(applicationDataPolicy).State = EntityState.Modified;
            await a3SContext.SaveChangesAsync();

            return applicationDataPolicy;
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
    }
}
