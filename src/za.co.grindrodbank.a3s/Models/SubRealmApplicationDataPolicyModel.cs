/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace za.co.grindrodbank.a3s.Models
{
    [Table("SubRealmApplicationDataPolicy")]
    public class SubRealmApplicationDataPolicyModel : AuditableModel
    {
        public Guid SubRealmId { get; set; }
        public SubRealmModel SubRealm { get; set; }
        public Guid ApplicationDataPolicyId { get; set; }
        public ApplicationDataPolicyModel ApplicationDataPolicy { get; set; }
    }
}
