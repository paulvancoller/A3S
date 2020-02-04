/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;

namespace za.co.grindrodbank.a3s.Services
{
    public interface IPermissionService
    {
        Task<Permission> GetByIdAsync(Guid permissionId);
        Task<List<Permission>> GetListAsync();
        Task<PaginatedResult<PermissionModel>> GetPaginatedListAsync(int page, int pageSize, string filterName, List<KeyValuePair<string, string>> orderBy);
    }
}
