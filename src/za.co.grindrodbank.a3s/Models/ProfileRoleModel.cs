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
    [Table("ProfileRole")]
    public class ProfileRoleModel : AuditableModel
    {
        public Guid ProfileId { get; set; }
        public ProfileModel Profile { get; set; }
        public Guid RoleId { get; set; }
        public RoleModel Role { get; set; }
    }
}
