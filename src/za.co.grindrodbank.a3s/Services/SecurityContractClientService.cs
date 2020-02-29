/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Repositories;
using AutoMapper;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.Models;
using NLog;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Services
{
    public class SecurityContractClientService : ISecurityContractClientService
    {
        private readonly IIdentityClientRepository identityClientRepository;
        private readonly IMapper mapper;
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public SecurityContractClientService(IIdentityClientRepository identityClientRepository, IMapper mapper)
        {
            this.identityClientRepository = identityClientRepository;
            this.mapper = mapper;
        }

        public async Task<Oauth2Client> ApplyClientDefinitionAsync(Oauth2ClientSubmit oauth2ClientSubmit, bool dryRun, SecurityContractDryRunResult securityContractDryRunResult)
        {
            logger.Debug($"[client.clientId: '{oauth2ClientSubmit.ClientId}']: Applying client definition for client: '{oauth2ClientSubmit.ClientId}'.");
            IdentityServer4.EntityFramework.Entities.Client client = await identityClientRepository.GetByClientIdAsync(oauth2ClientSubmit.ClientId);
            bool newClient = false;

            if(client == null)
            {
                client = new IdentityServer4.EntityFramework.Entities.Client();
                newClient = true;
            }

            client.AllowOfflineAccess = oauth2ClientSubmit.AllowedOfflineAccess;
            client.ClientId = oauth2ClientSubmit.ClientId;
            client.ClientName = oauth2ClientSubmit.Name;

            // The following properties of clients are not externally configurable, but we do need to add them th clients to get the desired behaviour.
            client.UpdateAccessTokenClaimsOnRefresh = true;
            client.AlwaysSendClientClaims = true;
            client.AlwaysIncludeUserClaimsInIdToken = true;
            client.RequireConsent = false;

            if (oauth2ClientSubmit.AccessTokenLifetime > 0)
            {
                client.AccessTokenLifetime = oauth2ClientSubmit.AccessTokenLifetime;
            }

            if (oauth2ClientSubmit.IdentityTokenLifetime > 0)
            {
                client.IdentityTokenLifetime = oauth2ClientSubmit.IdentityTokenLifetime;
            }

            client.RefreshTokenExpiration = (int)TokenExpiration.Absolute;
            client.RefreshTokenUsage = (int)TokenUsage.OneTimeOnly;

            ApplyClientAllowedScopes(client, oauth2ClientSubmit);
            ApplyClientAllowedGrantTypes(client, oauth2ClientSubmit);
            ApplyClientSecrets(client, oauth2ClientSubmit);
            ApplyClientRedirectUris(client, oauth2ClientSubmit);
            ApplyClientPostLogoutRedirectUris(client, oauth2ClientSubmit);
            ApplyClientAllowedCorsOrigins(client, oauth2ClientSubmit, dryRun, securityContractDryRunResult);

            if (newClient)
            {
                logger.Debug($"[client.clientId: '{oauth2ClientSubmit.ClientId}']: Client '{oauth2ClientSubmit.ClientId}' does not exist. Creating it.");
                return mapper.Map<Oauth2Client>(await identityClientRepository.CreateAsync(client));
            }

            logger.Debug($"[client.clientId: '{oauth2ClientSubmit.ClientId}']: Client '{oauth2ClientSubmit.ClientId}' already exists. Updating it.");
            return mapper.Map<Oauth2Client>(await identityClientRepository.UpdateAsync(client));
        }

        private void ApplyClientAllowedScopes(IdentityServer4.EntityFramework.Entities.Client client, Oauth2ClientSubmit oauth2ClientSubmit)
        {
            client.AllowedScopes = new List<ClientScope>();

            foreach (var clientScope in oauth2ClientSubmit.AllowedScopes)
            {
                client.AllowedScopes.Add(new ClientScope
                {
                    Client = client,
                    Scope = clientScope
                });
            }
        }

        private void ApplyClientAllowedGrantTypes(IdentityServer4.EntityFramework.Entities.Client client, Oauth2ClientSubmit oauth2ClientSubmit)
        {
            client.AllowedGrantTypes = new List<ClientGrantType>();

            foreach (var grantType in oauth2ClientSubmit.AllowedGrantTypes)
            {
                client.AllowedGrantTypes.Add(new ClientGrantType
                {
                    Client = client,
                    GrantType = grantType
                });
            }
        }

        private void ApplyClientSecrets(IdentityServer4.EntityFramework.Entities.Client client, Oauth2ClientSubmit oauth2ClientSubmit)
        {
            client.ClientSecrets = new List<ClientSecret>();

            if (oauth2ClientSubmit.HashedClientSecrets != null && oauth2ClientSubmit.HashedClientSecrets.Count > 0)
            {
                foreach (var hashedClientSecret in oauth2ClientSubmit.HashedClientSecrets)
                {
                    client.ClientSecrets.Add(new ClientSecret
                    {
                        Client = client,
                        Value = hashedClientSecret
                    });
                }
            }
            else
            {
                foreach (var clientSecret in oauth2ClientSubmit.ClientSecrets)
                {
                    client.ClientSecrets.Add(new ClientSecret
                    {
                        Client = client,
                        Value = clientSecret.Sha256()
                    });
                }
            }
        }

        private void ApplyClientAllowedCorsOrigins(IdentityServer4.EntityFramework.Entities.Client client, Oauth2ClientSubmit oauth2ClientSubmit, bool dryRun, SecurityContractDryRunResult securityContractDryRunResult)
        {
            client.AllowedCorsOrigins = new List<ClientCorsOrigin>();

            if (oauth2ClientSubmit.AllowedCorsOrigins != null && oauth2ClientSubmit.AllowedCorsOrigins.Any())
            {
                foreach (var corsOrigin in oauth2ClientSubmit.AllowedCorsOrigins)
                {
                    if (string.IsNullOrWhiteSpace(corsOrigin))
                    {
                        var errMessage = $"[client.clientId: '{oauth2ClientSubmit.ClientId}']: Empty or null 'allowedCorsOrigin' element declared for client: '{oauth2ClientSubmit.ClientId}'";

                        if (dryRun)
                        {
                            securityContractDryRunResult.ValidationErrors.Add(errMessage);
                        }
                        else
                        {
                            throw new InvalidFormatException(errMessage);
                        }
                    }

                    client.AllowedCorsOrigins.Add(new ClientCorsOrigin
                    {
                        Client = client,
                        Origin = corsOrigin
                    });
                }
            }
        }

        private void ApplyClientRedirectUris(IdentityServer4.EntityFramework.Entities.Client client, Oauth2ClientSubmit oauth2ClientSubmit)
        {
            client.RedirectUris = new List<ClientRedirectUri>();

            foreach (var redirectUri in oauth2ClientSubmit.RedirectUris)
            {
                client.RedirectUris.Add(new ClientRedirectUri
                {
                    Client = client,
                    RedirectUri = redirectUri
                });
            }
        }

        public void ApplyClientPostLogoutRedirectUris(IdentityServer4.EntityFramework.Entities.Client client, Oauth2ClientSubmit oauth2ClientSubmit)
        {
            client.PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUri>();

            if (oauth2ClientSubmit.PostLogoutRedirectUris != null && oauth2ClientSubmit.PostLogoutRedirectUris.Any())
            {
                foreach (var postLogoutRedirectUri in oauth2ClientSubmit.PostLogoutRedirectUris)
                {
                    client.PostLogoutRedirectUris.Add(new ClientPostLogoutRedirectUri
                    {
                        Client = client,
                        PostLogoutRedirectUri = postLogoutRedirectUri
                    });
                }
            }
        }

        public async Task<List<Oauth2ClientSubmit>> GetClientDefinitionsAsync()
        {
            logger.Debug($"Retrieving client security contract definition.");

            var contractClients = new List<Oauth2ClientSubmit>();
            List<IdentityServer4.EntityFramework.Entities.Client> clients = await identityClientRepository.GetListAsync();

            foreach (var client in clients.OrderBy(o => o.ClientName))
            {
                logger.Debug($"Retrieving client security contract definition for Client '{client.ClientId}'.");

                var contractClient = new Oauth2ClientSubmit()
                {
                    ClientId = client.ClientId,
                    Name = client.ClientName,
                    AccessTokenLifetime = client.AccessTokenLifetime,
                    IdentityTokenLifetime = client.IdentityTokenLifetime,
                    AllowedOfflineAccess = client.AllowOfflineAccess,
                    AllowedCorsOrigins = new List<string>(),
                    AllowedGrantTypes = new List<string>(),
                    AllowedScopes = new List<string>(),
                    HashedClientSecrets = new List<string>(),
                    PostLogoutRedirectUris = new List<string>(),
                    RedirectUris = new List<string>()
                };

                foreach (var origin in client.AllowedCorsOrigins.OrderBy(o => o.Origin))
                    contractClient.AllowedCorsOrigins.Add(origin.Origin);

                foreach (var origin in client.AllowedGrantTypes.OrderBy(o => o.GrantType))
                    contractClient.AllowedGrantTypes.Add(origin.GrantType);

                foreach (var origin in client.AllowedScopes.OrderBy(o => o.Scope))
                    contractClient.AllowedScopes.Add(origin.Scope);

                foreach (var origin in client.ClientSecrets.OrderBy(o => o.Value))
                    contractClient.HashedClientSecrets.Add(origin.Value);

                foreach (var origin in client.PostLogoutRedirectUris.OrderBy(o => o.PostLogoutRedirectUri))
                    contractClient.PostLogoutRedirectUris.Add(origin.PostLogoutRedirectUri);

                foreach (var origin in client.RedirectUris.OrderBy(o => o.RedirectUri))
                    contractClient.RedirectUris.Add(origin.RedirectUri);

                contractClients.Add(contractClient);
            }

            return contractClients;
        }

        public void InitSharedTransaction()
        {
            identityClientRepository.InitSharedTransaction();
        }

        public void CommitTransaction()
        {
            identityClientRepository.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            identityClientRepository.RollbackTransaction();
        }
    }
}
