/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.Managers;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.tests.Fakes;

namespace za.co.grindrodbank.a3s.tests.Managers
{
    public class CustomUserManager_Tests
    {
        private readonly CustomUserStoreFake fakesCustomUserStore;
        private readonly IOptions<IdentityOptions> mockOptionsAccessor;
        private readonly IPasswordHasher<UserModel> mockPasswordHasher;
        private readonly IEnumerable<IUserValidator<UserModel>> mockUserValidators;
        private readonly IEnumerable<IPasswordValidator<UserModel>> mockPasswordValidators;
        private readonly ILookupNormalizer mockKeyNormalizer;
        private readonly IdentityErrorDescriber mockErrors;
        private readonly IServiceProvider mockServices;
        private readonly ILogger<UserManager<UserModel>> mockUserLogger;
        private readonly IConfiguration mockConfiguration;

        public CustomUserManager_Tests()
        {
            var fakeA3SContext = new A3SContextFake(new Microsoft.EntityFrameworkCore.DbContextOptions<A3SContext>());

            mockConfiguration = Substitute.For<IConfiguration>();
            fakesCustomUserStore = new CustomUserStoreFake(fakeA3SContext, mockConfiguration);

            mockOptionsAccessor = Substitute.For<IOptions<IdentityOptions>>();
            mockPasswordHasher = Substitute.For<IPasswordHasher<UserModel>>();
            mockUserValidators = Substitute.For<IEnumerable<IUserValidator<UserModel>>>();
            mockPasswordValidators = Substitute.For<IEnumerable<IPasswordValidator<UserModel>>>();
            mockKeyNormalizer = Substitute.For<ILookupNormalizer>();
            mockErrors = Substitute.For<IdentityErrorDescriber>();
            mockServices = Substitute.For<IServiceProvider>();
            mockUserLogger = Substitute.For<ILogger<UserManager<UserModel>>>();
        }

        [Fact]
        public async Task SetAuthenticatorTokenVerifiedAsync_UserSpecified_ExecutesSuccessfully()
        {
            // Arrange
            var customUserManager = new CustomUserManager(fakesCustomUserStore, mockOptionsAccessor, mockPasswordHasher,
                mockUserValidators, mockPasswordValidators, mockKeyNormalizer, mockErrors, mockServices, mockUserLogger);

            // Act
            Exception caughtException = null;

            try
            {
                await customUserManager.SetAuthenticatorTokenVerifiedAsync(new UserModel());
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.Null(caughtException);
        }

        [Fact]
        public void IsAuthenticatorTokenVerified_UserSpecified_ReturnsTrue()
        {
            // Arrange
            var customUserManager = new CustomUserManager(fakesCustomUserStore, mockOptionsAccessor, mockPasswordHasher,
                mockUserValidators, mockPasswordValidators, mockKeyNormalizer, mockErrors, mockServices, mockUserLogger);

            bool result = customUserManager.IsAuthenticatorTokenVerified(new UserModel());

            Assert.True(result);
        }

        [Fact]
        public async Task AgreeToTermsOfServiceAsync_ParametersSpecified_ExecutesSuccessfully()
        {
            // Arrange
            var customUserManager = new CustomUserManager(fakesCustomUserStore, mockOptionsAccessor, mockPasswordHasher,
                mockUserValidators, mockPasswordValidators, mockKeyNormalizer, mockErrors, mockServices, mockUserLogger);

            // Act
            Exception caughtException = null;

            try
            {
                await customUserManager.AgreeToTermsOfServiceAsync(new UserModel(), Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.Null(caughtException);
        }

        [Fact]
        public async Task AgreeToTermsOfServiceAsync_UserNotSpecified_ArgumentNullExceptionThrown()
        {
            // Arrange
            var customUserManager = new CustomUserManager(fakesCustomUserStore, mockOptionsAccessor, mockPasswordHasher,
                mockUserValidators, mockPasswordValidators, mockKeyNormalizer, mockErrors, mockServices, mockUserLogger);

            // Act
            Exception caughtException = null;

            try
            {
                await customUserManager.AgreeToTermsOfServiceAsync(null, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.True(caughtException is ArgumentNullException);
        }

        [Fact]
        public async Task AgreeToTermsOfServiceAsync_TermsOfServiceIdNotSpecified_ArgumentNullExceptionThrown()
        {
            // Arrange
            var customUserManager = new CustomUserManager(fakesCustomUserStore, mockOptionsAccessor, mockPasswordHasher,
                mockUserValidators, mockPasswordValidators, mockKeyNormalizer, mockErrors, mockServices, mockUserLogger);

            // Act
            Exception caughtException = null;

            try
            {
                await customUserManager.AgreeToTermsOfServiceAsync(new UserModel(), Guid.Empty);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.True(caughtException is ArgumentNullException);
        }
    }
}
