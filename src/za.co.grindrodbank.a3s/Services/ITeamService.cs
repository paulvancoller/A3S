/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;

namespace za.co.grindrodbank.a3s.Services
{
    public interface ITeamService : ITransactableService
    {
        Task<Team> GetByIdAsync(Guid teamId, bool includeRelations = false);
        Task<Team> UpdateAsync(TeamSubmit teamSubmit, Guid updatedById);
        Task<Team> CreateAsync(TeamSubmit teamSubmit, Guid createdById);
        Task<List<Team>> GetListAsync();
        Task<List<Team>> GetListAsync(Guid teamMemberUserGuid);
        public Task<PaginatedResult<TeamModel>> GetPaginatedListAsync(int page, int pageSize, bool includeRelations, string filterName, List<KeyValuePair<string, string>> orderBy);
        /// <summary>
        /// Fetches a paginated list of teams that a given user is a member of. This includes child teams.
        /// </summary>
        /// <param name="teamMemberUserGuid"></param>
        /// <returns></returns>
        public Task<PaginatedResult<TeamModel>> GetPaginatedListForMemberUserAsync(Guid teamMemberUserGuid, int page, int pageSize, bool includeRelations, string filterName, List<KeyValuePair<string, string>> orderBy);
    }
}
