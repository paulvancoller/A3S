/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Controllers;
using za.co.grindrodbank.a3s.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.A3SApiResources;
using System.Collections.Generic;

namespace za.co.grindrodbank.a3s.tests.Controllers
{
    public class SecurityContractController_Tests
    {
        public SecurityContractController_Tests()
        {
        }

        [Fact]
        public async Task ApplySecurityContractAsync_WithNullInput_ReturnsBadRequest()
        {
            // Arrange
            var securityContractService = Substitute.For<ISecurityContractService>();
            var controller = new SecurityContractController(securityContractService);

            // Act
            var actionResult = await controller.ApplySecurityContractAsync(null);

            // Assert
            var badRequestResult = actionResult as BadRequestResult;
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task ApplySecurityContractAsync_WithValidNoSectionsInput_ReturnsNoContent()
        {
            // Arrange
            var securityContractService = Substitute.For<ISecurityContractService>();
            var controller = new SecurityContractController(securityContractService);

            // A security contract with all it's sub components set to null is a valid security contract.
            var securityContract = new SecurityContract();

            // Act
            var actionResult = await controller.ApplySecurityContractAsync(securityContract);

            // Assert
            var NoContent = actionResult as NoContentResult;
            Assert.NotNull(NoContent);
        }

        [Fact]
        public async Task GetSecurityContractAsync_Executed_ReturnsSecurityContract()
        {
            // Arrange
            var securityContractService = Substitute.For<ISecurityContractService>();
            var controller = new SecurityContractController(securityContractService);

            // A security contract with all it's sub components set to null is a valid security contract.
            var securityContract = new SecurityContract();
            securityContract.Applications = new List<SecurityContractApplication>
            {
                new SecurityContractApplication
                {
                    Fullname = "Test application"
                }
            };

            securityContract.Clients = new List<Oauth2ClientSubmit>
            {
                new Oauth2ClientSubmit
                {
                    ClientId = "test-client-id",
                    Name = "Test-Client-Name",
                    AllowedOfflineAccess = true,
                    AllowedCorsOrigins = new List<string>
                    {
                        "http://test-cors-origin.com"
                    },
                    ClientSecrets = new List<string>
                    {
                        "test-client-secret"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://test-post-logout-uri.com"
                    },
                    AllowedGrantTypes = new List<string>
                    {
                        "password"
                    },
                    AllowedScopes = new List<string>
                    {
                        "test-client-scope"
                    },
                    RedirectUris = new List<string>
                    {
                        "http://test-redirect-uri.com"
                    }
                }
            };

            securityContract.DefaultConfigurations = new List<SecurityContractDefaultConfiguration>
            {
                new SecurityContractDefaultConfiguration
                {
                    Name = "test default configuration"
                }
            };

            securityContractService.GetSecurityContractDefinitionAsync().Returns(securityContract);

            // Act
            var actionResult = await controller.GetSecurityContractAsync();

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var outContract = okResult.Value as SecurityContract;
            Assert.NotNull(outContract);

            Assert.True(outContract != null, "Retrieved security contract should not be null");
            Assert.True(outContract.Clients != null && outContract.Clients.Count > 0, "Retrieved security contract Clients should not be null or empty");
            Assert.True(outContract.Applications != null && outContract.Applications.Count > 0, "Retrieved security contract Applications should not be null or empty");
            Assert.True(outContract.DefaultConfigurations != null && outContract.DefaultConfigurations.Count > 0, "Retrieved security contract DefaultConfigurations should not be null or empty");

            Assert.True(outContract.Clients[0].Name == "Test-Client-Name", "Retrieved client Name should be 'Test-Client-Name'");
            Assert.True(outContract.Applications[0].Fullname == "Test application", "Retrieved Applications Fullname should be 'Test application'");
            Assert.True(outContract.DefaultConfigurations[0].Name == "test default configuration", "Retrieved DefaultConfigurations name should be 'test default configuration'");
        }
    }
}
