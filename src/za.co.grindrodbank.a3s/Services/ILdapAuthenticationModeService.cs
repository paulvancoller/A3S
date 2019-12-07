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

namespace za.co.grindrodbank.a3s.Services
{
    public interface ILdapAuthenticationModeService
    {
        Task<LdapAuthenticationMode> GetByIdAsync(Guid ldapAuthenticationModeId);
        Task<LdapAuthenticationMode> UpdateAsync(LdapAuthenticationModeSubmit ldapAuthenticationModeSubmit, Guid updatedById);
        Task<LdapAuthenticationMode> CreateAsync(LdapAuthenticationModeSubmit ldapAuthenticationModeSubmit, Guid createdById);
        Task<List<LdapAuthenticationMode>> GetListAsync();
        Task<ValidationResultResponse> TestAsync(LdapAuthenticationModeSubmit ldapAuthenticationModeSubmit);
        Task DeleteAsync(Guid ldapAuthenticationModeId);
    }
}
