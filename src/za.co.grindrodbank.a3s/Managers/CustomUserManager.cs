/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Stores;

namespace za.co.grindrodbank.a3s.Managers
{
    public class CustomUserManager : UserManager<UserModel>
    {
        private readonly CustomUserStore store;
        
        public CustomUserManager(IUserStore<UserModel> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<UserModel> passwordHasher,
            IEnumerable<IUserValidator<UserModel>> userValidators, IEnumerable<IPasswordValidator<UserModel>> passwordValidators, ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors, IServiceProvider services,ILogger<UserManager<UserModel>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            this.store = (CustomUserStore)store;
        }

        public async Task SetAuthenticatorTokenVerifiedAsync(UserModel user)
        {
            ThrowIfDisposed();
            await store.SetAuthenticatorTokenVerifiedAsync(user);
        }

        public virtual bool IsAuthenticatorTokenVerified(UserModel user)
        {
            ThrowIfDisposed();
            return store.IsAuthenticatorTokenVerified(user);
        }

        public virtual async Task AgreeToTermsOfService(UserModel user, Guid termsOfServiceId)
        {
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (termsOfServiceId == Guid.Empty)
                throw new ArgumentNullException(nameof(termsOfServiceId));

            await store.AgreeToTermsOfService(user.Id, termsOfServiceId);
        }

        public override async Task<bool> CheckPasswordAsync(UserModel user, string password)
        {
            ThrowIfDisposed();
            var passwordStore = GetPasswordStore();
            if (user == null)
            {
                return false;
            }

            var result = await VerifyPasswordAsync(passwordStore, user, password);
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                await UpdatePasswordHash(passwordStore, user, password, validatePassword: false);
                await UpdateUserAsync(user);
            }

            var success = result != PasswordVerificationResult.Failed;
            if (!success)
            {
                Logger.LogWarning(0, "Invalid password for user {userId}.", await GetUserIdAsync(user));
            }
            return success;
        }

        private IUserPasswordStore<UserModel> GetPasswordStore()
        {
            var cast = Store as IUserPasswordStore<UserModel>;
            if (cast == null)
            {
                throw new NotSupportedException();
            }
            return cast;
        }

        protected override Task<IdentityResult> UpdatePasswordHash(UserModel user, string newPassword, bool validatePassword)
    => UpdatePasswordHash(GetPasswordStore(), user, newPassword, validatePassword);

        private async Task<IdentityResult> UpdatePasswordHash(IUserPasswordStore<UserModel> passwordStore,
            UserModel user, string newPassword, bool validatePassword = true)
        {
            if (validatePassword)
            {
                var validate = await ValidatePasswordAsync(user, newPassword);
                if (!validate.Succeeded)
                {
                    return validate;
                }
            }
            var hash = newPassword != null ? PasswordHasher.HashPassword(user, newPassword) : null;
            await passwordStore.SetPasswordHashAsync(user, hash, CancellationToken);
            await UpdateSecurityStampInternal(user);
            return IdentityResult.Success;
        }

        private async Task UpdateSecurityStampInternal(UserModel user)
        {
            if (SupportsUserSecurityStamp)
            {
                await GetSecurityStore().SetSecurityStampAsync(user, NewSecurityStamp(), CancellationToken);
            }
        }

        private IUserSecurityStampStore<UserModel> GetSecurityStore()
        {
            var cast = Store as IUserSecurityStampStore<UserModel>;
            if (cast == null)
            {
                throw new NotSupportedException();
            }
            return cast;
        }

        private static string NewSecurityStamp()
        {
            byte[] bytes = new byte[20];
            _rng.GetBytes(bytes);
            return Base32.ToBase32(bytes);
        }

        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
    }
}
