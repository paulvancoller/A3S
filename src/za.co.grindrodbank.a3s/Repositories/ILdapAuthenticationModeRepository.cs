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
    public interface ILdapAuthenticationModeRepository : ITransactableRepository, IPaginatedRepository<LdapAuthenticationModeModel>
    {
        Task<LdapAuthenticationModeModel> GetByIdAsync(Guid ldapAuthenticationModeId, bool includePassword = false, bool includeUsers = false);
        Task<LdapAuthenticationModeModel> GetByNameAsync(string name, bool includePassword = false);
        Task<LdapAuthenticationModeModel> CreateAsync(LdapAuthenticationModeModel ldapAuthenticationMode);
        Task<LdapAuthenticationModeModel> UpdateAsync(LdapAuthenticationModeModel ldapAuthenticationMode);
        Task DeleteAsync(LdapAuthenticationModeModel authenticationMode);
        Task<List<LdapAuthenticationModeModel>> GetListAsync(bool includePassword = false);
        Task<PaginatedResult<LdapAuthenticationModeModel>> GetPaginatedListAsync(int page, int pageSize, string filterName, List<KeyValuePair<string, string>> orderBy);
    }
}
