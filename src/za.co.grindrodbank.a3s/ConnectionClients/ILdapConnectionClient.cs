/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using Novell.Directory.Ldap;

namespace za.co.grindrodbank.a3s.ConnectionClients
{
    public interface ILdapConnectionClient
    {
        void Connect(string host, int port);
        void Bind(string dn, string password);
        LdapSearchResults Search(string @base, int scope, string filter, string[] attrs, bool typesOnly);
    }
}
