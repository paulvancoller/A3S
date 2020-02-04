/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Services
{
    public interface ILdapConnectionService
    {
        Task<SignInResult> Login(UserModel appUser, string password);
        bool TestLdapSettings(LdapAuthenticationModeModel ldapAuthenticationModeModel, ref List<string> returnMessages);
        Task<bool> CheckIfUserExist(string userName, Guid ldapAuthenticationModeId);
    }
}
