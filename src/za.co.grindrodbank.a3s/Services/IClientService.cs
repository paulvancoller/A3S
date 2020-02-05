/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Repositories;

namespace za.co.grindrodbank.a3s.Services
{
    public interface IClientService
    {
        Task<List<Oauth2Client>> GetListAsync();
        Task<Oauth2Client> GetByClientIdAsync(string clientId);
        Task<PaginatedResult<Client>> GetPaginatedListAsync(int page, int pageSize, string filterName, string filterClientId, List<KeyValuePair<string, string>> orderBy);
    }
}
