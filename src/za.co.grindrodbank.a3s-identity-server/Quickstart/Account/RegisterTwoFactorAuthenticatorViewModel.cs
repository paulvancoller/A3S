/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
namespace za.co.grindrodbank.a3sidentityserver.Quickstart.UI
{
    public class RegisterTwoFactorAuthenticatorViewModel : TwoFactorInputModel
    {
        public string SharedKey { get; set; }
        public string QrCode { get; set; }

        public bool TwoFACompulsary { get; set; }
        public string Cancel2FARegistrationUrl { get; set; }
    }
}
