/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using za.co.grindrodbank.a3s;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Helpers;
using za.co.grindrodbank.a3s.Managers;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;

namespace za.co.grindrodbank.a3sidentityserver.Quickstart.UI
{
    [SecurityHeaders]
    [Authorize]
    public class TermsOfServiceController : Controller
    {
        private readonly CustomUserManager userManager;
        private readonly ITermsOfServiceRepository termsOfServiceRepository;
        private readonly IConfiguration configuration;
        private readonly IIdentityServerInteractionService interaction;
        private readonly IEventService events;
        private readonly IClientStore clientStore;
        private readonly IArchiveHelper archiveHelper;
        private readonly SignInManager<UserModel> signInManager;

        public TermsOfServiceController(
            CustomUserManager userManager,
            ITermsOfServiceRepository termsOfServiceRepository,
            IConfiguration configuration,
            IIdentityServerInteractionService interaction,
            IEventService events,
            IClientStore clientStore,
            IArchiveHelper archiveHelper,
            SignInManager<UserModel> signInManager)
        {
            this.userManager = userManager;
            this.termsOfServiceRepository = termsOfServiceRepository;
            this.configuration = configuration;
            this.interaction = interaction;
            this.events = events;
            this.clientStore = clientStore;
            this.archiveHelper = archiveHelper;
            this.signInManager = signInManager;
        }

        /// <summary>
        /// Entry point into the terms of service workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string returnUrl, int initialAgreementCount)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            if (user == null)
                throw new AuthenticationException("Invalid login data");

            List<Guid> outstandingTerms = await termsOfServiceRepository.GetAllOutstandingAgreementsByUserAsync(Guid.Parse(user.Id));
            if (outstandingTerms.Count == 0)
                return await CompleteTokenRequest(returnUrl, user);

            var vm = await BuildTermsOfServiceViewModel(returnUrl, outstandingTerms, initialAgreementCount);
            return View(vm);
        }

        /// <summary>
        /// Entry point into the terms of service workflow
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TermsOfServiceInputModel model, string button, string returnUrl)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            if (user == null)
                throw new AuthenticationException("Invalid login data");

            if (button != "accept" || !model.Accepted)
                return await CancelTokenRequest(model.ReturnUrl);

            await userManager.AgreeToTermsOfService(user, model.TermsOfServiceId);

            return RedirectToAction("Index", new { returnUrl, initialAgreementCount = model.InitialAgreementCount });
        }





        /************************************************/
        /* helper APIs for the TermsOfServiceController */
        /************************************************/
        private async Task<TermsOfServiceViewModel> BuildTermsOfServiceViewModel(string returnUrl, List<Guid> outstandingTerms, int initialAgreementCount)
        {
            var termsOfService = await termsOfServiceRepository.GetByIdAsync(outstandingTerms[0], includeRelations: false, includeFileContents: true);

            if (termsOfService == null)
                throw new ItemNotFoundException($"Terms of service entry '{outstandingTerms[0]}' not found.");

            return new TermsOfServiceViewModel()
            {
                AgreementCount = outstandingTerms.Count,
                AgreementName = termsOfService.AgreementName,
                TermsOfServiceId = termsOfService.Id,
                CssContents = LocaliseStyleSheetItems(termsOfService.CssContents),
                HtmlContents = termsOfService.HtmlContents,
                ReturnUrl = returnUrl,
                InitialAgreementCount = (initialAgreementCount > 0 ? initialAgreementCount : outstandingTerms.Count)
            };
        }

        private string LocaliseStyleSheetItems(string stylesheetContents)
        {
            if (string.IsNullOrWhiteSpace(stylesheetContents))
                return string.Empty;

            StringBuilder alteredStylesheetBuilder = new StringBuilder();

            MatchCollection matches = Regex.Matches(stylesheetContents, A3SConstants.CSS_STYLE_RULES_REGEX, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
                alteredStylesheetBuilder.Append($"#terms-body {match.Value}\n");

            return alteredStylesheetBuilder.ToString();
        }

        private bool ShowAfterSuccessManagementScreen()
        {
            bool showAfterSuccessManagementScreen = false;

            if (configuration.GetSection("TwoFactorAuthentication").GetValue<bool>("AuthenticatorEnabled") == true)
                showAfterSuccessManagementScreen = true;

            return showAfterSuccessManagementScreen;
        }

        private async Task<IActionResult> CompleteTokenRequest(string returnUrl, UserModel user)
        {
            // Redirect to after success management screen if applicable
            if (ShowAfterSuccessManagementScreen())
                return RedirectToAction("LoginSuccessful", "Account", new { redirectUrl = returnUrl, show2FARegMessage = true });

            // check if we are in the context of an authorization request
            var context = await interaction.GetAuthorizationContextAsync(returnUrl);

            await events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName));

            if (context != null)
            {
                if (await clientStore.IsPkceClientAsync(context.ClientId))
                {
                    // if the client is PKCE then we assume it's native, so this change in how to
                    // return the response is for better UX for the end user.
                    return View("Redirect", new RedirectViewModel { RedirectUrl = returnUrl });
                }

                // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                return Redirect(returnUrl);
            }

            // request for a local page
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (string.IsNullOrEmpty(returnUrl))
                return Redirect("~/");

            // user might have clicked on a malicious link - should be logged
            throw new Exception("invalid return URL");
        }

        private async Task<IActionResult> CancelTokenRequest(string returnUrl)
        {
            if (User?.Identity.IsAuthenticated == true)
            {
                await signInManager.SignOutAsync();
                await events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            var context = await interaction.GetAuthorizationContextAsync(returnUrl);

            if (context != null)
            {
                // Access denied
                await interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                if (await clientStore.IsPkceClientAsync(context.ClientId))
                    return View("Redirect", new RedirectViewModel { RedirectUrl = returnUrl });

                return Redirect(returnUrl);
            }
            else
                return Redirect("~/");
        }
    }
}
