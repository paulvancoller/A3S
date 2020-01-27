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
    public interface IRoleRepository : ITransactableRepository, IPaginatedRepository<RoleModel>
    {
        Task<RoleModel> GetByNameAsync(string name);
        RoleModel GetByName(string name);
        Task<RoleModel> GetByIdAsync(Guid roleId);
        Task<RoleModel> CreateAsync(RoleModel role);
        Task<RoleModel> UpdateAsync(RoleModel role);
        Task DeleteAsync(RoleModel role);
        Task<List<RoleModel>> GetListAsync();
        public Task<PaginatedResult<RoleModel>> GetPaginatedListAsync(int page, int pageSize, bool includeRelations, string filterName, List<KeyValuePair<string, string>> orderBy);
    }
}
