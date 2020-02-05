/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Repositories
{
    public interface ITeamRepository : ITransactableRepository, IPaginatedRepository<TeamModel>
    {
        Task<TeamModel> GetByIdAsync(Guid teamId, bool includeRelations);
        Task<TeamModel> GetByNameAsync(string name, bool includeRelations);
        Task<TeamModel> CreateAsync(TeamModel team);
        Task<TeamModel> UpdateAsync(TeamModel team);
        Task DeleteAsync(TeamModel team);
        Task<List<TeamModel>> GetListAsync();
        /// <summary>
        /// Fetches a list of teams that a given user is a member of.
        /// </summary>
        /// <param name="teamMemberUserGuid"></param>
        /// <returns></returns>
        Task<List<TeamModel>> GetListAsync(Guid teamMemberUserGuid);
        public Task<PaginatedResult<TeamModel>> GetPaginatedListAsync(int page, int pageSize, bool includeRelations, string filterName, List<KeyValuePair<string, string>> orderBy);
        /// <summary>
        /// Fetches a paginated list of teams that a given user is a member of. This includes child teams.
        /// </summary>
        /// <param name="teamMemberUserGuid"></param>
        /// <returns></returns>
        public Task<PaginatedResult<TeamModel>> GetPaginatedListForMemberUserAsync(Guid teamMemberUserGuid, int page, int pageSize, bool includeRelations, string filterName, List<KeyValuePair<string, string>> orderBy);
    }
}
