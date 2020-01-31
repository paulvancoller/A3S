/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using za.co.grindrodbank.a3s.Managers;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.tests.Fakes
{
    public class CustomUserManagerFake : CustomUserManager
    {
        private bool isAuthenticatorTokenVerified;
        private bool isAuthenticatorOtpValid;
        private UserModel userModel;
        private string authenticatorKey = string.Empty;

        public CustomUserManagerFake(IUserStore<UserModel> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<UserModel> passwordHasher,
            IEnumerable<IUserValidator<UserModel>> userValidators, IEnumerable<IPasswordValidator<UserModel>> passwordValidators, ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<UserModel>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public override bool IsAuthenticatorTokenVerified(UserModel user)
        {
            return isAuthenticatorTokenVerified;
        }

        public override Task<bool> VerifyTwoFactorTokenAsync(UserModel user, string tokenProvider, string token)
        {
            if (user != null && !string.IsNullOrWhiteSpace(tokenProvider) && !string.IsNullOrWhiteSpace(token))
                return Task.FromResult(isAuthenticatorOtpValid);
            else
                return Task.FromResult(false);
        }

        public override Task<UserModel> GetUserAsync(ClaimsPrincipal principal)
        {
            return Task.FromResult(userModel);
        }

        public override Task<UserModel> FindByNameAsync(string userName)
        {
            if (userModel?.UserName == userName)
                return Task.FromResult(userModel);

            return Task.FromResult<UserModel>(null);
        }

        public override Task AgreeToTermsOfService(UserModel user, Guid termsOfServiceId)
        {
            return Task.Run(() => { Console.WriteLine("AgreeToTermsOfService executed"); });
        }

        public override Task<UserModel> FindByIdAsync(string userId)
        {
            if (userModel?.Id == userId)
                return Task.FromResult(userModel);

            return Task.FromResult<UserModel>(null);
        }

        public void SetAuthenticatorTokenVerified(bool value)
        {
            isAuthenticatorTokenVerified = value;
        }

        public void SetAuthenticatorOtpValid(bool value)
        {
            isAuthenticatorOtpValid = value;
        }

        public void SetUserModel(UserModel value)
        {
            userModel = value;
        }

        public override Task<string> GetAuthenticatorKeyAsync(UserModel user)
        {
            return Task.FromResult(authenticatorKey);
        }

        public void SetAuthenticatorKey(string value)
        {
            authenticatorKey = value;
        }

    }
}
