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
    public interface IRoleService : ITransactableService
    {
        Task<Role> GetByIdAsync(Guid roleId);
        Task<Role> UpdateAsync(RoleSubmit roleSubmit, Guid updatedById);
        Task<Role> CreateAsync(RoleSubmit roleSubmit, Guid createdById);
        Task<List<Role>> GetListAsync();
    }
}
