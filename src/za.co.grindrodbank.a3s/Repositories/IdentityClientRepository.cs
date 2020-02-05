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
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using za.co.grindrodbank.a3s.Extensions;

namespace za.co.grindrodbank.a3s.Repositories
{
    public class IdentityClientRepository : PaginatedRepository<Client>, IIdentityClientRepository
    {
        private readonly ConfigurationDbContext identityServerConfigurationContext;

        public IdentityClientRepository(ConfigurationDbContext identityServerConfigurationContext)
        {
            this.identityServerConfigurationContext = identityServerConfigurationContext;
        }

        public void InitSharedTransaction()
        {
            if (identityServerConfigurationContext.Database.CurrentTransaction == null)
                identityServerConfigurationContext.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (identityServerConfigurationContext.Database.CurrentTransaction != null)
                identityServerConfigurationContext.Database.CurrentTransaction.Commit();
        }

        public void RollbackTransaction()
        {
            if (identityServerConfigurationContext.Database.CurrentTransaction != null)
                identityServerConfigurationContext.Database.CurrentTransaction.Rollback();
        }

        public async Task<Client> CreateAsync(Client client)
        {
            identityServerConfigurationContext.Clients.Add(client);
            await identityServerConfigurationContext.SaveChangesAsync();

            return client;
        }

        public async Task<Client> GetByClientIdAsync(string clientId)
        {
            IQueryable <Client> query = identityServerConfigurationContext.Clients.Where(c => c.ClientId == clientId);
            query = IncludeRelations(query);

            return  await query.FirstOrDefaultAsync();                                    
        }

        public async Task<List<Client>> GetListAsync()
        {
            IQueryable<Client> query = identityServerConfigurationContext.Clients;
            query = IncludeRelations(query);

            return await query.ToListAsync();
        }

        public async Task<PaginatedResult<Client>> GetPaginatedListAsync(int page, int pageSize, string filterName, string filterClientId, List<KeyValuePair<string, string>> orderBy)
        {
            IQueryable<Client> query = identityServerConfigurationContext.Clients;
            query = IncludeRelations(query);

            if (!string.IsNullOrWhiteSpace(filterName))
            {
                query = query.Where(c => c.ClientName == filterName);
            }

            if (!string.IsNullOrWhiteSpace(filterClientId))
            {
                query = query.Where(c => c.ClientId == filterClientId);
            }

            foreach (var orderByComponent in orderBy)
            {
                switch (orderByComponent.Key)
                {
                    case "name":
                        query = query.AppendOrderBy(a => a.ClientName, orderByComponent.Value == "asc" ? true : false);
                        break;
                    case "clientId":
                        query = query.AppendOrderBy(a => a.ClientId, orderByComponent.Value == "asc" ? true : false);
                        break;
                }
            }

            return await GetPaginatedListFromQueryAsync(query, page, pageSize);
        }

        public async Task<Client> UpdateAsync(Client client)
        {
            identityServerConfigurationContext.Entry(client).State = EntityState.Modified;
            await identityServerConfigurationContext.SaveChangesAsync();

            return client;
        }

        private IQueryable<Client> IncludeRelations(IQueryable<Client> query)
        {
            return query.Include(c => c.ClientSecrets)
                        .Include(c => c.AllowedCorsOrigins)
                        .Include(c => c.AllowedScopes)
                        .Include(c => c.PostLogoutRedirectUris)
                        .Include(c => c.RedirectUris)
                        .Include(c => c.AllowedGrantTypes);
        }
    }
}
