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
using za.co.grindrodbank.a3s.Helpers;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Repositories
{
    public class TermsOfServiceRepository : ITermsOfServiceRepository
    {
        private readonly A3SContext a3SContext;
        private readonly IArchiveHelper archiveHelper;

        public TermsOfServiceRepository(A3SContext a3SContext, IArchiveHelper archiveHelper)
        {
            this.a3SContext = a3SContext;
            this.archiveHelper = archiveHelper;
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

        public async Task<TermsOfServiceModel> GetByIdAsync(Guid termsOfServiceId, bool includeRelations, bool includeFileContents)
        {
            TermsOfServiceModel termsOfServiceModel = null;

            if (includeRelations)
            {
                termsOfServiceModel = await a3SContext.TermsOfService.Where(t => t.Id == termsOfServiceId)
                                      .Include(t => t.Teams)
                                      .FirstOrDefaultAsync();
            }

            termsOfServiceModel = await a3SContext.TermsOfService.Where(t => t.Id == termsOfServiceId).FirstOrDefaultAsync();

            if (includeFileContents)
            {
                
            }

            return termsOfServiceModel;
        }

        public async Task<TermsOfServiceModel> GetByAgreementNameAsync(string name, bool includeRelations, bool includeFileContents)
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
            return await a3SContext.TermsOfService
                .FromSql("SELECT ts.* " +
                            "FROM _a3s.terms_of_service ts " +
                            "INNER JOIN (SELECT t.terms_of_service_id " +
                            "	FROM _a3s.application_user u " +
                            "	JOIN _a3s.user_team ut ON u.id = ut.user_id " +
                            "	JOIN _a3s.team t ON t.id = ut.team_id " +
                            "	WHERE t.terms_of_service_id IS NOT NULL " +
                            "   AND	u.id = {0} " +
                            "	UNION " +
                            "	SELECT pt.terms_of_service_id " +
                            "	FROM _a3s.application_user u " +
                            "	JOIN _a3s.user_team ut ON u.id = ut.user_id " +
                            "	JOIN _a3s.team ct ON ct.id = ut.team_id " +
                            "	JOIN _a3s.team_team tt ON tt.child_team_id = ct.id " +
                            "	JOIN _a3s.team pt ON tt.parent_team_id = pt.id " +
                            "	WHERE pt.terms_of_service_id IS NOT NULL " +
                            "   AND	u.id = {0} " +
                            "   ) tsids ON ts.id = tsids.terms_of_service_id" +
                            "LEFT OUTER JOIN " +
                            "	_a3s.terms_of_service_user_acceptance tsua on tsua.terms_of_service_id = tsids.terms_of_service_id " +
                            "	and tsua.user_id = {0} " +
                            "WHERE tsua.acceptance_time IS NULL; ", userId)
                .Select(x => x.Id)
                .ToListAsync();
        }
    }
}
