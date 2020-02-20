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

    public interface IRoleTransientRepository
    {
        Task<List<RoleTransientModel>> GetTransientsForRoleAsync(Guid roleId);
        Task<RoleTransientModel> CreateNewTransientStateForRoleAsync(RoleTransientModel roleTransient);
    }
}
