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

        public async Task<List<ApplicationDataPolicyModel>> GetListAsync(Guid userId)
        {
            return await a3SContext.ApplicationDataPolicy
                .FromSqlRaw("select \"application_data_policy\".* " +
                          // Get data policies associated with teams that the user is directly a member of.
                          "FROM _a3s.application_user " +
                          "JOIN _a3s.user_team ON application_user.id = user_team.user_id " +
                          "JOIN _a3s.team ON team.id = user_team.team_id " +
                          "JOIN _a3s.team_application_data_policy ON team.id = team_application_data_policy.team_id " +
                          "JOIN _a3s.application_data_policy ON team_application_data_policy.application_data_policy_id = application_data_policy.id " +
                          "WHERE application_user.id = {0} " +
                          // Get parent team data policies, where the user is in a child team of the parent team.
                          "UNION " +
                          "select \"application_data_policy\".* " +
                          "FROM _a3s.application_user " +
                          "JOIN _a3s.user_team ON application_user.id = user_team.user_id " +
                          "JOIN _a3s.team AS \"ChildTeam\" ON \"ChildTeam\".id = user_team.team_id " +
                          "JOIN _a3s.team_team ON team_team.child_team_id = \"ChildTeam\".id " +
                          "JOIN _a3s.team AS \"ParentTeam\" ON team_team.parent_team_id = \"ParentTeam\".id " +
                          "JOIN _a3s.team_application_data_policy ON \"ParentTeam\".id = team_application_data_policy.team_id " +
                          "JOIN _a3s.application_data_policy ON team_application_data_policy.application_data_policy_id = application_data_policy.id " +
                          "WHERE application_user.id = {0} "
                          , userId.ToString()).ToListAsync();
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
