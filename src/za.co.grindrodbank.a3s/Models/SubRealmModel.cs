/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;

namespace za.co.grindrodbank.a3s.Models
{
    public class SubRealmModel : AuditableModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Decription { get; set; }
        // A Sub-realm can have many permissions associated with them and visa versa. 
        public List<SubRealmPermissionModel> SubRealmPermissions { get; set; }
        // A Sub-realm can have many profiles associted with it.
        public List<ProfileModel> Profiles { get; set; }
        // A Sub-realm can have many functions associated with it.
        public List<FunctionModel> Functions { get; set; }
        // A Sub-realm can have many roles associated with it.
        public List<RoleModel> Roles { get; set; }
        // A Sub-realm can have many teams associated with it.
        public List<TeamModel> Teams { get; set; }
    }
}
