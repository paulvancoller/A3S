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
    public interface ISubRealmService : ITransactableService
    {
        Task<SubRealm> GetByIdAsync(Guid subRealmId);
        Task<SubRealm> CreateAsync(SubRealmSubmit subRealmSubmit, Guid createdById);
        Task<SubRealm> UpdateAsync(Guid subRealmId, SubRealmSubmit subRealmSubmit, Guid updatedBy);
        Task<List<SubRealm>> GetListAsync();
        Task DeleteAsync(Guid subRealmId);
    }
}
