/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System.Linq;
using System.Threading.Tasks;

namespace za.co.grindrodbank.a3s.Repositories
{
    public interface IPaginatedRepository<T> where T : class
    {
        public Task<PaginatedResult<T>> GetPaginatedListFromQueryAsync(IQueryable<T> query, int page, int pageSize);
    }
}
