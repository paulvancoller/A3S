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
using za.co.grindrodbank.a3s.Models;
using Microsoft.EntityFrameworkCore;

namespace za.co.grindrodbank.a3s.Repositories
{
    public class FunctionRepository : IFunctionRepository
    {
        private readonly A3SContext a3SContext;

        public FunctionRepository(A3SContext a3SContext)
        {
            this.a3SContext = a3SContext;
        }

        public void InitSharedTransaction()
        {
            if (a3SContext.Database.CurrentTransaction == null)
                a3SContext.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (a3SContext.Database.CurrentTransaction != null)
                a3SContext.Database.CurrentTransaction.Commit();
        }

        public void RollbackTransaction()
        {
            if (a3SContext.Database.CurrentTransaction != null)
                a3SContext.Database.CurrentTransaction.Rollback();
        }

        public async Task<FunctionModel> CreateAsync(FunctionModel function)
        {
            a3SContext.Function.Add(function);
            await a3SContext.SaveChangesAsync();

            return function;
        }

        public async Task DeleteAsync(FunctionModel function)
        {
            a3SContext.Function.Remove(function);
            await a3SContext.SaveChangesAsync();
        }

        public async Task<FunctionModel> GetByIdAsync(Guid functionId)
        {
            return await a3SContext.Function.Where(f => f.Id == functionId)
                                            .Include(f => f.FunctionPermissions)
                                              .ThenInclude(fp => fp.Permission)
                                            .Include(f => f.Application)
                                            .FirstOrDefaultAsync();
        }

        public async Task<FunctionModel> GetByNameAsync(string name)
        {
            return await a3SContext.Function.Where(f => f.Name == name)
                                            .Include(f => f.FunctionPermissions)
                                              .ThenInclude(fp => fp.Permission)
                                            .Include(f => f.Application)
                                            .FirstOrDefaultAsync();
        }

        public async Task<List<FunctionModel>> GetListAsync()
        {
            return await a3SContext.Function.Include(f => f.FunctionPermissions)
                                              .ThenInclude(fp => fp.Permission)
                                             .Include(f => f.Application)
                                             .ToListAsync();
        }

        public async Task<FunctionModel> UpdateAsync(FunctionModel function)
        {
            a3SContext.Entry(function).State = EntityState.Modified;
            await a3SContext.SaveChangesAsync();

            return function;
        }
    }
}
