/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
namespace za.co.grindrodbank.a3s.Services
{
    public interface ITransactableService
    {
        void InitSharedTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }
}
