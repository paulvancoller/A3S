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
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;
using za.co.grindrodbank.a3s.tests.Fakes;

namespace za.co.grindrodbank.a3s.tests.Services
{
    public class TwoFactorAuthService_Tests
    {
        private readonly CustomUserManagerFake customUserManagerFake;
        private readonly UserModel mockedUserModel;
        private readonly TwoFactorAuthOTP twoFactorAuthOTP;

        public TwoFactorAuthService_Tests()
        {
            // Arrange
            var mockOptionsAccessor = Substitute.For<IOptions<IdentityOptions>>();
            var mockPasswordHasher = Substitute.For<IPasswordHasher<UserModel>>();
            var mockUserValidators = Substitute.For<IEnumerable<IUserValidator<UserModel>>>();
            var mockPasswordValidators = Substitute.For<IEnumerable<IPasswordValidator<UserModel>>>();
            var mockKeyNormalizer = Substitute.For<ILookupNormalizer>();
            var mockErrors = Substitute.For<IdentityErrorDescriber>();
            var mockServices = Substitute.For<IServiceProvider>();
            var mockUserLogger = Substitute.For<ILogger<UserManager<UserModel>>>();
            var mockConfiguration = Substitute.For<IConfiguration>();
            var a3SContextFake = new A3SContextFake(new Microsoft.EntityFrameworkCore.DbContextOptions<A3SContext>());
            var customUserStoreFake = new CustomUserStoreFake(a3SContextFake, mockConfiguration);

            customUserManagerFake = new CustomUserManagerFake(customUserStoreFake, mockOptionsAccessor, mockPasswordHasher, mockUserValidators, mockPasswordValidators, mockKeyNormalizer,
                mockErrors, mockServices, mockUserLogger);

            mockedUserModel = new UserModel()
            {
                Id = Guid.NewGuid().ToString(),
                UserTokens = new List<UserTokenModel>()
                    {
                        new UserTokenModel(),
                        new UserTokenModel(),
                        new UserTokenModel(),
                    }
            };

            twoFactorAuthOTP = new TwoFactorAuthOTP()
            {
                UserId = Guid.NewGuid(),
                OTP = "232020"
            };


        }

        [Fact]
        public async Task RemoveTwoFactorAuthenticationAsync_GivenWrongGuid_ThrowsNotFoundException()
        {
            var userRepository = Substitute.For<IUserRepository>();
            var twoFactorAuthService = new TwoFactorAuthService(userRepository, customUserManagerFake);

            Exception catchingException = null;
            try
            {
                await twoFactorAuthService.RemoveTwoFactorAuthenticationAsync(Guid.NewGuid());
            }
            catch (Exception ex)
            {
                catchingException = ex;
            }

            Assert.True(catchingException is ItemNotFoundException);
        }

        [Fact]
        public async Task RemoveTwoFactorAuthenticationAsync_GivenEmptyGuid_ThrowsNotFoundException()
        {
            var userRepository = Substitute.For<IUserRepository>();
            var twoFactorAuthService = new TwoFactorAuthService(userRepository, customUserManagerFake);

            Exception catchingException = null;
            try
            {
                await twoFactorAuthService.RemoveTwoFactorAuthenticationAsync(Guid.Empty);
            }
            catch (Exception ex)
            {
                catchingException = ex;
            }

            Assert.True(catchingException is ItemNotFoundException);
        }

        [Fact]
        public async Task RemoveTwoFactorAuthenticationAsync_GivenCorrectGuid_ThrowsNoException()
        {
            var userRepository = Substitute.For<IUserRepository>();
            var twoFactorAuthService = new TwoFactorAuthService(userRepository, customUserManagerFake);

            userRepository.GetByIdAsync(Guid.Parse(mockedUserModel.Id), Arg.Any<bool>()).Returns(mockedUserModel);

            var testGuid = Guid.NewGuid();
            userRepository.GetByIdAsync(testGuid, Arg.Any<bool>()).Returns(
                new UserModel()
                {
                    Id = testGuid.ToString(),
                    UserTokens = new List<UserTokenModel>()
                    {
                        new UserTokenModel(),
                        new UserTokenModel(),
                        new UserTokenModel(),
                    }
                });

            await twoFactorAuthService.RemoveTwoFactorAuthenticationAsync(testGuid);

            Exception catchingException = null;
            try
            {
                await twoFactorAuthService.RemoveTwoFactorAuthenticationAsync(Guid.Parse(mockedUserModel.Id));
            }
            catch (Exception ex)
            {
                catchingException = ex;
                Assert.True(false);
            }

            Assert.True(catchingException is null);
        }

        [Fact]
        public async Task ValidateTwoFactorAuthenticationOTPAsync_GivenCorrectOTPandFindableUser_ReturnsSuccess()
        {
            var userRepository = Substitute.For<IUserRepository>();
            var twoFactorAuthService = new TwoFactorAuthService(userRepository, customUserManagerFake);

            userRepository.GetByIdAsync(twoFactorAuthOTP.UserId, Arg.Any<bool>()).Returns(mockedUserModel);
            customUserManagerFake.SetAuthenticatorTokenVerified(true);
            customUserManagerFake.SetAuthenticatorOtpValid(true);

            ValidationResultResponse validationResultResponse = await twoFactorAuthService.ValidateTwoFactorAuthenticationOTPAsync(twoFactorAuthOTP);

            Assert.True(validationResultResponse.Success, "Giving correct OTP and findable user should return success true.");
        }

        [Fact]
        public async Task ValidateTwoFactorAuthenticationOTPAsync_GivenUnfindableUser_ThrowsNotFoundException()
        {
            var userRepository = Substitute.For<IUserRepository>();
            var twoFactorAuthService = new TwoFactorAuthService(userRepository, customUserManagerFake);

            customUserManagerFake.SetAuthenticatorTokenVerified(true);
            customUserManagerFake.SetAuthenticatorOtpValid(true);

            Exception catchingException = null;
            try
            {
                await twoFactorAuthService.ValidateTwoFactorAuthenticationOTPAsync(twoFactorAuthOTP);
            }
            catch (Exception ex)
            {
                catchingException = ex;
            }

            Assert.True(catchingException is ItemNotFoundException, "Giving unfindable user must throw ItemNotFoundException.");
        }

        [Fact]
        public async Task ValidateTwoFactorAuthenticationOTPAsync_GivenIncorrectOTPandFindableUser_ReturnsFailed()
        {
            var userRepository = Substitute.For<IUserRepository>();
            var twoFactorAuthService = new TwoFactorAuthService(userRepository, customUserManagerFake);

            userRepository.GetByIdAsync(twoFactorAuthOTP.UserId, Arg.Any<bool>()).Returns(mockedUserModel);
            customUserManagerFake.SetAuthenticatorTokenVerified(true);
            customUserManagerFake.SetAuthenticatorOtpValid(false);

            ValidationResultResponse validationResultResponse = await twoFactorAuthService.ValidateTwoFactorAuthenticationOTPAsync(twoFactorAuthOTP);

            Assert.False(validationResultResponse.Success, "Giving incorrect OTP and findable user must return success false.");
        }

        [Fact]
        public async Task ValidateTwoFactorAuthenticationOTPAsync_GivenUnverifiedTokenandFindableUser_ReturnsFailed()
        {
            var userRepository = Substitute.For<IUserRepository>();
            var twoFactorAuthService = new TwoFactorAuthService(userRepository, customUserManagerFake);

            userRepository.GetByIdAsync(twoFactorAuthOTP.UserId, Arg.Any<bool>()).Returns(mockedUserModel);
            customUserManagerFake.SetAuthenticatorTokenVerified(false);
            customUserManagerFake.SetAuthenticatorOtpValid(true);

            ValidationResultResponse validationResultResponse = await twoFactorAuthService.ValidateTwoFactorAuthenticationOTPAsync(twoFactorAuthOTP);

            Assert.False(validationResultResponse.Success, "Giving unverfied token and findable user must return success false.");
        }
    }
}
