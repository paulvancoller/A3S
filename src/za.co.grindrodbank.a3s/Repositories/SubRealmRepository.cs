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
    public class SubRealmRepository : ISubRealmsRepository
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
            if (!includeRelations)
            {
                return await a3SContext.SubRealm.Where(sr => sr.Name == name)
                                                .FirstOrDefaultAsync();
            }

            return await a3SContext.SubRealm.Where(sr => sr.Name == name)
                                                .Include(sr => sr.Profiles)
                                                .Include(sr => sr.Roles)
                                                .Include(sr => sr.Functions)
                                                .Include(sr => sr.Teams)
                                                .Include(sr => sr.SubRealmPermissions)
                                                  .ThenInclude(srp => srp.Permission)
                                                .FirstOrDefaultAsync();
        }

        public async Task<SubRealmModel> GetByIdAsync(Guid subRealmId, bool includeRelations)
        {
            if (!includeRelations)
            {
                return await a3SContext.SubRealm.Where(sr => sr.Id == subRealmId)
                                                .FirstOrDefaultAsync();
            }

            return await a3SContext.SubRealm.Where(sr => sr.Id == subRealmId)
                                                .Include(sr => sr.Profiles)
                                                .Include(sr => sr.Roles)
                                                .Include(sr => sr.Functions)
                                                .Include(sr => sr.Teams)
                                                .Include(sr => sr.SubRealmPermissions)
                                                  .ThenInclude(srp => srp.Permission)
                                                .FirstOrDefaultAsync();
        }

        public async Task<List<SubRealmModel>> GetListAsync(bool includeRelations)
        {
            if (!includeRelations)
            {
                return await a3SContext.SubRealm.ToListAsync();
            }

            return await a3SContext.SubRealm.Include(sr => sr.Profiles)
                                            .Include(sr => sr.Roles)
                                            .Include(sr => sr.Functions)
                                            .Include(sr => sr.Teams)
                                            .Include(sr => sr.SubRealmPermissions)
                                                .ThenInclude(srp => srp.Permission)
                                            .ToListAsync();
        }

        public async Task<SubRealmModel> UpdateAsync(SubRealmModel subRealm)
        {
            a3SContext.Entry(subRealm).State = EntityState.Modified;
            await a3SContext.SaveChangesAsync();

            return subRealm;
        }
    }
}
