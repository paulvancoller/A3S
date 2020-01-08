/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.ConnectionClients;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;

namespace za.co.grindrodbank.a3s.tests.Services
{
    public class LdapConnectionService_Tests
    {
        private readonly UserModel mockUserModel;
        private readonly ILdapConnectionClient mockedLdapConnectionClient;

        public LdapConnectionService_Tests()
        {
            mockUserModel = new UserModel()
            {
                UserName = "Test User",
                FirstName = "Test",
                Surname = "User",
                LdapAuthenticationMode = new LdapAuthenticationModeModel()
                {
                    Id = Guid.NewGuid(),
                    HostName = "localhost",
                    Account = "admin",
                    Password = "admin_this_cannot_be_the_password",
                    BaseDn = "dc=bigbaobab,dc=org",
                    Port = 389,
                    IsLdaps = false
                }
            };

            mockedLdapConnectionClient = Substitute.For<ILdapConnectionClient>();
        }

        [Fact]
        public async Task Login_GivenUserModelAndPassword_ReturnsFailedSignInResult()
        {
            // Arrange
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var userRepository = Substitute.For<IUserRepository>();

            var ldapConnectionService = new LdapConnectionService(ldapAuthenticationModeRepository, userRepository, mockedLdapConnectionClient, mockedLdapConnectionClient);

            // Act
            var signInResult = await ldapConnectionService.Login(mockUserModel, "Password1#");

            // Assert
            Assert.False(signInResult.Succeeded, "Testing an LDAP login for should return failed.");
        }

        [Fact]
        public async Task CheckIfUserExist_GivenUserModelAndLdapAuthMode_ReturnsFalse()
        {
            // Arrange
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var userRepository = Substitute.For<IUserRepository>();

            ldapAuthenticationModeRepository.GetByIdAsync(mockUserModel.LdapAuthenticationMode.Id, Arg.Any<bool>(), Arg.Any<bool>()).Returns(mockUserModel.LdapAuthenticationMode);
            var ldapConnectionService = new LdapConnectionService(ldapAuthenticationModeRepository, userRepository, mockedLdapConnectionClient, mockedLdapConnectionClient);

            // Act
            var checkResult = await ldapConnectionService.CheckIfUserExist(mockUserModel.UserName, mockUserModel.LdapAuthenticationMode.Id);

            // Assert
            Assert.False(checkResult, "Checking if an LDAP user exist should return false.");
        }

        [Fact]
        public async Task CheckIfUserExist_GivenUnfindableLdapAuthMode_ThrowsItemNotFoundException()
        {
            // Arrange
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var userRepository = Substitute.For<IUserRepository>();

            var ldapConnectionService = new LdapConnectionService(ldapAuthenticationModeRepository, userRepository, mockedLdapConnectionClient, mockedLdapConnectionClient);

            // Act
            Exception caughtEx = null;
            try
            {
                var checkResult = await ldapConnectionService.CheckIfUserExist(mockUserModel.UserName, mockUserModel.LdapAuthenticationMode.Id);
            }
            catch (Exception ex)
            {
                caughtEx = ex;
            }

            // Assert
            Assert.True(caughtEx is ItemNotFoundException, "Attempted user check with an unfindable LdapAuthMode must throw an ItemNotFoundException.");
        }

        [Fact]
        public void TestLdapSettings_GivenUserModelAndLdapAuthMode_ReturnsTrue()
        {
            // Arrange
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var userRepository = Substitute.For<IUserRepository>();

            ldapAuthenticationModeRepository.GetByIdAsync(mockUserModel.LdapAuthenticationMode.Id, Arg.Any<bool>(), Arg.Any<bool>()).Returns(mockUserModel.LdapAuthenticationMode);
            var ldapConnectionService = new LdapConnectionService(ldapAuthenticationModeRepository, userRepository, mockedLdapConnectionClient, mockedLdapConnectionClient);

            // Act
            List<string> returnMessages = new List<string>();
            var testResult = ldapConnectionService.TestLdapSettings(mockUserModel.LdapAuthenticationMode, ref returnMessages);

            // Assert
            Assert.True(testResult, "Testing LDAP settings should return true.");
        }
    }
}
