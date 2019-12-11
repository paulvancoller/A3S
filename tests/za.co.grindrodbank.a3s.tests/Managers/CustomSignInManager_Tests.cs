/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Models;
using NSubstitute;
using Xunit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System;
using za.co.grindrodbank.a3s.Managers;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;

namespace za.co.grindrodbank.a3s.tests.Managers
{
    public class CustomSignInManager_Tests
    {
        [Fact]
        public async Task PasswordSignInAsync_Given_Returns()
        {
            // Arrange
            var mockUserStore = Substitute.For<IUserStore<UserModel>>();
            var mockOptionsAccessor = Substitute.For<IOptions<IdentityOptions>>();
            var mockPasswordHasher = Substitute.For<IPasswordHasher<UserModel>>();
            var mockUserValidators = Substitute.For<IEnumerable<IUserValidator<UserModel>>>();
            var mockPasswordValidators = Substitute.For<IEnumerable<IPasswordValidator<UserModel>>>();
            var mockKeyNormalizer = Substitute.For<ILookupNormalizer>();
            var mockErrors = Substitute.For<IdentityErrorDescriber>();
            var mockServices = Substitute.For<IServiceProvider>();
            var mockUserLogger = Substitute.For<ILogger<UserManager<UserModel>>>();

            var mockUserManager = Substitute.For<UserManager<UserModel>>(mockUserStore, mockOptionsAccessor, mockPasswordHasher,
                mockUserValidators, mockPasswordValidators, mockKeyNormalizer, mockErrors, mockServices, mockUserLogger);

            var mockContextAccessor = Substitute.For<IHttpContextAccessor>();
            var mockClaimsFactory = Substitute.For<IUserClaimsPrincipalFactory<UserModel>>();
            var mockAuthenticationSchemeProvider = Substitute.For<IAuthenticationSchemeProvider>();
            var mockSinginLogger = Substitute.For<ILogger<SignInManager<UserModel>>>();
            var mockLdapRepo = Substitute.For<ILdapAuthenticationModeRepository>();
            var mockldapConnectionService = Substitute.For<ILdapConnectionService>();
            var mockUserConfirmation = Substitute.For<IUserConfirmation<UserModel>>();

            var applicationUser = new UserModel();

            var signInManager = new CustomSignInManager<UserModel>(mockUserManager, mockContextAccessor, mockClaimsFactory, mockOptionsAccessor,
                mockSinginLogger, null, mockAuthenticationSchemeProvider, mockLdapRepo, mockldapConnectionService, mockUserConfirmation);

            // Act
            SignInResult result = await signInManager.CheckPasswordSignInAsync(applicationUser, string.Empty, false);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Succeeded, $"Empty Application User should result in failed sign in.");
        }
    }
}
