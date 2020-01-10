using System;
namespace za.co.grindrodbank.a3s.Models
{
    public class SubRealmPermissionModel
    {
        public Guid SubRealmId { get; set; }
        public SubRealmModel SubRealm { get; set; }
        public Guid PermissionId { get; set; }
        public PermissionModel Permission { get; set; }
    }
}
