/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;

namespace za.co.grindrodbank.a3s.Services
{
    /// <summary>
    /// A service intended for service application function operations exposed by the REST API. Application functions cannot be created or modified by
    /// users, which is why this service only exposes methods for reading application function records.
    /// </summary>
    public interface IApplicationFunctionService
    {
        /// <summary>
        /// Fetch an application function by it's ID.
        /// </summary>
        /// <param name="applicationFunctionId"></param>
        /// <returns></returns>
        Task<ApplicationFunction> GetByIdAsync(Guid applicationFunctionId);

        /// <summary>
        /// Gets a list of application functions.
        /// </summary>
        /// <returns></returns>
        Task<List<ApplicationFunction>> GetListAsync();
        Task<PaginatedResult<ApplicationFunctionModel>> GetPaginatedListAsync(int page, int pageSize, bool includeRelations, string filterName, List<KeyValuePair<string, string>> orderBy);
    }
}
