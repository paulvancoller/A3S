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
    public interface IApplicationRepository : ITransactableRepository
    {
        Task<ApplicationModel> GetByNameAsync(string name);
        Task<ApplicationModel> GetByIdAsync(Guid applicationId);
        Task<List<ApplicationModel>> GetListAsync();
        Task<ApplicationModel> CreateAsync(ApplicationModel application);
        Task<ApplicationModel> UpdateAsync(ApplicationModel application);
    }
}
