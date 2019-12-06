/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.ComponentModel.DataAnnotations;

namespace za.co.grindrodbank.a3sidentityserver.Quickstart.UI
{
    public class TermsOfServiceInputModel
    {
        public Guid TermsOfServiceId { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "I have read and agree to this terms of service")]
        public bool Accepted { get; set; }
    }
}
