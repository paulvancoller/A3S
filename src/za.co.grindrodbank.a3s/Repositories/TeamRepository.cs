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
    public class TeamRepository : ITeamRepository
    {
        private readonly A3SContext a3SContext;

        public TeamRepository(A3SContext a3SContext)
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

        public async Task<TeamModel> CreateAsync(TeamModel team)
        {
            a3SContext.Team.Add(team);
            await a3SContext.SaveChangesAsync();

            return team;
        }

        public async Task DeleteAsync(TeamModel team)
        {
            a3SContext.Team.Remove(team);
            await a3SContext.SaveChangesAsync();
        }

        public async Task<TeamModel> GetByIdAsync(Guid teamId, bool includeRelations)
        {
            if (includeRelations)
            {
                return await a3SContext.Team.Where(t => t.Id == teamId)
                                      .Include(t => t.UserTeams)
                                        .ThenInclude(ut => ut.User)
                                      .Include(t => t.ChildTeams)
                                        .ThenInclude(ct => ct.ChildTeam)
                                      .Include(t => t.ApplicationDataPolicies)
                                        .ThenInclude(adp => adp.ApplicationDataPolicy)
                                      .FirstOrDefaultAsync();
            }

            return await a3SContext.Team.Where(t => t.Id == teamId).FirstOrDefaultAsync();
        }

        public async Task<List<TeamModel>> GetListAsync()
        {
            return await a3SContext.Team.Include(t => t.UserTeams)
                                          .ThenInclude(ut => ut.User)
                                        .Include(t => t.ChildTeams)
                                         .ThenInclude(ct => ct.ChildTeam)
                                        .Include(t => t.ApplicationDataPolicies)
                                         .ThenInclude(adp => adp.ApplicationDataPolicy)
                                        .ToListAsync();
        }

        public async Task<TeamModel> UpdateAsync(TeamModel team)
        {
            a3SContext.Entry(team).State = EntityState.Modified;
            await a3SContext.SaveChangesAsync();

            return team;
        }

        public async Task<TeamModel> GetByNameAsync(string name, bool includeRelations)
        {
            if (includeRelations)
            {
                return await a3SContext.Team.Where(t => t.Name == name)
                                      .Include(t => t.UserTeams)
                                        .ThenInclude(ut => ut.User)
                                      .Include(t => t.ChildTeams)
                                        .ThenInclude(ct => ct.ChildTeam)
                                      .Include(t => t.ApplicationDataPolicies)
                                        .ThenInclude(adp => adp.ApplicationDataPolicy)
                                      .FirstOrDefaultAsync();
            }

            return await a3SContext.Team.Where(t => t.Name == name).FirstOrDefaultAsync();
        }

        public async Task<List<TeamModel>> GetListAsync(Guid teamMemberUserGuid)
        {
            return await a3SContext.Team.FromSql("select team.* " +
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
                          , teamMemberUserGuid.ToString())
                          .Include(t => t.UserTeams)
                              .ThenInclude(ut => ut.User)
                          .Include(t => t.ChildTeams)
                              .ThenInclude(ct => ct.ChildTeam)
                          .Include(t => t.ApplicationDataPolicies)
                              .ThenInclude(adp => adp.ApplicationDataPolicy)
                          .ToListAsync();
        }
    }
}
