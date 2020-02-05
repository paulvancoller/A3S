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
    public interface IApplicationFunctionRepository : ITransactableRepository, IPaginatedRepository<ApplicationFunctionModel>
    {
        Task<ApplicationFunctionModel> GetByNameAsync(string name);
        ApplicationFunctionModel GetByName(string name);
        Task<ApplicationFunctionModel> GetByIdAsync(Guid functionId);
        Task<ApplicationFunctionModel> CreateAsync(ApplicationFunctionModel function);
        Task<ApplicationFunctionModel> UpdateAsync(ApplicationFunctionModel function);
        Task DeleteAsync(ApplicationFunctionModel function);
        Task<List<ApplicationFunctionModel>> GetListAsync();
        Task<PaginatedResult<ApplicationFunctionModel>> GetPaginatedListAsync(int page, int pageSize, bool includeRelations, string filterName, List<KeyValuePair<string, string>> orderBy);
    }
}
