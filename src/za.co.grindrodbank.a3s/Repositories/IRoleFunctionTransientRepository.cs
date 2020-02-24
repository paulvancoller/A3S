using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Repositories
{
    public interface IRoleFunctionTransientRepository : ITransactableRepository
    {
        Task<List<RoleFunctionTransientModel>> GetTransientFunctionRelationsForRoleAsync(Guid roleId, Guid functionId);
        Task<RoleFunctionTransientModel> CreateNewTransientStateForRoleFunctionAsync(RoleFunctionTransientModel roleFunctionTransient);
    }
}
