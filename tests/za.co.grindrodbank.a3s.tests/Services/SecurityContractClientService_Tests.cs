/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.MappingProfiles;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;
using AutoMapper;
using IdentityServer4.EntityFramework.Entities;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.tests.Services
{
    public class SecurityContractClientService_Tests
    {
        IMapper mapper;
        Client mockedClientEntity;
        Oauth2ClientSubmit oauth2ClientSubmit;

        public SecurityContractClientService_Tests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new Oauth2ClientResourceClientModelProfile());
            });

            mapper = config.CreateMapper();

            // Set up a re-usable input OauthClientSubmit object.
            this.oauth2ClientSubmit = new Oauth2ClientSubmit
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
            };


            // Set up a mocked identity server 4 client entity, but use the values of the above configured Oauth2ClientSubmit as the data source.
            this.mockedClientEntity = new Client
            {
                ClientName = "Test-Client-Name",
                AllowOfflineAccess = true,
                ClientId = "test-client-id",
                UpdateAccessTokenClaimsOnRefresh = true,
                AlwaysSendClientClaims = true,
                AlwaysIncludeUserClaimsInIdToken = true,

                AllowedCorsOrigins = new List<ClientCorsOrigin> {
                    new ClientCorsOrigin {
                        Client = this.mockedClientEntity,
                        Origin = this.oauth2ClientSubmit.AllowedCorsOrigins.First()
                    }
                },
                ClientSecrets = new List<ClientSecret> {
                    new ClientSecret
                    {
                        Client = this.mockedClientEntity,
                        Value = this.oauth2ClientSubmit.ClientSecrets.First()
                    }
                },
                PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUri>
                {
                    new ClientPostLogoutRedirectUri
                    {
                        Client = this.mockedClientEntity,
                        PostLogoutRedirectUri = this.oauth2ClientSubmit.PostLogoutRedirectUris.First()
                    }
                },
                AllowedScopes = new List<ClientScope>
                {
                    new ClientScope
                    {
                        Client = this.mockedClientEntity,
                        Scope = this.oauth2ClientSubmit.AllowedScopes.First()
                    }
                },
                AllowedGrantTypes = new List<ClientGrantType>
                {
                    new ClientGrantType
                    {
                        Client = this.mockedClientEntity,
                        GrantType = this.oauth2ClientSubmit.AllowedGrantTypes.First()
                    }
                },
                RedirectUris = new List<ClientRedirectUri>
                {
                    new ClientRedirectUri
                    {
                        Client = this.mockedClientEntity,
                        RedirectUri = this.oauth2ClientSubmit.RedirectUris.First()
                    }
                }
            };
        }

        [Fact]
        public async Task ApplyClientDefninition_NoExistingClient_ReturnsNewClient()
        {
            var clientRespository = Substitute.For<IIdentityClientRepository>();
            // The service will look for an existing client by it's ID, return null to trigger the client creation flow.
            clientRespository.GetByClientIdAsync(Arg.Any<string>()).Returns((Client)null);
            clientRespository.CreateAsync(Arg.Any<Client>()).Returns(mockedClientEntity);

            var clientService = new SecurityContractClientService(clientRespository, mapper);
            var createClientResource = await clientService.ApplyClientDefinitionAsync(oauth2ClientSubmit, false, new SecurityContractDryRunResult());

            Assert.True(createClientResource.Name == oauth2ClientSubmit.Name, $"Retrieved name: {createClientResource.Name} not the same as the expected name: {oauth2ClientSubmit.Name}");
            Assert.True(createClientResource.ClientId == oauth2ClientSubmit.ClientId, $"Retrieved clientId: {createClientResource.ClientId} not the same as the expected name: {oauth2ClientSubmit.ClientId}");
            Assert.True(createClientResource.AllowedOfflineAccess == true, $"Retrieved allowedOfflineAccess: {createClientResource.AllowedOfflineAccess} not the expeced value: true");
            Assert.True(createClientResource.AllowedGrantTypes.First() == oauth2ClientSubmit.AllowedGrantTypes.First(), $"Retrieved allowedGrantTypes first element: {createClientResource.AllowedGrantTypes.First()} not the expected value: {oauth2ClientSubmit.AllowedGrantTypes.First()}");
            Assert.True(createClientResource.AllowedCorsOrigins.First() == oauth2ClientSubmit.AllowedCorsOrigins.First(), $"Retrieved allowedCorsOrigins first element: {createClientResource.AllowedCorsOrigins.First()} not the expected value: {oauth2ClientSubmit.AllowedCorsOrigins.First()}");
            Assert.True(createClientResource.PostLogoutRedirectUris.First() == oauth2ClientSubmit.PostLogoutRedirectUris.First(), $"Retrieved PostLogoutRedirectUris first element: {createClientResource.PostLogoutRedirectUris.First()} not the expected value: {oauth2ClientSubmit.PostLogoutRedirectUris.First()}");
            Assert.True(createClientResource.AllowedScopes.First() == oauth2ClientSubmit.AllowedScopes.First(), $"Retrieved AllowedScopes first element: {createClientResource.AllowedScopes.First()} not the expected value: {oauth2ClientSubmit.AllowedScopes.First()}");
            Assert.True(createClientResource.RedirectUris.First() == oauth2ClientSubmit.RedirectUris.First(), $"Retrieved RedirectUris first element: {createClientResource.RedirectUris.First()} not the expected value: {oauth2ClientSubmit.RedirectUris.First()}");
        }

        [Fact]
        public async Task ApplyClientDefninition_IsExistingClient_ReturnsUpdatedClient()
        {
            var clientRespository = Substitute.For<IIdentityClientRepository>();
            // The service will look for an existing client by it's ID, return null to trigger the client creation flow.
            clientRespository.GetByClientIdAsync(Arg.Any<string>()).Returns(mockedClientEntity);
            clientRespository.UpdateAsync(Arg.Any<Client>()).Returns(mockedClientEntity);

            var clientService = new SecurityContractClientService(clientRespository, mapper);
            var updateClientResource = await clientService.ApplyClientDefinitionAsync(oauth2ClientSubmit, false, new SecurityContractDryRunResult());

            Assert.True(updateClientResource.Name == oauth2ClientSubmit.Name, $"Retrieved name: {updateClientResource.Name} not the same as the expected name: {oauth2ClientSubmit.Name}");
            Assert.True(updateClientResource.ClientId == oauth2ClientSubmit.ClientId, $"Retrieved clientId: {updateClientResource.ClientId} not the same as the expected name: {oauth2ClientSubmit.ClientId}");
            Assert.True(updateClientResource.AllowedOfflineAccess == true, $"Retrieved allowedOfflineAccess: {updateClientResource.AllowedOfflineAccess} not the expeced value: true");
            Assert.True(updateClientResource.AllowedGrantTypes.First() == oauth2ClientSubmit.AllowedGrantTypes.First(), $"Retrieved allowedGrantTypes first element: {updateClientResource.AllowedGrantTypes.First()} not the expected value: {oauth2ClientSubmit.AllowedGrantTypes.First()}");
            Assert.True(updateClientResource.AllowedCorsOrigins.First() == oauth2ClientSubmit.AllowedCorsOrigins.First(), $"Retrieved allowedCorsOrigins first element: {updateClientResource.AllowedCorsOrigins.First()} not the expected value: {oauth2ClientSubmit.AllowedCorsOrigins.First()}");
            Assert.True(updateClientResource.PostLogoutRedirectUris.First() == oauth2ClientSubmit.PostLogoutRedirectUris.First(), $"Retrieved PostLogoutRedirectUris first element: {updateClientResource.PostLogoutRedirectUris.First()} not the expected value: {oauth2ClientSubmit.PostLogoutRedirectUris.First()}");
            Assert.True(updateClientResource.AllowedScopes.First() == oauth2ClientSubmit.AllowedScopes.First(), $"Retrieved AllowedScopes first element: {updateClientResource.AllowedScopes.First()} not the expected value: {oauth2ClientSubmit.AllowedScopes.First()}");
            Assert.True(updateClientResource.RedirectUris.First() == oauth2ClientSubmit.RedirectUris.First(), $"Retrieved RedirectUris first element: {updateClientResource.RedirectUris.First()} not the expected value: {oauth2ClientSubmit.RedirectUris.First()}");
        }

        [Fact]
        public void InitSharedTransaction_Executed_ExecutesWithNoException()
        {
            var clientRespository = Substitute.For<IIdentityClientRepository>();
            var clientService = new SecurityContractClientService(clientRespository, mapper);

            Exception caughtException = null;

            try
            {
                clientService.InitSharedTransaction();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.True(caughtException == null, "InitSharedTransaction must not throw an exception.");
        }

        [Fact]
        public void CommitTransaction_Executed_ExecutesWithNoException()
        {
            var clientRespository = Substitute.For<IIdentityClientRepository>();
            var clientService = new SecurityContractClientService(clientRespository, mapper);

            Exception caughtException = null;

            try
            {
                clientService.CommitTransaction();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.True(caughtException == null, "CommitTransaction must not throw an exception.");
        }

        [Fact]
        public void RollbackTransaction_Executed_ExecutesWithNoException()
        {
            var clientRespository = Substitute.For<IIdentityClientRepository>();
            var clientService = new SecurityContractClientService(clientRespository, mapper);

            Exception caughtException = null;

            try
            {
                clientService.RollbackTransaction();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.True(caughtException == null, "RollbackTransaction must not throw an exception.");
        }

        [Fact]
        public async Task ApplyClientDefninition_EmptyAllowedCorsOriginArrayMember_ReturnsInvalidFormatException()
        {
            var clientRespository = Substitute.For<IIdentityClientRepository>();
            // The service will look for an existing client by it's ID, return null to trigger the client creation flow.
            clientRespository.GetByClientIdAsync(Arg.Any<string>()).Returns(mockedClientEntity);
            clientRespository.UpdateAsync(Arg.Any<Client>()).Returns(mockedClientEntity);

            var clientService = new SecurityContractClientService(clientRespository, mapper);
            var oauthClientSubmitMock = oauth2ClientSubmit;
            // Set the allowed CORS orgin array member to an empty string field. This should throw a v
            oauthClientSubmitMock.AllowedCorsOrigins = new List<string> { "" };
            try
            {
                var updateClientResource = await clientService.ApplyClientDefinitionAsync(oauth2ClientSubmit, false, new SecurityContractDryRunResult());
            }
            catch(Exception e)
            {
                Assert.True(e is InvalidFormatException);
            }
        }

        [Fact]
        public async Task ApplyClientDefninition_WhitespaceAllowedCorsOriginArrayMember_ReturnsInvalidFormatException()
        {
            var clientRespository = Substitute.For<IIdentityClientRepository>();
            // The service will look for an existing client by it's ID, return null to trigger the client creation flow.
            clientRespository.GetByClientIdAsync(Arg.Any<string>()).Returns(mockedClientEntity);
            clientRespository.UpdateAsync(Arg.Any<Client>()).Returns(mockedClientEntity);

            var clientService = new SecurityContractClientService(clientRespository, mapper);
            var oauthClientSubmitMock = oauth2ClientSubmit;
            // Set the allowed CORS orgin array member to an empty string field. This should throw a v
            oauthClientSubmitMock.AllowedCorsOrigins = new List<string> { "    " };
            try
            {
                var updateClientResource = await clientService.ApplyClientDefinitionAsync(oauth2ClientSubmit, false, new SecurityContractDryRunResult());
            }
            catch (Exception e)
            {
                Assert.True(e is InvalidFormatException);
            }
        }

        [Fact]
        public async Task ApplyClientDefninition_NoAllowedCorsOriginDefined_ReturnsClientWithEmptyAllowedCorsOrigins()
        {
            var clientRespository = Substitute.For<IIdentityClientRepository>();
            // The service will look for an existing client by it's ID, return null to trigger the client creation flow.
            clientRespository.GetByClientIdAsync(Arg.Any<string>()).Returns(mockedClientEntity);
            clientRespository.UpdateAsync(Arg.Any<Client>()).Returns(mockedClientEntity);

            var oauthClientSubmitMock = oauth2ClientSubmit;
            // Set the allowed CORS orgin array member to an empty string field. This should throw a v
            oauthClientSubmitMock.AllowedCorsOrigins = null;

            var clientService = new SecurityContractClientService(clientRespository, mapper);
            var updateClientResource = await clientService.ApplyClientDefinitionAsync(oauthClientSubmitMock, false, new SecurityContractDryRunResult());

           
            Assert.True(updateClientResource.Name == oauth2ClientSubmit.Name, $"Retrieved name: {updateClientResource.Name} not the same as the expected name: {oauth2ClientSubmit.Name}");
            Assert.True(updateClientResource.ClientId == oauth2ClientSubmit.ClientId, $"Retrieved clientId: {updateClientResource.ClientId} not the same as the expected name: {oauth2ClientSubmit.ClientId}");
            Assert.True(updateClientResource.AllowedOfflineAccess == true, $"Retrieved allowedOfflineAccess: {updateClientResource.AllowedOfflineAccess} not the expeced value: true");
            Assert.True(updateClientResource.AllowedGrantTypes.First() == oauth2ClientSubmit.AllowedGrantTypes.First(), $"Retrieved allowedGrantTypes first element: {updateClientResource.AllowedGrantTypes.First()} not the expected value: {oauth2ClientSubmit.AllowedGrantTypes.First()}");
            Assert.True(!updateClientResource.AllowedCorsOrigins.Any(), $"Retrieved allowedCorsOrigins is expected to be empty, but it is not.");
            Assert.True(updateClientResource.PostLogoutRedirectUris.First() == oauth2ClientSubmit.PostLogoutRedirectUris.First(), $"Retrieved PostLogoutRedirectUris first element: {updateClientResource.PostLogoutRedirectUris.First()} not the expected value: {oauth2ClientSubmit.PostLogoutRedirectUris.First()}");
            Assert.True(updateClientResource.AllowedScopes.First() == oauth2ClientSubmit.AllowedScopes.First(), $"Retrieved AllowedScopes first element: {updateClientResource.AllowedScopes.First()} not the expected value: {oauth2ClientSubmit.AllowedScopes.First()}");
            Assert.True(updateClientResource.RedirectUris.First() == oauth2ClientSubmit.RedirectUris.First(), $"Retrieved RedirectUris first element: {updateClientResource.RedirectUris.First()} not the expected value: {oauth2ClientSubmit.RedirectUris.First()}");
        }

        [Fact]
        public async Task ApplyClientDefninition_NoPostLogoutRedirectUrisDefined_ReturnsClientWithEmptyPostLogoutRredirectUris()
        {
            var clientRespository = Substitute.For<IIdentityClientRepository>();
            // The service will look for an existing client by it's ID, return null to trigger the client creation flow.
            clientRespository.GetByClientIdAsync(Arg.Any<string>()).Returns(mockedClientEntity);
            clientRespository.UpdateAsync(Arg.Any<Client>()).Returns(mockedClientEntity);

            var oauthClientSubmitMock = oauth2ClientSubmit;
            // Set the allowed CORS orgin array member to an empty string field. This should throw a v
            oauthClientSubmitMock.PostLogoutRedirectUris = null;

            var clientService = new SecurityContractClientService(clientRespository, mapper);
            var updateClientResource = await clientService.ApplyClientDefinitionAsync(oauthClientSubmitMock, false, new SecurityContractDryRunResult());


            Assert.True(updateClientResource.Name == oauth2ClientSubmit.Name, $"Retrieved name: {updateClientResource.Name} not the same as the expected name: {oauth2ClientSubmit.Name}");
            Assert.True(updateClientResource.ClientId == oauth2ClientSubmit.ClientId, $"Retrieved clientId: {updateClientResource.ClientId} not the same as the expected name: {oauth2ClientSubmit.ClientId}");
            Assert.True(updateClientResource.AllowedOfflineAccess == true, $"Retrieved allowedOfflineAccess: {updateClientResource.AllowedOfflineAccess} not the expeced value: true");
            Assert.True(updateClientResource.AllowedGrantTypes.First() == oauth2ClientSubmit.AllowedGrantTypes.First(), $"Retrieved allowedGrantTypes first element: {updateClientResource.AllowedGrantTypes.First()} not the expected value: {oauth2ClientSubmit.AllowedGrantTypes.First()}");
            Assert.True(updateClientResource.AllowedCorsOrigins.First() == oauth2ClientSubmit.AllowedCorsOrigins.First(), $"Retrieved allowedCorsOrigins first element: {updateClientResource.AllowedCorsOrigins.First()} not the expected value: {oauth2ClientSubmit.AllowedCorsOrigins.First()}");
            Assert.True(!updateClientResource.PostLogoutRedirectUris.Any(), $"Retrieved PostLogoutRedirectUris is expected to be empty, but it is not.");
            Assert.True(updateClientResource.AllowedScopes.First() == oauth2ClientSubmit.AllowedScopes.First(), $"Retrieved AllowedScopes first element: {updateClientResource.AllowedScopes.First()} not the expected value: {oauth2ClientSubmit.AllowedScopes.First()}");
            Assert.True(updateClientResource.RedirectUris.First() == oauth2ClientSubmit.RedirectUris.First(), $"Retrieved RedirectUris first element: {updateClientResource.RedirectUris.First()} not the expected value: {oauth2ClientSubmit.RedirectUris.First()}");        
        }
    }
}
