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
    public interface IApplicationRepository : ITransactableRepository, IPaginatedRepository<ApplicationModel>
    {
        Task<ApplicationModel> GetByNameAsync(string name);
        Task<ApplicationModel> GetByIdAsync(Guid applicationId);
        Task<List<ApplicationModel>> GetListAsync();
        Task<PaginatedResult<ApplicationModel>> GetPaginatedListAsync(int page, int pageSize, string filterName, List<string> orderBy);
        Task<ApplicationModel> CreateAsync(ApplicationModel application);
        Task<ApplicationModel> UpdateAsync(ApplicationModel application);
    }
}
