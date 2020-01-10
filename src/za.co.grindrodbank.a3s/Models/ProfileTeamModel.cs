/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
namespace za.co.grindrodbank.a3s.Models
{
    public class ProfileTeamModel : AuditableModel
    {
        public Guid ProfileId { get; set; }
        public ProfileModel Profile { get; set; }
        public Guid TeamId { get; set; }
        public TeamModel Team { get; set; }
    }
}
