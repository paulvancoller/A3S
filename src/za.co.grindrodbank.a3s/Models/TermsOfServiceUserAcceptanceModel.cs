/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.ComponentModel.DataAnnotations.Schema;
using NpgsqlTypes;

namespace za.co.grindrodbank.a3s.Models
{
    public class TermsOfServiceUserAcceptanceModel
    {
        public Guid TermsOfServiceId { get; set; }
        public TermsOfServiceModel TermsOfService { get; set; }

        public string UserId { get; set; }
        public UserModel User { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public NpgsqlRange<DateTime> AcceptanceTime { get; set; }
    }
}
