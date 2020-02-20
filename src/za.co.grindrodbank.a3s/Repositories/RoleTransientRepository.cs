/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Repositories
{
    public class RoleTransientRepository : IRoleTransientRepository
    {
        private readonly A3SContext a3SContext;

        public RoleTransientRepository(A3SContext a3SContext)
        {
            this.a3SContext = a3SContext;
        }

        public async Task<RoleTransientModel> CreateAsync(RoleTransientModel roleTransient)
        {
            a3SContext.RoleTransient.Add(roleTransient);
            await a3SContext.SaveChangesAsync();

            return roleTransient;
        }

        public async Task<List<RoleTransientModel>> GetTransientsForRoleAsync(Guid roleId)
        {
            return await a3SContext.RoleTransient.Where(rt => rt.RoleId == roleId).OrderBy(rt => rt.Id).ToListAsync();
        }
    }
}
