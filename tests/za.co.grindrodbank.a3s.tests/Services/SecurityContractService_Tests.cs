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
using za.co.grindrodbank.a3s.Services;
using AutoMapper;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Exceptions;

namespace za.co.grindrodbank.a3s.tests.Services
{
    public class SecurityContractService_Tests
    {
        [Fact]
        public async Task ApplySecurityContractAsync_WithValidNoSectionsInput_NoExceptionsThrown()
        {
            //Arrange
            var securityContractClientService = Substitute.For<ISecurityContractClientService>();
            var securityContractDefaultConfigurationService = Substitute.For<ISecurityContractDefaultConfigurationService>();
            var securityContractApplicationService = Substitute.For<ISecurityContractApplicationService>();

            // A security contract with all it's sub components set to null is a valid security contract.
            var securityContract = new SecurityContract();
            var securityContractService = new SecurityContractService(securityContractApplicationService, securityContractClientService, securityContractDefaultConfigurationService);

            try
            {
                await securityContractService.ApplySecurityContractDefinitionAsync(securityContract, Guid.NewGuid());
                Assert.True(true);
            }
            catch (Exception e)
            {
                Assert.True(false, $"Unexpected Exception: '{e.Message}' thrown when applying security contract");
            }
        }

        [Fact]
        public async Task ApplySecurityContractAsync_WithValidHighLevelSecurityContractInput_NoExceptionsThrown()
        {
            //Arrange
            var securityContractClientService = Substitute.For<ISecurityContractClientService>();
            var securityContractDefaultConfigurationService = Substitute.For<ISecurityContractDefaultConfigurationService>();
            var securityContractApplicationService = Substitute.For<ISecurityContractApplicationService>();

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

            var securityContractService = new SecurityContractService(securityContractApplicationService, securityContractClientService, securityContractDefaultConfigurationService);

            try
            {
                await securityContractService.ApplySecurityContractDefinitionAsync(securityContract, Guid.NewGuid());
                Assert.True(true);
            }
            catch (Exception e)
            {
                Assert.True(false, $"Unexpected Exception: '{e.Message}' thrown when applying security contract");
            }
        }

        [Fact]
        public async Task ApplySecurityContractAsync_WithDefaultConfigurationServiceThrowingItemNotFoundException_ItemNotFoundExceptionThrown()
        {
            //Arrange
            var securityContractClientService = Substitute.For<ISecurityContractClientService>();
            var securityContractDefaultConfigurationService = Substitute.For<ISecurityContractDefaultConfigurationService>();
            var securityContractApplicationService = Substitute.For<ISecurityContractApplicationService>();

            // A security contract with all it's sub components set to null is a valid security contract.
            var securityContract = new SecurityContract();

            securityContract.DefaultConfigurations = new List<SecurityContractDefaultConfiguration>
            {
                new SecurityContractDefaultConfiguration
                {
                    Name = "test default configuration"
                }
            };

            securityContractDefaultConfigurationService.ApplyDefaultConfigurationDefinitionAsync(Arg.Any<SecurityContractDefaultConfiguration>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<List<string>>()).Returns(x => throw new ItemNotFoundException());

            var securityContractService = new SecurityContractService(securityContractApplicationService, securityContractClientService, securityContractDefaultConfigurationService);

            Exception caughtException = null;
            try
            {
                await securityContractService.ApplySecurityContractDefinitionAsync(securityContract, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.True(caughtException is ItemNotFoundException);
        }

        [Fact]
        public async Task ApplySecurityContractAsync_WithDefaultConfigurationServiceThrowingItemNotProcessableException_ItemNotProcessableExceptionThrown()
        {
            //Arrange
            var securityContractClientService = Substitute.For<ISecurityContractClientService>();
            var securityContractDefaultConfigurationService = Substitute.For<ISecurityContractDefaultConfigurationService>();
            var securityContractApplicationService = Substitute.For<ISecurityContractApplicationService>();

            // A security contract with all it's sub components set to null is a valid security contract.
            var securityContract = new SecurityContract();

            securityContract.DefaultConfigurations = new List<SecurityContractDefaultConfiguration>
            {
                new SecurityContractDefaultConfiguration
                {
                    Name = "test default configuration"
                }
            };

            securityContractDefaultConfigurationService.ApplyDefaultConfigurationDefinitionAsync(Arg.Any<SecurityContractDefaultConfiguration>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<List<string>>()).Returns(x => throw new ItemNotProcessableException());

            var securityContractService = new SecurityContractService(securityContractApplicationService, securityContractClientService, securityContractDefaultConfigurationService);

            Exception caughtException = null;
            try
            {
                await securityContractService.ApplySecurityContractDefinitionAsync(securityContract, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.True(caughtException is ItemNotProcessableException);
        }

        [Fact]
        public async Task ApplySecurityContractAsync_WithApplicationServiceThrowingException_ExceptionThrown()
        {
            //Arrange
            var securityContractClientService = Substitute.For<ISecurityContractClientService>();
            var securityContractDefaultConfigurationService = Substitute.For<ISecurityContractDefaultConfigurationService>();
            var securityContractApplicationService = Substitute.For<ISecurityContractApplicationService>();

            // A security contract with all it's sub components set to null is a valid security contract.
            var securityContract = new SecurityContract();

            securityContract.DefaultConfigurations = new List<SecurityContractDefaultConfiguration>
            {
                new SecurityContractDefaultConfiguration
                {
                    Name = "test default configuration"
                }
            };

            securityContractDefaultConfigurationService.ApplyDefaultConfigurationDefinitionAsync(Arg.Any<SecurityContractDefaultConfiguration>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<List<string>>()).Returns(x => throw new Exception());

            var securityContractService = new SecurityContractService(securityContractApplicationService, securityContractClientService, securityContractDefaultConfigurationService);

            Exception caughtException = null;
            try
            {
                await securityContractService.ApplySecurityContractDefinitionAsync(securityContract, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.True(caughtException is Exception);
        }

        [Fact]
        public async Task GetSecurityContractAsync_WithValidHighLevelSecurityContractInput_NoExceptionsThrown()
        {
            //Arrange
            var securityContractClientService = Substitute.For<ISecurityContractClientService>();
            var securityContractDefaultConfigurationService = Substitute.For<ISecurityContractDefaultConfigurationService>();
            var securityContractApplicationService = Substitute.For<ISecurityContractApplicationService>();

            var securityContractService = new SecurityContractService(securityContractApplicationService, securityContractClientService, securityContractDefaultConfigurationService);

            securityContractClientService.GetClientDefinitionsAsync().Returns(new List<Oauth2ClientSubmit>
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
            });

            securityContractApplicationService.GetResourceServerDefinitionsAsync().Returns(new List<SecurityContractApplication>
            {
                new SecurityContractApplication
                {
                    Fullname = "Test application"
                }
            });

            securityContractDefaultConfigurationService.GetDefaultConfigurationDefinitionAsync().Returns(new SecurityContractDefaultConfiguration
            {
                Name = "test default configuration"
            });

            // Act
            SecurityContract retrievedSecurityContract = null;
            try
            {
                retrievedSecurityContract = await securityContractService.GetSecurityContractDefinitionAsync();
                Assert.True(true);
            }
            catch (Exception e)
            {
                Assert.True(false, $"Unexpected Exception: '{e.Message}' thrown when retrieving security contract");
            }

            // Assert
            Assert.True(retrievedSecurityContract != null, "Retrieved security contract should not be null");
            Assert.True(retrievedSecurityContract.Clients != null && retrievedSecurityContract.Clients.Count > 0, "Retrieved security contract Clients should not be null or empty");
            Assert.True(retrievedSecurityContract.Applications != null && retrievedSecurityContract.Applications.Count > 0, "Retrieved security contract Applications should not be null or empty");
            Assert.True(retrievedSecurityContract.DefaultConfigurations != null && retrievedSecurityContract.DefaultConfigurations.Count > 0, "Retrieved security contract DefaultConfigurations should not be null or empty");

            Assert.True(retrievedSecurityContract.Clients[0].Name == "Test-Client-Name", "Retrieved client Name should be 'Test-Client-Name'");
            Assert.True(retrievedSecurityContract.Applications[0].Fullname == "Test application", "Retrieved Applications Fullname should be 'Test application'");
            Assert.True(retrievedSecurityContract.DefaultConfigurations[0].Name == "test default configuration", "Retrieved DefaultConfigurations name should be 'test default configuration'");

        }
    }
}
