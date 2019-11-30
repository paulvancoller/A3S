/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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

        public TermsOfServiceController(
            CustomUserManager userManager,
            ITermsOfServiceRepository termsOfServiceRepository,
            IConfiguration configuration,
            IIdentityServerInteractionService interaction,
            IEventService events,
            IClientStore clientStore,
            IArchiveHelper archiveHelper)
        {
            this.userManager = userManager;
            this.termsOfServiceRepository = termsOfServiceRepository;
            this.configuration = configuration;
            this.interaction = interaction;
            this.events = events;
            this.clientStore = clientStore;
            this.archiveHelper = archiveHelper;
        }

        /// <summary>
        /// Entry point into the terms of service workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string returnUrl)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            if (user == null)
                throw new AuthenticationException("Invalid login data");

            List<Guid> outstandingTerms = await termsOfServiceRepository.GetAllOutstandingAgreementsByUserAsync(Guid.Parse(user.Id));
            if (outstandingTerms.Count == 0)
                return await CompleteTokenRetrievalProcess(returnUrl, user);

            var vm = await BuildTermsOfServiceViewModel(returnUrl, user, outstandingTerms);
            return View(vm);
        }






        /************************************************/
        /* helper APIs for the TermsOfServiceController */
        /************************************************/
        private async Task<TermsOfServiceViewModel> BuildTermsOfServiceViewModel(string returnUrl, UserModel user, List<Guid> outstandingTerms)
        {
            var termsOfService = await termsOfServiceRepository.GetByIdAsync(outstandingTerms[0], includeRelations: false, includeFileContents: true);

            if (termsOfService == null)
                throw new ItemNotFoundException($"Terms of service entry '{outstandingTerms[0]}' not found.");

            return new TermsOfServiceViewModel()
            {
                TermCount = outstandingTerms.Count,
                TermName = termsOfService.AgreementName,
                TermsOfServiceId = termsOfService.Id,
                CssContents = termsOfService.CssContents,
                HtmlContents = termsOfService.HtmlContents
            };
        }


        private bool ShowAfterSuccessManagementScreen()
        {
            bool showAfterSuccessManagementScreen = false;

            if (configuration.GetSection("TwoFactorAuthentication").GetValue<bool>("AuthenticatorEnabled") == true)
                showAfterSuccessManagementScreen = true;

            return showAfterSuccessManagementScreen;
        }

        private async Task<IActionResult> CompleteTokenRetrievalProcess(string returnUrl, UserModel user)
        {
            // Redirect to after success management screen if applicable
            if (ShowAfterSuccessManagementScreen())
                return RedirectToAction("LoginSuccessful", new { redirectUrl = returnUrl, show2FARegMessage = true });

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
    }
}
