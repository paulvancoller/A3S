using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace za.co.grindrodbank.a3s.Models
{
    [Table("SubRealmApplicationDataPolicy")]
    public class SubRealmApplicationDataPolicyModel
    {
        public Guid SubRealmId { get; set; }
        public SubRealmModel SubRealm { get; set; }
        public Guid ApplicationDataPolicyId { get; set; }
        public ApplicationDataPolicyModel ApplicationDataPolicy { get; set; }
    }
}
