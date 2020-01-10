/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
namespace za.co.grindrodbank.a3s.Models
{
    public class SubRealmPermissionModel : AuditableModel
    {
        public Guid SubRealmId { get; set; }
        public SubRealmModel SubRealm { get; set; }
        public Guid PermissionId { get; set; }
        public PermissionModel Permission { get; set; }
    }
}
