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
    public class ProfileRepository : PaginatedRepository<ProfileModel>, IProfileRepository
    {
        private readonly A3SContext a3SContext;

        public ProfileRepository(A3SContext a3SContext)
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

        public async Task<ProfileModel> CreateAsync(ProfileModel profile)
        {
            a3SContext.Add(profile);
            await a3SContext.SaveChangesAsync();

            return profile;
        }

        public async Task DeleteAsync(ProfileModel profile)
        {
            a3SContext.Remove(profile);
            await a3SContext.SaveChangesAsync();
        }

        public async Task<ProfileModel> GetByIdAsync(Guid profileId, bool includeRelations)
        {
            IQueryable<ProfileModel> query = a3SContext.Profile.Where(p => p.Id == profileId);
            query = includeRelations ? IncludeRelations(query) : query;

            return await query.FirstOrDefaultAsync();
        }

        public async Task<ProfileModel> GetByNameAsync(Guid userId, string name, bool includeRelations)
        {
            IQueryable<ProfileModel> query = a3SContext.Profile.Where(p => p.Name == name)
                                                               .Where(p => p.User.Id == userId.ToString());

            query = includeRelations ? IncludeRelations(query) : query;

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<ProfileModel>> GetListAsync(bool includeRelations = false)
        {
            IQueryable<ProfileModel> query = a3SContext.Profile;
            query = includeRelations ? IncludeRelations(query) : query;

            return await query.ToListAsync();
        }

        public async Task<ProfileModel> UpdateAsync(ProfileModel profile)
        {
            a3SContext.Entry(profile).State = EntityState.Modified;
            await a3SContext.SaveChangesAsync();

            return profile;
        }

        public async Task<List<ProfileModel>> GetListForUserAsync(Guid userId, bool includeRelations)
        {
            IQueryable<ProfileModel> query = a3SContext.Profile.Where(p => p.User.Id == userId.ToString());
            query = includeRelations ? IncludeRelations(query) : query;

            return await query.ToListAsync();
        }

        public async Task<PaginatedResult<ProfileModel>> GetPaginatedListForUserAsync(Guid userId, int page, int pageSize, bool includeRelations, string filterName, List<KeyValuePair<string, string>> orderBy)
        {
            IQueryable<ProfileModel> query = a3SContext.Profile.Where(p => p.User.Id == userId.ToString());
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

        private IQueryable<ProfileModel> IncludeRelations(IQueryable<ProfileModel> query)
        {
            return query.Include(p => p.SubRealm)
                        .Include(p => p.User)
                        .Include(p => p.ProfileRoles)
                            .ThenInclude(pr => pr.Role)
                        .Include(p => p.ProfileTeams)
                            .ThenInclude(pt => pt.Team);
        }
    }
}
