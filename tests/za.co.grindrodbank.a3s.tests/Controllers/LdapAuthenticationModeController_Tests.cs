/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Controllers;
using za.co.grindrodbank.a3s.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.A3SApiResources;

namespace za.co.grindrodbank.a3s.tests.Controllers
{
    public class LdapAuthenticationModeController_Tests
    {
        [Fact]
        public async Task GetAuthenticationModeAsync_WithEmptyGuid_ReturnsNotFoundResult()
        {
            // Arrange
            var authenticationModeService = Substitute.For<ILdapAuthenticationModeService>();
            var controller = new LdapAuthenticationModeController(authenticationModeService);

            // Act
            var actionResult = await controller.GetLdapAuthenticationModeAsync(Guid.Empty);

            // Assert
            Assert.IsType<NotFoundResult>(actionResult);
        }

        [Fact]
        public async Task GetAuthenticationModeAsync_WithRandomGuid_ReturnsNotFoundResult()
        {
            // Arrange
            var authenticationModeService = Substitute.For<ILdapAuthenticationModeService>();
            var controller = new LdapAuthenticationModeController(authenticationModeService);

            // Act
            var result = await controller.GetLdapAuthenticationModeAsync(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAuthenticationModeAsync_WithTestGuid_ReturnsCorrectResult()
        {
            // Arrange
            var authenticationModeService = Substitute.For<ILdapAuthenticationModeService>();
            var testGuid = Guid.NewGuid();
            var testName = "TestAuthenticationModeName";

            authenticationModeService.GetByIdAsync(testGuid).Returns(new LdapAuthenticationMode { Uuid = testGuid, Name = testName });

            var controller = new LdapAuthenticationModeController(authenticationModeService);

            // Act
            IActionResult actionResult = await controller.GetLdapAuthenticationModeAsync(testGuid);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var authenticationMode = okResult.Value as LdapAuthenticationMode;
            Assert.NotNull(authenticationMode);
            Assert.True(authenticationMode.Uuid == testGuid, $"Retrieved Id {authenticationMode.Uuid} not the same as sample id {testGuid}.");
            Assert.True(authenticationMode.Name == testName, $"Retrieved Name {authenticationMode.Name} not the same as sample id {testName}.");
        }

        [Fact]
        public async Task ListAuthenticationModesAsync_WithNoInputs_ReturnsList()
        {
            // Arrange
            var authenticationModeService = Substitute.For<ILdapAuthenticationModeService>();

            var inList = new List<LdapAuthenticationMode>();
            inList.Add(new LdapAuthenticationMode { Name = "Test AuthenticationModes 1", Uuid = Guid.NewGuid() });
            inList.Add(new LdapAuthenticationMode { Name = "Test AuthenticationModes 2", Uuid = Guid.NewGuid() });
            inList.Add(new LdapAuthenticationMode { Name = "Test AuthenticationModes 3", Uuid = Guid.NewGuid() });

            authenticationModeService.GetListAsync().Returns(inList);

            var controller = new LdapAuthenticationModeController(authenticationModeService);

            // Act
            IActionResult actionResult = await controller.ListLdapAuthenticationModesAsync(null);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var outList = okResult.Value as List<LdapAuthenticationMode>;
            Assert.NotNull(outList);

            for (var i = 0; i < outList.Count; i++)
            {
                Assert.Equal(outList[i].Uuid, inList[i].Uuid);
                Assert.Equal(outList[i].Name, inList[i].Name);
            }
        }

        [Fact]
        public async Task UpdateAuthenticationModeAsync_WithEmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            var authenticationModeService = Substitute.For<ILdapAuthenticationModeService>();
            var controller = new LdapAuthenticationModeController(authenticationModeService);

            // Act
            IActionResult actionResult = await controller.UpdateLdapAuthenticationModeAsync(Guid.Empty, null);

            // Assert
            var badRequestResult = actionResult as BadRequestResult;
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task UpdateAuthenticationModeAsync_WithTestAuthenticationMode_ReturnsUpdatedAuthenticationMode()
        {
            // Arrange
            var authenticationModeService = Substitute.For<ILdapAuthenticationModeService>();
            var inputModel = new LdapAuthenticationModeSubmit()
            {
                Uuid = Guid.NewGuid(),
                Name = "Test AuthenticationMode Name",
                Account = "TestAccount",
                BaseDn = "TestBaseDN",
                HostName = "TestHostName",
                IsLdaps = true,
                Password = "TestPass",
                Port = 389
            };

            authenticationModeService.UpdateAsync(inputModel, Arg.Any<Guid>())
                .Returns(new LdapAuthenticationMode()
                {
                    Uuid = inputModel.Uuid,
                    Name = inputModel.Name,
                    Account = inputModel.Account,
                    BaseDn = inputModel.BaseDn,
                    HostName = inputModel.HostName,
                    IsLdaps = inputModel.IsLdaps,
                    Port = inputModel.Port
                }
                );

            var controller = new LdapAuthenticationModeController(authenticationModeService);

            // Act
            IActionResult actionResult = await controller.UpdateLdapAuthenticationModeAsync(inputModel.Uuid, inputModel);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var authenticationMode = okResult.Value as LdapAuthenticationMode;
            Assert.NotNull(authenticationMode);
            Assert.True(authenticationMode.Uuid == inputModel.Uuid, $"Retrieved Id {authenticationMode.Uuid} not the same as sample id {inputModel.Uuid}.");
            Assert.True(authenticationMode.Name == inputModel.Name, $"Retrieved Name {authenticationMode.Name} not the same as sample Name {inputModel.Name}.");
            Assert.True(authenticationMode.Account == inputModel.Account, $"Retrieved Account {authenticationMode.Account} not the same as sample Account {inputModel.Account}.");
            Assert.True(authenticationMode.BaseDn == inputModel.BaseDn, $"Retrieved BaseDn {authenticationMode.BaseDn} not the same as sample Name {inputModel.BaseDn}.");
            Assert.True(authenticationMode.HostName == inputModel.HostName, $"Retrieved HostName {authenticationMode.HostName} not the same as sample HostName {inputModel.HostName}.");
            Assert.True(authenticationMode.IsLdaps == inputModel.IsLdaps, $"Retrieved Name {authenticationMode.IsLdaps} not the same as sample Ldaps {inputModel.IsLdaps}.");
            Assert.True(authenticationMode.Port == inputModel.Port, $"Retrieved Port {authenticationMode.Port} not the same as sample Port {inputModel.Port}.");
        }

        [Fact]
        public async Task CreateAuthenticationModeAsync_WithTestAuthenticationMode_ReturnsUpdatedAuthenticationMode()
        {
            // Arrange
            var authenticationModeService = Substitute.For<ILdapAuthenticationModeService>();
            var inputModel = new LdapAuthenticationModeSubmit()
            {
                Uuid = Guid.NewGuid(),
                Name = "Test AuthenticationMode Name",
                Account = "TestAccount",
                BaseDn = "TestBaseDN",
                HostName = "TestHostName",
                IsLdaps = true,
                Password = "TestPass",
                Port = 389
            };

            authenticationModeService.CreateAsync(inputModel, Arg.Any<Guid>())
                .Returns(new LdapAuthenticationMode()
                {
                    Uuid = inputModel.Uuid,
                    Name = inputModel.Name,
                    Account = inputModel.Account,
                    BaseDn = inputModel.BaseDn,
                    HostName = inputModel.HostName,
                    IsLdaps = inputModel.IsLdaps,
                    Port = inputModel.Port
                }
                );

            var controller = new LdapAuthenticationModeController(authenticationModeService);

            // Act
            IActionResult actionResult = await controller.CreateLdapAuthenticationModeAsync(inputModel);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var authenticationMode = okResult.Value as LdapAuthenticationMode;
            Assert.NotNull(authenticationMode);
            Assert.True(authenticationMode.Uuid == inputModel.Uuid, $"Retrieved Id {authenticationMode.Uuid} not the same as sample id {inputModel.Uuid}.");
            Assert.True(authenticationMode.Name == inputModel.Name, $"Retrieved Name {authenticationMode.Name} not the same as sample Name {inputModel.Name}.");
            Assert.True(authenticationMode.Account == inputModel.Account, $"Retrieved Account {authenticationMode.Account} not the same as sample Account {inputModel.Account}.");
            Assert.True(authenticationMode.BaseDn == inputModel.BaseDn, $"Retrieved BaseDn {authenticationMode.BaseDn} not the same as sample Name {inputModel.BaseDn}.");
            Assert.True(authenticationMode.HostName == inputModel.HostName, $"Retrieved HostName {authenticationMode.HostName} not the same as sample HostName {inputModel.HostName}.");
            Assert.True(authenticationMode.IsLdaps == inputModel.IsLdaps, $"Retrieved Name {authenticationMode.IsLdaps} not the same as sample Ldaps {inputModel.IsLdaps}.");
            Assert.True(authenticationMode.Port == inputModel.Port, $"Retrieved Port {authenticationMode.Port} not the same as sample Port {inputModel.Port}.");
        }

        [Fact]
        public async Task TestAuthenticationModeAsync_WithTestAuthenticationMode_ReturnsUpdatedAuthenticationMode()
        {
            // Arrange
            var authenticationModeService = Substitute.For<ILdapAuthenticationModeService>();
            var inputModel = new LdapAuthenticationModeSubmit()
            {
                Uuid = Guid.NewGuid(),
                Name = "Test AuthenticationMode Name",
                Account = "TestAccount",
                BaseDn = "TestBaseDN",
                HostName = "TestHostName",
                IsLdaps = true,
                Password = "TestPass",
                Port = 389
            };

            authenticationModeService.TestAsync(inputModel)
                .Returns(new ValidationResultResponse()
                {
                    Success = false,
                    Messages = new List<string>()
                    {
                        "123",
                        "456"
                    }
                });

            var controller = new LdapAuthenticationModeController(authenticationModeService);

            // Act
            IActionResult actionResult = await controller.TestLdapAuthenticationModeAsync(inputModel);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var testResult = okResult.Value as ValidationResultResponse;
            Assert.NotNull(testResult);
            Assert.True(testResult.Success == false, $"Retrieved Success {testResult.Success} not the same as sample 'false'.");
            Assert.True(testResult.Messages[0] == "123", $"Retrieved Message {testResult.Messages[0]} not the same as sample message '123'.");
            Assert.True(testResult.Messages[1] == "456", $"Retrieved Message {testResult.Messages[1]} not the same as sample message '456'.");
        }

        [Fact]
        public async Task DeleteLdapAuthenticationModeAsync_WithId_ReturnsNoContent()
        {
            // Arrange
            var authenticationModeService = Substitute.For<ILdapAuthenticationModeService>();
            var controller = new LdapAuthenticationModeController(authenticationModeService);

            // Act
            IActionResult actionResult = await controller.DeleteLdapAuthenticationModeAsync(Guid.NewGuid());

            // Assert
            var noContentResult = actionResult as NoContentResult;
            Assert.NotNull(noContentResult);
        }
    }
}
