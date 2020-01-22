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
    public interface IPaginatedRepository
    {
        public Task<PaginatedResult> GetPaginatedListAsync<T>(IQueryable<T> query, int page, int pageSize) where T : class;
    }
}
