/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using za.co.grindrodbank.a3s.Extensions;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Stores
{
    public class CustomUserStore : UserStore<UserModel>
    {
        private readonly TokenOptions tokenOptions = new TokenOptions();
        private readonly A3SContext a3SContext;
        private readonly string encryptionKey;

        public CustomUserStore(A3SContext a3SContext, IdentityErrorDescriber describer, IConfiguration configuration)
            : base(a3SContext, describer)
        {
            this.a3SContext = a3SContext;
            this.encryptionKey = configuration.GetSection("EncryptionKeys").GetValue<string>("UserTokenKey");
        }

        public override Task ReplaceCodesAsync(UserModel user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
        {
            List<string> recoveryCodesList = recoveryCodes.ToList();

            // Hash each recovery code
            using (var sha256 = SHA256.Create())
            {
                for (int i = 0; i < recoveryCodesList.Count; i++)
                {
                    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(recoveryCodesList[i]));
                    var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                    recoveryCodesList[i] = hash;
                }
            }

            var mergedCodes = string.Join(";", recoveryCodesList);
            return SetTokenAsync(user, tokenOptions.GetAspNetUserStoreProviderName(), tokenOptions.GetRecoverCodesName(), mergedCodes, cancellationToken);
        }

        public async override Task<bool> RedeemCodeAsync(UserModel user, string code, CancellationToken cancellationToken)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(code));
                var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                code = hash;
            }

            var mergedCodes = await GetTokenAsync(user, tokenOptions.GetAspNetUserStoreProviderName(), tokenOptions.GetRecoverCodesName(), cancellationToken) ?? "";
            var splitCodes = mergedCodes.Split(';');
            if (splitCodes.Contains(code))
            {
                var updatedCodes = new List<string>(splitCodes.Where(s => s != code));
                await ReplaceAlreadyHashedCodesAsync(user, updatedCodes, cancellationToken);
                return true;
            }
            return false;
        }

        public async override Task SetTokenAsync(UserModel user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            bool encrypt = (name == tokenOptions.GetAuthenticatorKeyName());
            bool isAutomaticallyVerified = (name == tokenOptions.GetRecoverCodesName());

            var tokenEntity =
                a3SContext.UserToken.SingleOrDefault(
                    l =>
                        l.Name == name && l.LoginProvider == loginProvider &&
                        l.UserId == user.Id);
            if (tokenEntity != null)
            {
                tokenEntity.Value = (encrypt ? string.Empty : value);

                if (isAutomaticallyVerified)
                    tokenEntity.IsVerified = true;

                a3SContext.Entry(tokenEntity).State = EntityState.Modified;
            }
            else
            {
                a3SContext.UserToken.Add(new UserTokenModel
                {
                    UserId = user.Id,
                    LoginProvider = loginProvider,
                    Name = name,
                    Value = (encrypt ? string.Empty : value),
                    IsVerified = isAutomaticallyVerified
                });
            }

            await a3SContext.SaveChangesAsync();

            if (encrypt)
                await StoreEncryptedValue(user, loginProvider, name, value);
        }

        public async override Task<string> GetTokenAsync(UserModel user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            bool encrypted = (name == tokenOptions.GetAuthenticatorKeyName());

            UserTokenModel tokenEntity;
            if (encrypted)
            {
                tokenEntity = await a3SContext.UserToken
                    .FromSqlRaw("SELECT user_id, login_provider, name, _a3s.pgp_sym_decrypt(\"value\"::bytea, {0}) as \"value\", is_verified FROM _a3s.application_user_token WHERE user_id = {1} and login_provider = {2} and name = {3}",
                    encryptionKey,
                    user.Id,
                    loginProvider,
                    name)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            else
            {
                tokenEntity = await a3SContext.UserToken
                    .FromSqlRaw("SELECT user_id, login_provider, name, \"value\", is_verified FROM _a3s.application_user_token WHERE user_id = {0} and login_provider = {1} and name = {2}",
                    user.Id,
                    loginProvider,
                    name)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            if (tokenEntity != null)
                return tokenEntity?.Value;

            return null;
        }

        public async Task SetAuthenticatorTokenVerifiedAsync(UserModel user)
        {
            await a3SContext.Database.ExecuteSqlRawAsync("UPDATE _a3s.application_user_token SET is_verified = true where user_id = {0} and login_provider = {1} and name = {2};",
                user.Id,
                tokenOptions.GetAspNetUserStoreProviderName(),
                tokenOptions.GetAuthenticatorKeyName());

        }

        public bool IsAuthenticatorTokenVerified(UserModel user)
        {
            var tokenEntity =
                a3SContext.UserToken.SingleOrDefault(
                    l =>
                        l.Name == tokenOptions.GetAuthenticatorKeyName() && l.LoginProvider == tokenOptions.GetAspNetUserStoreProviderName() &&
                        l.UserId == user.Id);

            if (tokenEntity == null)
                return false;

            return tokenEntity.IsVerified;
        }

        public async override Task<int> CountCodesAsync(UserModel user, CancellationToken cancellationToken)
        {
            var mergedCodes = await GetTokenAsync(user, tokenOptions.GetAspNetUserStoreProviderName(), tokenOptions.GetRecoverCodesName(), cancellationToken) ?? "";
            if (mergedCodes.Length > 0)
            {
                return mergedCodes.Split(';').Length;
            }
            return 0;
        }

        public async override Task RemoveTokenAsync(UserModel user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            var tokenEntity =
                a3SContext.UserToken.SingleOrDefault(
                    l =>
                        l.Name == tokenOptions.GetAuthenticatorKeyName() && l.LoginProvider == tokenOptions.GetAspNetUserStoreProviderName() &&
                        l.UserId == user.Id);

            if (tokenEntity != null)
                a3SContext.UserToken.Remove(tokenEntity);

            await a3SContext.SaveChangesAsync();
        }

        public async Task AgreeToTermsOfService(string userId, Guid termsOfServiceId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            if (termsOfServiceId == Guid.Empty)
                throw new ArgumentNullException(nameof(termsOfServiceId));

            var agreementEntry =
                await a3SContext.TermsOfServiceUserAcceptance.SingleOrDefaultAsync(
                    x =>
                        x.TermsOfServiceId == termsOfServiceId && x.UserId == userId);

            if (agreementEntry == null)
            {
                agreementEntry = new TermsOfServiceUserAcceptanceModel()
                {
                    UserId = userId,
                    TermsOfServiceId = termsOfServiceId
                };
                a3SContext.TermsOfServiceUserAcceptance.Add(agreementEntry);
                await a3SContext.SaveChangesAsync();
            }
        }

        private async Task StoreEncryptedValue(UserModel user, string loginProvider, string name, string value)
        {
            await a3SContext.Database.ExecuteSqlRawAsync("UPDATE _a3s.application_user_token SET value = _a3s.pgp_sym_encrypt({0}, {1}) where user_id = {2} and login_provider = {3} and name = {4};",
                value,
                encryptionKey,
                user.Id,
                loginProvider,
                name);

            await a3SContext.SaveChangesAsync();
        }

        private Task ReplaceAlreadyHashedCodesAsync(UserModel user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
        {
            var mergedCodes = string.Join(";", recoveryCodes);
            return SetTokenAsync(user, tokenOptions.GetAspNetUserStoreProviderName(), tokenOptions.GetRecoverCodesName(), mergedCodes, cancellationToken);
        }

    }
}
