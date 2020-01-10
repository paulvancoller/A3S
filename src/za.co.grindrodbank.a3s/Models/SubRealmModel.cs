using System;
using System.Collections.Generic;

namespace za.co.grindrodbank.a3s.Models
{
    public class SubRealmModel : AuditableModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Decription { get; set; }
        public List<SubRealmPermissionModel> SubRealmPermissions { get; set; }
    }
}
