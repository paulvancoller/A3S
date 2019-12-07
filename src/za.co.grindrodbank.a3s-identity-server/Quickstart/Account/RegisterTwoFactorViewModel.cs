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
    public class RegisterTwoFactorViewModel
    {
        public bool AllowRegisterAuthenticator { get; set; }
        public bool HasAuthenticator { get; set; }
        public bool TwoFACompulsary { get; set; }

        public string RedirectUrl { get; set; }
    }
}
