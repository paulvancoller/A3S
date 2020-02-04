/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using za.co.grindrodbank.a3s.Extensions;

namespace za.co.grindrodbank.a3s.Repositories
{
    public class LdapAuthenticationModeRepository : PaginatedRepository<LdapAuthenticationModeModel>, ILdapAuthenticationModeRepository
    {
        private readonly A3SContext a3SContext;
        private readonly string encryptionKey;

        private const string SELECT_COLUMNS_EXCEPT_PASSWORD = "id, name, host_name, port, is_ldaps, account, base_dn, changed_by, sys_period";
        private const string SELECT_COLUMNS_PASSWORD = "pgp_sym_decrypt(password::bytea, {0}) as \"password\"";
        private const string SELECT_TABLE = "_a3s.ldap_authentication_mode";
        private readonly string SELECT_STATEMENT_EXCLUDING_PASSWORD = $"SELECT {SELECT_COLUMNS_EXCEPT_PASSWORD}, '' as \"password\" FROM {SELECT_TABLE}";
        private readonly string SELECT_STATEMENT_INCLUDING_PASSWORD = $"SELECT {SELECT_COLUMNS_EXCEPT_PASSWORD}, {SELECT_COLUMNS_PASSWORD} FROM {SELECT_TABLE}";

        public LdapAuthenticationModeRepository(A3SContext a3SContext, IConfiguration configuration)
        {
            this.a3SContext = a3SContext;
            this.encryptionKey = configuration.GetSection("EncryptionKeys").GetValue<string>("LdapAdminKey");
        }

        public void InitSharedTransaction()
        {
            if (a3SContext.Database.CurrentTransaction == null)
                a3SContext.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (a3SContext.Database.CurrentTransaction != null)
                a3SContext.Database.CurrentTransaction.Commit();
        }

        public void RollbackTransaction()
        {
            if (a3SContext.Database.CurrentTransaction != null)
                a3SContext.Database.CurrentTransaction.Rollback();
        }

        public async Task<LdapAuthenticationModeModel> CreateAsync(LdapAuthenticationModeModel ldapAuthenticationMode)
        {
            string password = ldapAuthenticationMode.Password;
            ldapAuthenticationMode.Password = string.Empty;

            a3SContext.LdapAuthenticationMode.Add(ldapAuthenticationMode);
            await a3SContext.SaveChangesAsync();

            // Now store encrypted password
            await StoreEncryptedPassword(ldapAuthenticationMode.Id, password);

            return ldapAuthenticationMode;
        }

        public async Task DeleteAsync(LdapAuthenticationModeModel ldapAuthenticationMode)
        {
            a3SContext.LdapAuthenticationMode.Remove(ldapAuthenticationMode);
            await a3SContext.SaveChangesAsync();
        }

        public async Task<LdapAuthenticationModeModel> GetByIdAsync(Guid ldapAuthenticationModeId, bool includePassword = false, bool includeUsers = false)
        {
            string sql = (includePassword ? SELECT_STATEMENT_INCLUDING_PASSWORD : SELECT_STATEMENT_EXCLUDING_PASSWORD);

            if (!includeUsers)
            {
                return await a3SContext.LdapAuthenticationMode.FromSqlRaw(sql, encryptionKey)
                    .Where(x => x.Id == ldapAuthenticationModeId)
                    .Include(a => a.LdapAttributes)
                    .FirstOrDefaultAsync();
            }

            return await a3SContext.LdapAuthenticationMode.FromSqlRaw(sql, encryptionKey)
                   .Where(x => x.Id == ldapAuthenticationModeId)
                   .Include(a => a.LdapAttributes)
                   .Include(a => a.Users)
                   .FirstOrDefaultAsync();
        }

        public async Task<List<LdapAuthenticationModeModel>> GetListAsync(bool includePassword = false)
        {
            if (includePassword)
                return await a3SContext.LdapAuthenticationMode.FromSqlRaw(SELECT_STATEMENT_INCLUDING_PASSWORD, encryptionKey)
                    .Include(a => a.LdapAttributes)
                    .ToListAsync();

            return await a3SContext.LdapAuthenticationMode.FromSqlRaw(SELECT_STATEMENT_EXCLUDING_PASSWORD)
                .Include(a => a.LdapAttributes)
                .ToListAsync();
        }

        public async Task<LdapAuthenticationModeModel> UpdateAsync(LdapAuthenticationModeModel ldapAuthenticationMode)
        {
            // Record plain text password and clear from model
            string password = ldapAuthenticationMode.Password;
            ldapAuthenticationMode.Password = string.Empty;

            a3SContext.Entry(ldapAuthenticationMode).State = EntityState.Modified;

            // Replace Ldap Attribute Links
            a3SContext.RemoveRange(a3SContext.LdapAuthenticationModeLdapAttribute.Where(x => x.LdapAuthenticationModeId == ldapAuthenticationMode.Id));
            await a3SContext.SaveChangesAsync();

            // Now store encrypted password
            await StoreEncryptedPassword(ldapAuthenticationMode.Id, password);

            return ldapAuthenticationMode;
        }

        public async Task<LdapAuthenticationModeModel> GetByNameAsync(string name, bool includePassword = false)
        {
            string sql = (includePassword ? SELECT_STATEMENT_INCLUDING_PASSWORD : SELECT_STATEMENT_EXCLUDING_PASSWORD);
            return await a3SContext.LdapAuthenticationMode.FromSqlRaw(sql, encryptionKey)
                .Where(t => t.Name == name)
                .Include(a => a.LdapAttributes)
                .FirstOrDefaultAsync();
        }

        private async Task StoreEncryptedPassword(Guid ldapAuthenticationModeId, string password)
        {
            await a3SContext.Database.ExecuteSqlRawAsync("UPDATE _a3s.ldap_authentication_mode SET password = pgp_sym_encrypt({0}, {1}) WHERE id = {2};", password, encryptionKey, ldapAuthenticationModeId);
            await a3SContext.SaveChangesAsync(); 
        }

        private IQueryable<LdapAuthenticationModeModel> IncludeRelations(IQueryable<LdapAuthenticationModeModel> query)
        {
            return query.Include(a => a.LdapAttributes)
                        .Include(a => a.Users);
        }

        public async Task<PaginatedResult<LdapAuthenticationModeModel>> GetPaginatedListAsync(int page, int pageSize, string filterName, List<KeyValuePair<string, string>> orderBy)
        {
            IQueryable<LdapAuthenticationModeModel> query = a3SContext.LdapAuthenticationMode;
            query = IncludeRelations(query);

            if (!string.IsNullOrWhiteSpace(filterName))
            {
                query = query.Where(lam => lam.Name == filterName);
            }

            foreach (var orderByComponent in orderBy)
            {
                switch (orderByComponent.Key)
                {
                    case "name":
                        query = query.AppendOrderBy(lam => lam.Name, orderByComponent.Value == "asc" ? true : false);
                        break;
                }
            }

            return await GetPaginatedListFromQueryAsync(query, page, pageSize);
        }
    }
}
