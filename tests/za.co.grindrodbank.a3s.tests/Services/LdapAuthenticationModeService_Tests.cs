/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.MappingProfiles;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;
using AutoMapper;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Exceptions;

namespace za.co.grindrodbank.a3s.tests.Services
{
    public class LdapAuthenticationModeService_Tests
    {
        private readonly IMapper mapper;
        private readonly Guid authenticationModeGuid;
        private readonly LdapAuthenticationModeModel mockedAuthenticationMode;
        private readonly LdapAuthenticationModeSubmit mockedAuthenticationModeSubmit;

        public LdapAuthenticationModeService_Tests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new LdapAuthenticationModeResourceLdapAuthenticationModeModelProfile());
                cfg.AddProfile(new LdapAuthenticationModeSubmitResourceLdapAuthenticationModeModelProfile());
            });

            mapper = config.CreateMapper();
            authenticationModeGuid = Guid.NewGuid();

            mockedAuthenticationMode = new LdapAuthenticationModeModel
            {
                Id = authenticationModeGuid,
                Name = "Test AuthenticationMode Name",
                Account = "TestAccount",
                BaseDn = "TestBaseDN",
                HostName = "TestHostName",
                IsLdaps = true,
                Password = "TestPass",
                Port = 389,
                Users = new List<UserModel>()
                {
                    new UserModel(),
                    new UserModel()
                }
            };

            mockedAuthenticationModeSubmit = new LdapAuthenticationModeSubmit()
            {
                Uuid = mockedAuthenticationMode.Id,
                Name = mockedAuthenticationMode.Name,
                Account = mockedAuthenticationMode.Account,
                BaseDn = mockedAuthenticationMode.BaseDn,
                HostName = mockedAuthenticationMode.HostName,
                IsLdaps = mockedAuthenticationMode.IsLdaps,
                Password = mockedAuthenticationMode.Password,
                Port = mockedAuthenticationMode.Port
            };
        }

        [Fact]
        public async Task GetById_GivenGuid_ReturnsAuthenticationModeResource()
        {
            // Arrange
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var ldapConnectionService = Substitute.For<ILdapConnectionService>();

            ldapAuthenticationModeRepository.GetByIdAsync(authenticationModeGuid).Returns(mockedAuthenticationMode);

            var ldapAuthenticationModeService = new LdapAuthenticationModeService(ldapAuthenticationModeRepository, mapper, ldapConnectionService);

            // Act
            var ldapAuthenticationModeResource = await ldapAuthenticationModeService.GetByIdAsync(authenticationModeGuid);

            // Assert
            Assert.True(ldapAuthenticationModeResource.Name == mockedAuthenticationMode.Name, $"Expected authenticationMode name: '{ldapAuthenticationModeResource.Name}' does not equal expected value: '{mockedAuthenticationMode.Name}'");
            Assert.True(ldapAuthenticationModeResource.Uuid == authenticationModeGuid, $"Expected authenticationMode UUID: '{ldapAuthenticationModeResource.Uuid}' does not equal expected value: '{authenticationModeGuid}'");
            Assert.True(ldapAuthenticationModeResource.Account == mockedAuthenticationMode.Account, $"Expected authenticationMode name: '{ldapAuthenticationModeResource.Account}' does not equal expected value: '{mockedAuthenticationMode.Account}'");
            Assert.True(ldapAuthenticationModeResource.BaseDn == mockedAuthenticationMode.BaseDn, $"Expected authenticationMode name: '{ldapAuthenticationModeResource.BaseDn}' does not equal expected value: '{mockedAuthenticationMode.BaseDn}'");
            Assert.True(ldapAuthenticationModeResource.HostName == mockedAuthenticationMode.HostName, $"Expected authenticationMode name: '{ldapAuthenticationModeResource.HostName}' does not equal expected value: '{mockedAuthenticationMode.HostName}'");
            Assert.True(ldapAuthenticationModeResource.IsLdaps == mockedAuthenticationMode.IsLdaps, $"Expected authenticationMode name: '{ldapAuthenticationModeResource.IsLdaps}' does not equal expected value: '{mockedAuthenticationMode.IsLdaps}'");
            Assert.True(ldapAuthenticationModeResource.Port == mockedAuthenticationMode.Port, $"Expected authenticationMode name: '{ldapAuthenticationModeResource.Port}' does not equal expected value: '{mockedAuthenticationMode.Port}'");
        }

        [Fact]
        public async Task CreateAsync_GivenFullProcessableModel_ReturnsCreatedModel()
        {
            // Arrange
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var ldapConnectionService = Substitute.For<ILdapConnectionService>();

            ldapAuthenticationModeRepository.CreateAsync(Arg.Any<LdapAuthenticationModeModel>()).Returns(mockedAuthenticationMode);

            var ldapAuthenticationModeService = new LdapAuthenticationModeService(ldapAuthenticationModeRepository, mapper, ldapConnectionService);

            // Act
            var ldapAuthenticationModeResource = await ldapAuthenticationModeService.CreateAsync(mockedAuthenticationModeSubmit, Guid.NewGuid());

            // Assert
            Assert.True(ldapAuthenticationModeResource.Name == mockedAuthenticationModeSubmit.Name, $"Resource Name: '{ldapAuthenticationModeResource.Name}' not the expected value: '{mockedAuthenticationModeSubmit.Name}'");
            Assert.True(ldapAuthenticationModeResource.Account == mockedAuthenticationModeSubmit.Account, $"Resource Account: '{ldapAuthenticationModeResource.Account}' not the expected value: '{mockedAuthenticationModeSubmit.Account}'");
            Assert.True(ldapAuthenticationModeResource.BaseDn == mockedAuthenticationModeSubmit.BaseDn, $"Resource BaseDn: '{ldapAuthenticationModeResource.BaseDn}' not the expected value: '{mockedAuthenticationModeSubmit.BaseDn}'");
            Assert.True(ldapAuthenticationModeResource.HostName == mockedAuthenticationModeSubmit.HostName, $"Resource HostName: '{ldapAuthenticationModeResource.HostName}' not the expected value: '{mockedAuthenticationModeSubmit.HostName}'");
            Assert.True(ldapAuthenticationModeResource.IsLdaps == mockedAuthenticationModeSubmit.IsLdaps, $"Resource IsLdaps: '{ldapAuthenticationModeResource.IsLdaps}' not the expected value: '{mockedAuthenticationModeSubmit.IsLdaps}'");
            Assert.True(ldapAuthenticationModeResource.Port == mockedAuthenticationModeSubmit.Port, $"Resource Port: '{ldapAuthenticationModeResource.Port}' not the expected value: '{mockedAuthenticationModeSubmit.Port}'");
        }

        [Fact]
        public async Task CreateAsync_GivenAlreadyUsedName_ThrowsItemNotProcessableException()
        {
            // Arrange
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var ldapConnectionService = Substitute.For<ILdapConnectionService>();

            ldapAuthenticationModeRepository.CreateAsync(Arg.Any<LdapAuthenticationModeModel>()).Returns(mockedAuthenticationMode);
            ldapAuthenticationModeRepository.GetByNameAsync(mockedAuthenticationModeSubmit.Name).Returns(mockedAuthenticationMode);

            var authenticationModeService = new LdapAuthenticationModeService(ldapAuthenticationModeRepository, mapper, ldapConnectionService);

            // Act
            Exception caughtEx = null;
            try
            {
                var authenticationModeResource = await authenticationModeService.CreateAsync(mockedAuthenticationModeSubmit, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughtEx = ex;
            }

            // Assert
            Assert.True(caughtEx is ItemNotProcessableException, "Attempted create with an already used name must throw an ItemNotProcessableException.");
        }

        [Fact]
        public async Task UpdateAsync_GivenFullProcessableModel_ReturnsCreatedModel()
        {
            // Arrange
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var ldapConnectionService = Substitute.For<ILdapConnectionService>();

            ldapAuthenticationModeRepository.UpdateAsync(Arg.Any<LdapAuthenticationModeModel>()).Returns(mockedAuthenticationMode);
            ldapAuthenticationModeRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(mockedAuthenticationMode);

            var ldapAuthenticationModeService = new LdapAuthenticationModeService(ldapAuthenticationModeRepository, mapper, ldapConnectionService);

            // Act
            var ldapAuthenticationModeResource = await ldapAuthenticationModeService.UpdateAsync(mockedAuthenticationModeSubmit, Guid.NewGuid());

            // Assert
            Assert.True(ldapAuthenticationModeResource.Uuid == mockedAuthenticationModeSubmit.Uuid, $"Resource Uuid: '{ldapAuthenticationModeResource.Uuid}' not the expected value: '{mockedAuthenticationModeSubmit.Uuid}'");
            Assert.True(ldapAuthenticationModeResource.Name == mockedAuthenticationModeSubmit.Name, $"Resource Name: '{ldapAuthenticationModeResource.Name}' not the expected value: '{mockedAuthenticationModeSubmit.Name}'");
            Assert.True(ldapAuthenticationModeResource.Account == mockedAuthenticationModeSubmit.Account, $"Resource Account: '{ldapAuthenticationModeResource.Account}' not the expected value: '{mockedAuthenticationModeSubmit.Account}'");
            Assert.True(ldapAuthenticationModeResource.BaseDn == mockedAuthenticationModeSubmit.BaseDn, $"Resource BaseDn: '{ldapAuthenticationModeResource.BaseDn}' not the expected value: '{mockedAuthenticationModeSubmit.BaseDn}'");
            Assert.True(ldapAuthenticationModeResource.HostName == mockedAuthenticationModeSubmit.HostName, $"Resource HostName: '{ldapAuthenticationModeResource.HostName}' not the expected value: '{mockedAuthenticationModeSubmit.HostName}'");
            Assert.True(ldapAuthenticationModeResource.IsLdaps == mockedAuthenticationModeSubmit.IsLdaps, $"Resource IsLdaps: '{ldapAuthenticationModeResource.IsLdaps}' not the expected value: '{mockedAuthenticationModeSubmit.IsLdaps}'");
            Assert.True(ldapAuthenticationModeResource.Port == mockedAuthenticationModeSubmit.Port, $"Resource Port: '{ldapAuthenticationModeResource.Port}' not the expected value: '{mockedAuthenticationModeSubmit.Port}'");
        }

        [Fact]
        public async Task UpdateAsync_GivenUnfindableId_ThrowsItemNotFoundException()
        {
            // Arrange
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var ldapConnectionService = Substitute.For<ILdapConnectionService>();

            ldapAuthenticationModeRepository.UpdateAsync(Arg.Any<LdapAuthenticationModeModel>()).Returns(mockedAuthenticationMode);

            var authenticationModeService = new LdapAuthenticationModeService(ldapAuthenticationModeRepository, mapper, ldapConnectionService);

            // Act
            Exception caughtEx = null;
            try
            {
                var authenticationModeResource = await authenticationModeService.UpdateAsync(mockedAuthenticationModeSubmit, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughtEx = ex;
            }

            // Assert
            Assert.True(caughtEx is ItemNotFoundException, "Attempted update with an unfindable Id must throw an ItemNotFoundException.");
        }

        [Fact]
        public async Task UpdateAsync_GivenAlreadyUsedName_ThrowsItemNotProcessableException()
        {
            // Arrange
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var ldapConnectionService = Substitute.For<ILdapConnectionService>();

            mockedAuthenticationModeSubmit.Name += "_changed_name";

            ldapAuthenticationModeRepository.UpdateAsync(Arg.Any<LdapAuthenticationModeModel>()).Returns(mockedAuthenticationMode);
            ldapAuthenticationModeRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(mockedAuthenticationMode);
            ldapAuthenticationModeRepository.GetByNameAsync(mockedAuthenticationModeSubmit.Name).Returns(mockedAuthenticationMode);

            var authenticationModeService = new LdapAuthenticationModeService(ldapAuthenticationModeRepository, mapper, ldapConnectionService);

            // Act
            Exception caughtEx = null;
            try
            {
                var authenticationModeResource = await authenticationModeService.UpdateAsync(mockedAuthenticationModeSubmit, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughtEx = ex;
            }

            // Assert
            Assert.True(caughtEx is ItemNotProcessableException, "Attempted create with an already used name must throw an ItemNotProcessableException.");
        }

        [Fact]
        public async Task GetListAsync_Executed_ReturnsList()
        {
            // Arrange
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var ldapConnectionService = Substitute.For<ILdapConnectionService>();

            ldapAuthenticationModeRepository.GetListAsync().Returns(
                new List<LdapAuthenticationModeModel>()
                {
                    mockedAuthenticationMode,
                    mockedAuthenticationMode
                });

            var ldapAuthenticationModeService = new LdapAuthenticationModeService(ldapAuthenticationModeRepository, mapper, ldapConnectionService);

            // Act

            // Act
            var ldapAuthenticationModeList = await ldapAuthenticationModeService.GetListAsync();

            // Assert
            Assert.True(ldapAuthenticationModeList.Count == 2, "Expected list count is 2");
            Assert.True(ldapAuthenticationModeList[0].Name == mockedAuthenticationMode.Name, $"Expected authenticationMode name: '{ldapAuthenticationModeList[0].Name}' does not equal expected value: '{mockedAuthenticationMode.Name}'");
            Assert.True(ldapAuthenticationModeList[0].Uuid == authenticationModeGuid, $"Expected authenticationMode UUID: '{ldapAuthenticationModeList[0].Uuid}' does not equal expected value: '{authenticationModeGuid}'");
            Assert.True(ldapAuthenticationModeList[0].Account == mockedAuthenticationMode.Account, $"Expected authenticationMode name: '{ldapAuthenticationModeList[0].Account}' does not equal expected value: '{mockedAuthenticationMode.Account}'");
            Assert.True(ldapAuthenticationModeList[0].BaseDn == mockedAuthenticationMode.BaseDn, $"Expected authenticationMode name: '{ldapAuthenticationModeList[0].BaseDn}' does not equal expected value: '{mockedAuthenticationMode.BaseDn}'");
            Assert.True(ldapAuthenticationModeList[0].HostName == mockedAuthenticationMode.HostName, $"Expected authenticationMode name: '{ldapAuthenticationModeList[0].HostName}' does not equal expected value: '{mockedAuthenticationMode.HostName}'");
            Assert.True(ldapAuthenticationModeList[0].IsLdaps == mockedAuthenticationMode.IsLdaps, $"Expected authenticationMode name: '{ldapAuthenticationModeList[0].IsLdaps}' does not equal expected value: '{mockedAuthenticationMode.IsLdaps}'");
            Assert.True(ldapAuthenticationModeList[0].Port == mockedAuthenticationMode.Port, $"Expected authenticationMode name: '{ldapAuthenticationModeList[0].Port}' does not equal expected value: '{mockedAuthenticationMode.Port}'");
            Assert.True(ldapAuthenticationModeList[1].Name == mockedAuthenticationMode.Name, $"Expected authenticationMode name: '{ldapAuthenticationModeList[1].Name}' does not equal expected value: '{mockedAuthenticationMode.Name}'");
            Assert.True(ldapAuthenticationModeList[1].Uuid == authenticationModeGuid, $"Expected authenticationMode UUID: '{ldapAuthenticationModeList[1].Uuid}' does not equal expected value: '{authenticationModeGuid}'");
            Assert.True(ldapAuthenticationModeList[1].Account == mockedAuthenticationMode.Account, $"Expected authenticationMode name: '{ldapAuthenticationModeList[1].Account}' does not equal expected value: '{mockedAuthenticationMode.Account}'");
            Assert.True(ldapAuthenticationModeList[1].BaseDn == mockedAuthenticationMode.BaseDn, $"Expected authenticationMode name: '{ldapAuthenticationModeList[1].BaseDn}' does not equal expected value: '{mockedAuthenticationMode.BaseDn}'");
            Assert.True(ldapAuthenticationModeList[1].HostName == mockedAuthenticationMode.HostName, $"Expected authenticationMode name: '{ldapAuthenticationModeList[1].HostName}' does not equal expected value: '{mockedAuthenticationMode.HostName}'");
            Assert.True(ldapAuthenticationModeList[1].IsLdaps == mockedAuthenticationMode.IsLdaps, $"Expected authenticationMode name: '{ldapAuthenticationModeList[1].IsLdaps}' does not equal expected value: '{mockedAuthenticationMode.IsLdaps}'");
            Assert.True(ldapAuthenticationModeList[1].Port == mockedAuthenticationMode.Port, $"Expected authenticationMode name: '{ldapAuthenticationModeList[1].Port}' does not equal expected value: '{mockedAuthenticationMode.Port}'");
        }

        [Fact]
        public async Task DeleteAsync_GivenFindableGuidWithNoUsersAttached_ExecutesSuccessfully()
        {
            // Arrange
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var ldapConnectionService = Substitute.For<ILdapConnectionService>();

            mockedAuthenticationMode.Users = new List<UserModel>();
            ldapAuthenticationModeRepository.GetByIdAsync(mockedAuthenticationMode.Id, Arg.Any<bool>(), Arg.Any<bool>()).Returns(mockedAuthenticationMode);

            var ldapAuthenticationModeService = new LdapAuthenticationModeService(ldapAuthenticationModeRepository, mapper, ldapConnectionService);

            // Act
            Exception caughtEx = null;
            try
            {
                await ldapAuthenticationModeService.DeleteAsync(mockedAuthenticationMode.Id);
            }
            catch (Exception ex)
            {
                caughtEx = ex;
            }

            // Assert
            Assert.True(caughtEx is null, "Delete on a findable GUID must execute successfully.");
        }

        [Fact]
        public async Task DeleteAsync_GivenUnfindableGuid_ThrowsItemNotFoundException()
        {
            // Arrange
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var ldapConnectionService = Substitute.For<ILdapConnectionService>();

            var ldapAuthenticationModeService = new LdapAuthenticationModeService(ldapAuthenticationModeRepository, mapper, ldapConnectionService);

            // Act
            Exception caughtEx = null;
            try
            {
                await ldapAuthenticationModeService.DeleteAsync(mockedAuthenticationMode.Id);
            }
            catch (Exception ex)
            {
                caughtEx = ex;
            }

            // Assert
            Assert.True(caughtEx is ItemNotFoundException, "Delete on an unfindable GUID must throw an ItemNotFoundException.");
        }

        [Fact]
        public async Task DeleteAsync_GivenFindableGuidWithUsersAttached_ThrowsItemNotProcessableException()
        {
            // Arrange
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var ldapConnectionService = Substitute.For<ILdapConnectionService>();

            ldapAuthenticationModeRepository.GetByIdAsync(mockedAuthenticationMode.Id, Arg.Any<bool>(), Arg.Any<bool>()).Returns(mockedAuthenticationMode);

            var ldapAuthenticationModeService = new LdapAuthenticationModeService(ldapAuthenticationModeRepository, mapper, ldapConnectionService);

            // Act
            Exception caughtEx = null;
            try
            {
                await ldapAuthenticationModeService.DeleteAsync(mockedAuthenticationMode.Id);
            }
            catch (Exception ex)
            {
                caughtEx = ex;
            }

            // Assert
            Assert.True(caughtEx is ItemNotProcessableException, "Delete on a findable GUID with users still attached must throw an ItemNotProcessableException.");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TestAsync_GivenFullProcessableModel_ReturnsTestResultsl(bool ldapTestResult)
        {
            // Arrange
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var ldapConnectionService = Substitute.For<ILdapConnectionService>();

            ldapConnectionService.TestLdapSettings(Arg.Any<LdapAuthenticationModeModel>(), ref Arg.Any<List<string>>()).Returns(ldapTestResult);

            var ldapAuthenticationModeService = new LdapAuthenticationModeService(ldapAuthenticationModeRepository, mapper, ldapConnectionService);

            // Act
            var testResults = await ldapAuthenticationModeService.TestAsync(mockedAuthenticationModeSubmit);

            // Assert
            Assert.True(testResults.Success == ldapTestResult, $"Test result Success: '{testResults.Success}' not the expected value: '{ldapTestResult}'");
        }
    }
}
