/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
namespace za.co.grindrodbank.a3sidentityserver.Quickstart.UI
{
    public class TermsOfServiceInputModel
    {
        public Guid TermsOfServiceId { get; set; }
        public bool Accepted { get; set; }
        public string ReturnUrl { get; set; }
    }
}
