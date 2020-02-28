/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace za.co.grindrodbank.a3s.Models
{
    [Table("RoleRoleTransient")]
    public class RoleRoleTransientModel : TransientStateMachineRecord
    {
        [Required]
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid ParentRoleId { get; set; }
        [Required]
        public Guid ChildRoleId { get; set; }

        public RoleRoleTransientModel()
        {

        }
    }
}
