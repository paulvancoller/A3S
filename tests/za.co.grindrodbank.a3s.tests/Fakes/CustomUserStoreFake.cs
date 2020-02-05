/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Stores;

namespace za.co.grindrodbank.a3s.tests.Fakes
{
    public class CustomUserStoreFake : CustomUserStore
    {
        public CustomUserStoreFake(A3SContext a3SContext, IConfiguration configuration) : base(a3SContext, null, configuration)
        {
        }

        public override Task SetAuthenticatorTokenVerifiedAsync(UserModel userModel)
        {
            return Task.CompletedTask;
        }

        public override bool IsAuthenticatorTokenVerified(UserModel user)
        {
            return true;
        }

        public override Task AgreeToTermsOfServiceAsync(string userId, Guid termsOfServiceId)
        {
            return Task.CompletedTask;
        }
    }
}