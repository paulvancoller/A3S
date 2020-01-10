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
    public interface ISubRealmsRepository : ITransactableRepository
    {
        Task<SubRealmModel> GetByNameAsync(string name, bool includeRelations);
        Task<SubRealmModel> GetByIdAsync(Guid subRealmId, bool includeRelations);
        Task<SubRealmModel> CreateAsync(SubRealmModel subRealm);
        Task<SubRealmModel> UpdateAsync(SubRealmModel subRealm);
        Task DeleteAsync(SubRealmModel subRealm);
        Task<List<SubRealmModel>> GetListAsync(bool includeRelations);
    }
}
