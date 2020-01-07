using System;
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
