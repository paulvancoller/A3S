/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.tests.Fakes;
using za.co.grindrodbank.a3sidentityserver.Quickstart.UI;

namespace za.co.grindrodbank.a3sidentityserver.tests.Quickstart.TermsOfService
{
    public class TermsOfServiceController_Tests
    {
        private readonly CustomUserManagerFake fakeUserManager;
        private readonly IConfiguration mockConfiguration;
        private readonly UserModel userModel;
        private readonly ITermsOfServiceRepository termsOfServiceRepository;

        private const string RETURN_URL = "/returnUrl";

        public TermsOfServiceController_Tests()
        {
            var mockOptionsAccessor = Substitute.For<IOptions<IdentityOptions>>();
            var mockPasswordHasher = Substitute.For<IPasswordHasher<UserModel>>();
            var mockUserValidators = Substitute.For<IEnumerable<IUserValidator<UserModel>>>();
            var mockPasswordValidators = Substitute.For<IEnumerable<IPasswordValidator<UserModel>>>();
            var mockKeyNormalizer = Substitute.For<ILookupNormalizer>();
            var mockErrors = Substitute.For<IdentityErrorDescriber>();
            var mockServices = Substitute.For<IServiceProvider>();
            var mockUserLogger = Substitute.For<ILogger<UserManager<UserModel>>>();
            var fakeA3SContext = new A3SContextFake(new Microsoft.EntityFrameworkCore.DbContextOptions<A3SContext>());

            mockConfiguration = Substitute.For<IConfiguration>();
            var fakesCustomUserStore = new CustomUserStoreFake(fakeA3SContext, mockConfiguration);

            fakeUserManager = new CustomUserManagerFake(fakesCustomUserStore, mockOptionsAccessor, mockPasswordHasher, mockUserValidators, mockPasswordValidators, mockKeyNormalizer,
                mockErrors, mockServices, mockUserLogger);

            termsOfServiceRepository = Substitute.For<ITermsOfServiceRepository>();

            userModel = new UserModel()
            {
                UserName = "username",
                Id = Guid.NewGuid().ToString()
            };
        }

        [Fact]
        public async Task Index_Executed_ViewResultReturned()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            using var termsOfServiceController = new TermsOfServiceController(fakeUserManager, termsOfServiceRepository)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "example name"),
                            new Claim(ClaimTypes.NameIdentifier, id),
                            new Claim("sub", id),
                            new Claim("custom-claim", "example claim value"),
                        }, "mock")),
                    }
                }
            };

            fakeUserManager.SetUserModel(userModel);

            // Act
            var actionResult = await termsOfServiceController.Index(RETURN_URL);

            // Assert
            var viewResult = actionResult as ViewResult;
            Assert.NotNull(viewResult);
        }

        [Fact]
        public async Task Index_ExecutedWithNullUser_ThrowsException()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            using var termsOfServiceController = new TermsOfServiceController(fakeUserManager, termsOfServiceRepository)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "example name"),
                            new Claim(ClaimTypes.NameIdentifier, id),
                            new Claim("sub", id),
                            new Claim("custom-claim", "example claim value"),
                        }, "mock")),
                    }
                }
            };

            fakeUserManager.SetUserModel(null);

            // Act
            Exception caughtException = null;

            try
            {
                var actionResult = await termsOfServiceController.Index(RETURN_URL);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.True(caughtException is AuthenticationException, "Invalid login data must throw an AuthenticationException.");
        }
    }
}
