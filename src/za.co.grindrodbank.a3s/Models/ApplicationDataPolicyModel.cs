/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace za.co.grindrodbank.a3s.Models
{
    [Table("ApplicationDataPolicy")]
    public class ApplicationDataPolicyModel : AuditableModel
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ApplicationModel Application { get; set; }
        // An application data policy can be assigned to many teams.
        public List<TeamApplicationDataPolicyModel> ApplicationDataPolicies { get; set; }
        // An application data policy can be assigned to many sub-realms.
        public List<SubRealmApplicationDataPolicyModel> SubRealmApplicationDataPolicies { get; set; }
    }
}
