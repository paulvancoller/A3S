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
    public class ProfileRepository : IProfileRepository
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
            if (!includeRelations)
            {
                return await a3SContext.Profile.Where(p => p.Id == profileId)
                                                .FirstOrDefaultAsync();
            }

            return await a3SContext.Profile.Where(p => p.Id == profileId)
                                                .Include(p => p.SubRealm)
                                                .Include(p => p.User)
                                                .Include(p => p.ProfileRoles)
                                                  .ThenInclude(pr => pr.Role)
                                                .Include(p => p.ProfileTeams)
                                                  .ThenInclude(pt => pt.Team)
                                                .FirstOrDefaultAsync();
        }

        public async Task<ProfileModel> GetByNameAsync(string name, bool includeRelations)
        {
            if (!includeRelations)
            {
                return await a3SContext.Profile.Where(p => p.Name == name)
                                                .FirstOrDefaultAsync();
            }

            return await a3SContext.Profile.Where(p => p.Name == name)
                                                .Include(p => p.SubRealm)
                                                .Include(p => p.User)
                                                .Include(p => p.ProfileRoles)
                                                  .ThenInclude(pr => pr.Role)
                                                .Include(p => p.ProfileTeams)
                                                  .ThenInclude(pt => pt.Team)
                                                .FirstOrDefaultAsync();
        }

        public async Task<List<ProfileModel>> GetListAsync(bool includeRelations = false)
        {
            if (!includeRelations)
            {
                return await a3SContext.Profile.ToListAsync();
            }

            return await a3SContext.Profile.Include(p => p.SubRealm)
                                           .Include(p => p.User)
                                           .Include(p => p.ProfileRoles)
                                             .ThenInclude(pr => pr.Role)
                                           .Include(p => p.ProfileTeams)
                                             .ThenInclude(pt => pt.Team)
                                           .ToListAsync();
        }

        public async Task<ProfileModel> UpdateAsync(ProfileModel profile)
        {
            a3SContext.Entry(profile).State = EntityState.Modified;
            await a3SContext.SaveChangesAsync();

            return profile;
        }

        public async Task<List<ProfileModel>> GetListForUserAsync(Guid userId, bool includeRelations)
        {
            if(!includeRelations)
            {
                return await a3SContext.Profile.ToListAsync();
            }

            return await a3SContext.Profile.Where(p => p.User.Id == userId.ToString())
                                           .Include(p => p.SubRealm)
                                           .Include(p => p.User)
                                           .Include(p => p.ProfileRoles)
                                             .ThenInclude(pr => pr.Role)
                                           .Include(p => p.ProfileTeams)
                                             .ThenInclude(pt => pt.Team)
                                           .ToListAsync();
        }
    }
}
