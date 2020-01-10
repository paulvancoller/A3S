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
    [Table("Team")]
    public class TeamModel : AuditableModel
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<UserTeamModel> UserTeams { get; set; }
        public List<TeamTeamModel> ChildTeams{ get; set; }
        public List<TeamTeamModel> ParentTeams { get; set; }
        public List<TeamApplicationDataPolicyModel> ApplicationDataPolicies { get; set; }
        public Guid? TermsOfServiceId { get; set; }
        public TermsOfServiceModel TermsOfService { get; set; }
        // A team can have many profiles associated with it.
        public List<ProfileTeamModel> ProfileTeams { get; set; }
        // A team can be assigned to a single sub-realm.
        public SubRealmModel SubRealm { get; set; }
    }
}
