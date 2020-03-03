/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;

namespace za.co.grindrodbank.a3sidentityserver.Quickstart.UI
{
    public static class AccountOptions
    {
        public static readonly bool AllowLocalLogin = true;
        public static readonly bool AllowRememberLogin = true;
        public static readonly TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);

        public static readonly bool ShowLogoutPrompt = true;
        public static readonly bool AutomaticRedirectAfterSignOut = true;

        // specify the Windows authentication scheme being used
        public static readonly string WindowsAuthenticationSchemeName = Microsoft.AspNetCore.Server.IISIntegration.IISDefaults.AuthenticationScheme;
        // if user uses windows auth, should we load the groups from windows
        public static readonly bool IncludeWindowsGroups = false;

        public static readonly string InvalidCredentialsErrorMessage = "Invalid username or password";
        public static readonly string AccountLockedOutErrorMessage = "Account has been are locked out";
        public static readonly string OTPSendErrorMessage = "There was an error sending your OTP on the email address specified";

    }
}
