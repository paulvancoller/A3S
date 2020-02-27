using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Repositories
{
    public class RoleRoleTransientRepository : IRoleRoleTransientRepository
    {
        private readonly A3SContext a3SContext;

        public RoleRoleTransientRepository(A3SContext a3SContext)
        {
            this.a3SContext = a3SContext;
        }

        public void CommitTransaction()
        {
            if (a3SContext.Database.CurrentTransaction != null)
                a3SContext.Database.CurrentTransaction.Commit();
        }

        public async Task<RoleRoleTransientModel> CreateNewTransientStateForRoleChildRoleAsync(RoleRoleTransientModel roleRoleTransient)
        {
            a3SContext.RoleRoleTransient.Add(roleRoleTransient);
            await a3SContext.SaveChangesAsync();

            return roleRoleTransient;
        }

        public async Task<List<RoleRoleTransientModel>> GetAllTransientChildRoleRelationsForRoleAsync(Guid roleId)
        {
            return await a3SContext.RoleRoleTransient
                                   .Where(rrt => rrt.ParentRoleId == roleId)
                                   .OrderBy(rrt => rrt.CreatedAt).ToListAsync();
        }

        public async Task<List<RoleRoleTransientModel>> GetTransientChildRoleRelationsForRoleAsync(Guid roleId, Guid childRoleId)
        {
            return await a3SContext.RoleRoleTransient
                                   .Where(rrt => rrt.ParentRoleId == roleId)
                                   .Where(rrt => rrt.ChildRoleId == childRoleId)
                                   .OrderBy(rrt => rrt.CreatedAt).ToListAsync();
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
