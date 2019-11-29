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
    public class TermsOfServiceRepository : ITermsOfServiceRepository
    {
        private readonly A3SContext a3SContext;

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

        public TermsOfServiceRepository(A3SContext a3SContext)
        {
            this.a3SContext = a3SContext;
        }

        public async Task<TermsOfServiceModel> CreateAsync(TermsOfServiceModel termsOfService)
        {
            a3SContext.TermsOfService.Add(termsOfService);
            await a3SContext.SaveChangesAsync();

            return termsOfService;
        }

        public async Task DeleteAsync(TermsOfServiceModel termsOfService)
        {
            a3SContext.TermsOfService.Remove(termsOfService);
            await a3SContext.SaveChangesAsync();
        }

        public async Task<TermsOfServiceModel> GetByIdAsync(Guid termsOfServiceId, bool includeRelations)
        {
            if (includeRelations)
            {
                return await a3SContext.TermsOfService.Where(t => t.Id == termsOfServiceId)
                                      .Include(t => t.Teams)
                                      .FirstOrDefaultAsync();
            }

            return await a3SContext.TermsOfService.Where(t => t.Id == termsOfServiceId).FirstOrDefaultAsync();
        }

        public async Task<TermsOfServiceModel> GetByAgreementNameAsync(string name, bool includeRelations)
        {
            if (includeRelations)
            {
                return await a3SContext.TermsOfService.Where(t => t.AgreementName == name)
                                      .Include(t => t.Teams)
                                      .FirstOrDefaultAsync();
            }

            return await a3SContext.TermsOfService.Where(t => t.AgreementName == name).FirstOrDefaultAsync();
        }

        public async Task<List<TermsOfServiceModel>> GetListAsync()
        {
            return await a3SContext.TermsOfService.Include(t => t.Teams)
                                        .ToListAsync();
        }

        public async Task<TermsOfServiceModel> UpdateAsync(TermsOfServiceModel termsOfService)
        {
            a3SContext.Entry(termsOfService).State = EntityState.Modified;
            await a3SContext.SaveChangesAsync();

            return termsOfService;
        }

        public async Task<string> GetLastestVersionByAgreementName(string agreementName)
        {
            return await a3SContext.TermsOfService.Where(t => t.AgreementName == agreementName)
                .OrderByDescending(x => x.SysPeriod.LowerBound)
                .Take(1)
                .Select((term) => term.Version)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Guid>> GetAllOutstandingAgreementsByUserAsync(Guid userId)
        {
            /*return await a3SContext.TermsOfService.FromSql("select team.* " +
                          // Select the teams that users are directly in.
                          "FROM _a3s.application_user " +
                          "JOIN _a3s.user_team ON application_user.id = user_team.user_id " +
                          "JOIN _a3s.team ON team.id = user_team.team_id " +
                          "WHERE application_user.id = {0} " +
                          // Select the parent teams where the user is in a child team of the parent.
                          "UNION " +
                          "select \"ParentTeam\".* " +
                          "FROM _a3s.application_user " +
                          "JOIN _a3s.user_team ON application_user.id = user_team.user_id " +
                          "JOIN _a3s.team AS \"ChildTeam\" ON \"ChildTeam\".id = user_team.team_id " +
                          "JOIN _a3s.team_team ON team_team.child_team_id = \"ChildTeam\".id " +
                          "JOIN _a3s.team AS \"ParentTeam\" ON team_team.parent_team_id = \"ParentTeam\".id " +
                          "WHERE application_user.id = {0} "
                          , context.Subject.GetSubjectId()).ToListAsync();*/


            return await a3SContext.TermsOfService
                .OrderByDescending(x => x.SysPeriod.LowerBound)
                .Take(1)
                .Select((term) => term.Id)
                .ToListAsync();
                
        }

    }
}
