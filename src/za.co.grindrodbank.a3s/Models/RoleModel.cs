/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace za.co.grindrodbank.a3s.Models
{
    [Table("Role")]
    public class RoleModel : AuditableModel
{
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<RoleFunctionModel> RoleFunctions { get; set; }
        public List<UserRoleModel> UserRoles { get; set; }
        public List<RoleRoleModel> ParentRoles { get; set; }
        public List<RoleRoleModel> ChildRoles { get; set; }
        // A role can be assigned to many profiles.
        public List<ProfileRoleModel> ProfileRoles { get; set; }
        // A role can be assigned to a single sub-realm.
        public SubRealmModel SubRealm { get; set; }
    }
}
