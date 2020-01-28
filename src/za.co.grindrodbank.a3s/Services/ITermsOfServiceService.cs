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
    public interface ITermsOfServiceService
    {
        Task<TermsOfService> GetByIdAsync(Guid termsOfServiceId, bool includeRelations = false);
        Task<TermsOfService> CreateAsync(TermsOfServiceSubmit termsOfServiceSubmit, Guid createdById);
        Task<List<TermsOfService>> GetListAsync();
        Task DeleteAsync(Guid termsOfServiceId);
        Task<PaginatedResult<TermsOfServiceModel>> GetPaginatedListAsync(int page, int pageSize, bool includeRelations, string filterAgreementName, List<KeyValuePair<string, string>> orderBy);
    }
}
