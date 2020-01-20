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

namespace za.co.grindrodbank.a3s.Services
{
    public interface ITeamService : ITransactableService
    {
        Task<Team> GetByIdAsync(Guid teamId, bool includeRelations = false);
        Task<Team> UpdateAsync(TeamSubmit teamSubmit, Guid updatedById);
        Task<Team> CreateAsync(TeamSubmit teamSubmit, Guid createdById);
        Task<List<Team>> GetListAsync();
        Task<List<Team>> GetListAsync(Guid teamMemberUserGuid);
    }
}
