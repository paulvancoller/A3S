/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using za.co.grindrodbank.a3s.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Configuration;
using za.co.grindrodbank.a3sidentityserver.Exceptions;
using System.Collections.Generic;
using za.co.grindrodbank.a3s.Managers;
using za.co.grindrodbank.a3s.Extensions;
using System.Security.Authentication;

namespace za.co.grindrodbank.a3sidentityserver.Quickstart.UI
{
    [SecurityHeaders]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly CustomUserManager _userManager;
        private readonly SignInManager<UserModel> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private readonly UrlEncoder _urlEncoder;
        private readonly IConfiguration _configuration;
        private readonly TokenOptions _tokenOptions = new TokenOptions();

        private const string AUTHENTICATOR_URI_FORMAT = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        private const int RECOVERY_CODE_MAX = 10;

        public AccountController(
            CustomUserManager userManager,
            SignInManager<UserModel> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            UrlEncoder urlEncoder,
            IConfiguration configuration
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _urlEncoder = urlEncoder;
            _configuration = configuration;
        }

        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToAction("Challenge", "External", new { provider = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {

            // the user clicked the "cancel" button
            if (button != "login")
                return await CancelTokenRequest(model.ReturnUrl);

            if (ModelState.IsValid)
            {
                // check if we are in the context of an authorization request
                var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberLogin, lockoutOnFailure: true);

                if (result.RequiresTwoFactor)
                {
                    return await RedirectTo2FA(model.ReturnUrl, model.Username);
                }

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.Username);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName));

                    // Redirect to 2fa offer screen if any options are available
                    if (TwoFACompulsary())
                    {
                        if (context != null)
                            await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                        return RedirectToAction("Register2FA", new { redirectUrl = model.ReturnUrl });
                    }

                    return RedirectToAction("Index", "TermsOfService", new { returnUrl = model.ReturnUrl });
                }
                else
                {
                    await HandleFailureLoginResult(result, model.Username);
                }
            }

            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);
        }

        /// <summary>
        /// Entry point into login successful workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> LoginSuccessful(string redirectUrl, bool show2FARegMessage)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
                throw new AuthenticationException("Invalid login data");

            var vm = new LoginSuccessfulViewModel()
            {
                RedirectUrl = redirectUrl,
                Show2FARegMessage = show2FARegMessage,
                TwoFAAlreadyEnabled = user.TwoFactorEnabled,
                TwoFAUrl = Url.Action("Register2FA", new { redirectUrl, userId = GetLoggedOnUserId() }),
                UserId = GetLoggedOnUserId()
            };

            return View(vm);
        }

        /// <summary>
        /// Entry point into registration of 2FA workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Register2FA(string redirectUrl)
        {
            var vm = await GetRegister2FAViewModel(redirectUrl, GetLoggedOnUserId());
            return View(vm);
        }

        /// <summary>
        /// Entry point into cancel registration of 2FA workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Cancel2FARegistration(string returnUrl)
        {
            return await CancelTokenRequest(returnUrl);
        }

        /// <summary>
        /// Entry point into registration of 2FA Authenticator workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Register2FAAuthenticator(string redirectUrl)
        {
            var vm = await GetRegister2FAAuthenticatorViewModel(redirectUrl, GetLoggedOnUserId(), true);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register2FAAuthenticator(TwoFactorInputModel model, string redirectUrl = null)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (ModelState.IsValid)
            {
                model.OTP = model.OTP.Replace(" ", string.Empty).Replace("-", string.Empty);
                var is2FaTokenValid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, model.OTP);

                if (is2FaTokenValid)
                {
                    await _userManager.SetTwoFactorEnabledAsync(user, true);
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    await _userManager.SetAuthenticatorTokenVerifiedAsync(user);

                    return RedirectToAction("Register2FAAuthenticatorComplete", new { redirectUrl = redirectUrl });
                }
                else
                    ModelState.AddModelError(string.Empty, "Token is invalid");
            }

            return View(await GetRegister2FAAuthenticatorViewModel(redirectUrl, user.Id, false));
        }

        /// <summary>
        /// Entry point into deregistration of 2FA Authenticator workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Deregister2FAAuthenticator(string redirectUrl)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
                throw new AuthenticationException("Invalid login info.");

            await _userManager.RemoveAuthenticationTokenAsync(user, _tokenOptions.GetAspNetUserStoreProviderName(), _tokenOptions.GetRecoverCodesName());
            await _userManager.RemoveAuthenticationTokenAsync(user, _tokenOptions.GetAspNetUserStoreProviderName(), _tokenOptions.GetAuthenticatorKeyName());
            await UpdateUser2faStatus(user.Id);
            
            return RedirectToAction("Register2FA", new { redirectUrl, userId = user.Id });
        }

        /// <summary>
        /// Entry point into recovery codes reset workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ResetRecoveryCodes(string redirectUrl)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
                throw new AuthenticationException("Invalid login info.");

            if (!_userManager.IsAuthenticatorTokenVerified(user))
                throw new TwoFactorAuthException("Invalid authenticator data");
            
            TempData["recoveryCodes"] = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, RECOVERY_CODE_MAX);

            return RedirectToAction("DisplayResetRecoveryCodes", new { redirectUrl });
        }

        /// <summary>
        /// Entry point into display recovery codes reset workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DisplayResetRecoveryCodes(string redirectUrl)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
                throw new AuthenticationException("Invalid login info.");

            if (!_userManager.IsAuthenticatorTokenVerified(user))
                throw new TwoFactorAuthException("Invalid authenticator data");

            if (TempData["recoveryCodes"] == null)
                TempData["recoveryCodes"] = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, RECOVERY_CODE_MAX);

            IEnumerable<string> recoveryCodes = (IEnumerable<string>)TempData["recoveryCodes"];
            TempData["recoveryCodes"] = recoveryCodes;

            return View(new ResetRecoveryCodesModel()
            {
                RedirectUrl = redirectUrl,
                RecoveryCodes = recoveryCodes
            });
        }

        /// <summary>
        /// Entry point into registration of 2FA Authenticator completion workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Register2FAAuthenticatorComplete(string redirectUrl)
        {
            var vm = await GetRegister2FAAuthenticatorCompleteViewModel(redirectUrl, GetLoggedOnUserId());
            return View(vm);
        }

        // POST: /Login/Verify2FAAuthenticator
        /// <summary>
        /// Entry point into the two factor auth authenticator workflow
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Verify2FAAuthenticator(string redirectUrl, string username)
        {
            var vm = await GetTwoFactorViewModelAsync(redirectUrl, username);
            return View(vm);
        }

        // POST: /Login/Verify2FAAuthenticator
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify2FAAuthenticator(TwoFactorInputModel model, string button)
        {
            // the user clicked the "cancel" button
            if (button != "validate")
                return await CancelTokenRequest(model.RedirectUrl);

            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.RedirectUrl);

            if (ModelState.IsValid)
            {
                model.OTP = model.OTP.Replace(" ", string.Empty).Replace("-", string.Empty);

                Microsoft.AspNetCore.Identity.SignInResult result;
                if (model.IsRecoveryCode)
                    result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(model.OTP);
                else
                    result = await _signInManager.TwoFactorAuthenticatorSignInAsync(model.OTP, false, false);

                if (result.Succeeded)
                    return RedirectToAction("Index", "TermsOfService", new { returnUrl = model.RedirectUrl });

                if (result.IsLockedOut)
                    ModelState.AddModelError(string.Empty, "Your account is locked out");
                else
                    ModelState.AddModelError(string.Empty, $"{(model.IsRecoveryCode ? "Recovery code" : "OTP")} is invalid");

                ModelState.Remove("OTP");
            }

            // something went wrong, show form with error
            return View(await GetTwoFactorViewModelAsync(model.RedirectUrl, model.Username));
        }

        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await _signInManager.SignOutAsync();

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return View("LoggedOut", vm);
        }



        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/

        private async Task HandleFailureLoginResult(Microsoft.AspNetCore.Identity.SignInResult result, string username)
        {
            if (result.IsLockedOut)
            {
                await _events.RaiseAsync(new UserLoginFailureEvent(username, "account locked out"));
                ModelState.AddModelError(string.Empty, AccountOptions.AccountLockedOutErrorMessage);
            }
            else
            {
                await _events.RaiseAsync(new UserLoginFailureEvent(username, "invalid credentials"));
                ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
            }
        }

        private async Task UpdateUser2faStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new AuthenticationException("Invalid user");

            if (!(_userManager.IsAuthenticatorTokenVerified(user)))
                await _userManager.SetTwoFactorEnabledAsync(user, false);
        }

        private async Task<IActionResult> RedirectTo2FA(string returnUrl, string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                throw new AuthenticationException("Invalid login data");

            if (!string.IsNullOrWhiteSpace(await _userManager.GetAuthenticatorKeyAsync(user)))
                return RedirectToAction("Verify2FAAuthenticator", new { redirectUrl = returnUrl, username });

            throw new TwoFactorAuthException("Invalid two-factor configration");
        }

        private string GetLoggedOnUserId()
        {
            return HttpContext.User.GetSubjectId();
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null)
            {
                // this is meant to short circuit the UI and only trigger the one external IdP
                return new LoginViewModel
                {
                    EnableLocalLogin = false,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                    ExternalProviders = new ExternalProvider[] { new ExternalProvider { AuthenticationScheme = context.IdP } }
                };
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null ||
                            (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }

        private async Task<RegisterTwoFactorViewModel> GetRegister2FAViewModel(string redirectUrl, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new AuthenticationException("Invalid login data");

            return new RegisterTwoFactorViewModel()
            {
                RedirectUrl = redirectUrl,
                AllowRegisterAuthenticator = ShouldAllowAuthenticatorRegistration(userId),
                HasAuthenticator = _userManager.IsAuthenticatorTokenVerified(user),
                TwoFACompulsary = TwoFACompulsary()
            };

        }

        private bool ShouldAllowAuthenticatorRegistration(string userId)
        {
            if (!_configuration.GetSection("TwoFactorAuthentication").GetValue<bool>("AuthenticatorEnabled"))
                return false;

            return true;
        }

        private bool TwoFACompulsary()
        {
            return _configuration.GetSection("TwoFactorAuthentication").GetValue<bool>("OrganizationEnforced");
        }

        private bool ShowAfterSuccessManagementScreen()
        {
            bool showAfterSuccessManagementScreen = false;

            if (_configuration.GetSection("TwoFactorAuthentication").GetValue<bool>("AuthenticatorEnabled") == true)
                showAfterSuccessManagementScreen = true;

            return showAfterSuccessManagementScreen;
        }

        private async Task<RegisterTwoFactorAuthenticatorViewModel> GetRegister2FAAuthenticatorViewModel(string redirectUrl, string userId, bool resetUnverifiedAuthenticatorKey)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new AuthenticationException("Invalid login data");

            // Load the authenticator key & QR code URI to display on the form]
            string unformattedKey = string.Empty;
            if (_userManager.IsAuthenticatorTokenVerified(user) || !resetUnverifiedAuthenticatorKey)
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);

            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            string sharedKey = FormatKey(unformattedKey);
            string qrCode = GenerateQrCodeUri(user.Email, unformattedKey);

            return new RegisterTwoFactorAuthenticatorViewModel()
            {
                QrCode = qrCode,
                SharedKey = sharedKey,
                TwoFACompulsary = TwoFACompulsary(),
                Cancel2FARegistrationUrl = Url.Action("Cancel2FARegistration", new { redirectUrl }),
                RedirectUrl = redirectUrl
            };
        }

        private async Task<RegisterTwoFactorAuthenticatorCompleteViewModel> GetRegister2FAAuthenticatorCompleteViewModel(string redirectUrl, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new AuthenticationException("Invalid login data");

            IEnumerable<string> recoveryCodes = null;
            if (await _userManager.CountRecoveryCodesAsync(user) == 0)
                recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, RECOVERY_CODE_MAX);
            else
                recoveryCodes = (IEnumerable<string>)TempData["recoveryCodes"];

            TempData["recoveryCodes"] = recoveryCodes;
            await _userManager.SetAuthenticatorTokenVerifiedAsync(user);
            return new RegisterTwoFactorAuthenticatorCompleteViewModel()
            {
                RecoveryCodes = recoveryCodes,
                RedirectUrl = redirectUrl
            };

        }
        private async Task<TwoFactorViewModel> GetTwoFactorViewModelAsync(string redirectUrl, string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                throw new AuthenticationException("Invalid login data");

            return new TwoFactorViewModel
            {
                RedirectUrl = redirectUrl,
                Username = username,
                AuthenticatorConfigured = _userManager.IsAuthenticatorTokenVerified(user),
                OTP = string.Empty
            };
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                AUTHENTICATOR_URI_FORMAT,
                _urlEncoder.Encode("A3S"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }


        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private async Task<IActionResult> CancelTokenRequest(string returnUrl)
        {
            if (User?.Identity.IsAuthenticated == true)
            {
                await _signInManager.SignOutAsync();
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // Only redirect to return URL if a valid context is loaded (based on the registered URL).
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

            if (context != null)
            {
                // Access denied
                await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                if (await _clientStore.IsPkceClientAsync(context.ClientId))
                    return View("Redirect", new RedirectViewModel { RedirectUrl = returnUrl });

                return Redirect(returnUrl);
            }
            else
                return Redirect("~/");
        }
    }
}
