/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Linq;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;

namespace za.co.grindrodbank.a3s.Managers
{
    /// <summary>
    /// Provides the APIs for user sign in.
    /// </summary>
    /// <typeparam name="TUser">The type encapsulating a user.
    public class CustomSignInManager<TUser> : SignInManager<TUser> where TUser : class
    {
        private readonly A3SContext a3SContext;
        private readonly ILogger<SignInManager<TUser>> logger;
        private readonly ILdapAuthenticationModeRepository ldapAuthenticationModeRepository;
        private readonly ILdapConnectionService ldapConnectionService;

        public CustomSignInManager(UserManager<TUser> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<TUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<TUser>> logger, A3SContext a3SContext, IAuthenticationSchemeProvider authenticationSchemeProvider,
            ILdapAuthenticationModeRepository ldapAuthenticationModeRepository, ILdapConnectionService ldapConnectionService, IUserConfirmation<TUser> userConfirmation)
                : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, authenticationSchemeProvider, userConfirmation)
        {
            this.a3SContext = a3SContext;
            this.logger = logger;
            this.ldapAuthenticationModeRepository = ldapAuthenticationModeRepository;
            this.ldapConnectionService = ldapConnectionService;
        }

        public async override Task<SignInResult> CheckPasswordSignInAsync(TUser user, string password, bool lockoutOnFailure)
        {
            try
            {
                var appUser = (user as UserModel);
                logger.LogInformation($"Password login for user {appUser.UserName}.");

                // Confirm user not deleted
                if (appUser.IsDeleted)
                {
                    logger.LogError($"{appUser.UserName} is a deleted user.");
                    return SignInResult.Failed;
                }

                if (a3SContext != null && a3SContext.User != null && a3SContext.LdapAuthenticationMode != null)
                {
                    appUser = a3SContext.User.FirstOrDefault(u => u.Id == (user as UserModel).Id);

                    if (appUser.LdapAuthenticationModeId != null && appUser.LdapAuthenticationModeId != Guid.Empty)
                        appUser.LdapAuthenticationMode = await ldapAuthenticationModeRepository.GetByIdAsync((Guid)appUser.LdapAuthenticationModeId, true);
                }

                SignInResult signInResult = null;
                if (appUser.LdapAuthenticationMode != null)
                {
                    logger.LogInformation($"LDAP User Detected.");
                    signInResult = ldapConnectionService.Login(appUser, password).Result;
                }
                else
                    signInResult = CheckPasswordSignInOverrideAsync(user, password, lockoutOnFailure).Result;
                    //signInResult = base.CheckPasswordSignInAsync(user, password, lockoutOnFailure).Result;

                return signInResult;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during [LDAPCompatibleSignInManager].[CheckPasswordSignInAsync]");
                return SignInResult.Failed;
            }
        }

        public async Task<SignInResult> CheckPasswordSignInOverrideAsync(TUser user, string password, bool lockoutOnFailure)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var error = await PreSignInCheck(user);
            if (error != null)
            {
                return error;
            }

            if (await UserManager.CheckPasswordAsync(user, password))
            {
                var alwaysLockout = AppContext.TryGetSwitch("Microsoft.AspNetCore.Identity.CheckPasswordSignInAlwaysResetLockoutOnSuccess", out var enabled) && enabled;
                // Only reset the lockout when TFA is not enabled when not in quirks mode
                if (alwaysLockout || !await IsTfaEnabled(user))
                {
                    await ResetLockout(user);
                }

                return SignInResult.Success;
            }
            Logger.LogWarning(2, "User {userId} failed to provide the correct password.", await UserManager.GetUserIdAsync(user));

            if (UserManager.SupportsUserLockout && lockoutOnFailure)
            {
                // If lockout is requested, increment access failed count which might lock out the user
                await UserManager.AccessFailedAsync(user);
                if (await UserManager.IsLockedOutAsync(user))
                {
                    return await LockedOut(user);
                }
            }
            return SignInResult.Failed;
        }

        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password,
            bool isPersistent, bool lockoutOnFailure)
        {
            var user = await UserManager.FindByNameAsync(userName);
            if (user == null)
            {
                return SignInResult.Failed;
            }

            return await PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        }

        private async Task<bool> IsTfaEnabled(TUser user)
    => UserManager.SupportsUserTwoFactor &&
    await UserManager.GetTwoFactorEnabledAsync(user) &&
    (await UserManager.GetValidTwoFactorProvidersAsync(user)).Count > 0;


        protected override async Task<SignInResult> PreSignInCheck(TUser user)
        {
            if (!await CanSignInAsync(user))
            {
                return SignInResult.NotAllowed;
            }
            if (await IsLockedOut(user))
            {
                return await LockedOut(user);
            }
            return null;
        }

    }
}
