using System;
using Novell.Directory.Ldap;

namespace za.co.grindrodbank.a3s.ConnectionClients
{
    public class LdapConnectionClient : ILdapConnectionClient
    {
        private readonly LdapConnection ldapConnection;

        public LdapConnectionClient()
        {
            ldapConnection = new LdapConnection();
        }


        ~LdapConnectionClient()
        {
            if (ldapConnection != null)
                ldapConnection.Dispose();
        }

        public void Bind(string dn, string password)
        {
            ldapConnection.Bind(dn, password);
        }

        public void Connect(string host, int port)
        {
            ldapConnection.Connect(host, port);
        }

        public LdapSearchResults Search(string @base, int scope, string filter, string[] attrs, bool typesOnly)
        {
            return ldapConnection.Search(@base, scope, filter, attrs, typesOnly);
        }
    }
}
