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
    public interface ISubRealmRepository : ITransactableRepository, IPaginatedRepository<SubRealmModel>
    {
        Task<SubRealmModel> GetByNameAsync(string name, bool includeRelations);
        Task<SubRealmModel> GetByIdAsync(Guid subRealmId, bool includeRelations);
        Task<SubRealmModel> CreateAsync(SubRealmModel subRealm);
        Task<SubRealmModel> UpdateAsync(SubRealmModel subRealm);
        Task DeleteAsync(SubRealmModel subRealm);
        Task<List<SubRealmModel>> GetListAsync(bool includeRelations);
        Task<PaginatedResult<SubRealmModel>> GetPaginatedListAsync(int page, int pageSize, bool includeRelations, string filterName, List<KeyValuePair<string, string>> orderBy);
    }
}
