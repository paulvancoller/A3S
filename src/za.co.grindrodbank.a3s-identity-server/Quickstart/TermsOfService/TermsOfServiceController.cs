/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using za.co.grindrodbank.a3s.Managers;
using za.co.grindrodbank.a3sidentityserver.Quickstart.UI;

namespace za.co.grindrodbank.a3sidentityserver.Quickstart.UI
{
    [SecurityHeaders]
    [Authorize]
    public class TermsOfServiceController : Controller
    {
        private readonly CustomUserManager userManager;

        public TermsOfServiceController(CustomUserManager userManager)
        {
            this.userManager = userManager;
        }

        /// <summary>
        /// Entry point into the terms of service workflow
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index(string returnUrl)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            if (user == null)
                throw new AuthenticationException("Invalid login data");

            var vm = BuildTermsOfServiceViewModel(returnUrl);
            return View(vm);
        }






        /************************************************/
        /* helper APIs for the TermsOfServiceController */
        /************************************************/
        private TermsOfServiceViewModel BuildTermsOfServiceViewModel(string returnUrl)
        {

            return new TermsOfServiceViewModel();
        }

    }
}
