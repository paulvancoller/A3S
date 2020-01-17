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
    public interface IFunctionService : ITransactableService
    {
        Task<Function> GetByIdAsync(Guid functionId);
        Task<Function> UpdateAsync(FunctionSubmit functionSubmit, Guid updatedByGuid);
        Task<Function> CreateAsync(FunctionSubmit functionSubmit, Guid createdByGuid);
        Task<List<Function>> GetListAsync();
        Task DeleteAsync(Guid functionId);
    }
}
