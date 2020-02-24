using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Repositories
{
    public class RoleFunctionTransientRepository : IRoleFunctionTransientRepository
    {
        private readonly A3SContext a3SContext;

        public RoleFunctionTransientRepository(A3SContext a3SContext)
        {
            this.a3SContext = a3SContext;
        }

        public void CommitTransaction()
        {
            if (a3SContext.Database.CurrentTransaction != null)
                a3SContext.Database.CurrentTransaction.Commit();
        }

        public async Task<RoleFunctionTransientModel> CreateNewTransientStateForRoleFunctionAsync(RoleFunctionTransientModel roleFunctionTransient)
        {
            a3SContext.RoleFunctionTransient.Add(roleFunctionTransient);
            await a3SContext.SaveChangesAsync();

            return roleFunctionTransient;
        }

        public async Task<List<RoleFunctionTransientModel>> GetTransientFunctionRelationsForRoleAsync(Guid roleId, Guid functionId)
        {
            return await a3SContext.RoleFunctionTransient
                                   .Where(rft => rft.RoleId == roleId)
                                   .Where(rft => rft.FunctionId == functionId)
                                   .OrderBy(rt => rt.CreatedAt).ToListAsync();
        }

        public void InitSharedTransaction()
        {
            if (a3SContext.Database.CurrentTransaction == null)
                a3SContext.Database.BeginTransaction();
        }

        public void RollbackTransaction()
        {
            if (a3SContext.Database.CurrentTransaction != null)
                a3SContext.Database.CurrentTransaction.Rollback();
        }
    }
}
