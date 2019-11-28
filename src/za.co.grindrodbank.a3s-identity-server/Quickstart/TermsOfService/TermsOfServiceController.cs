using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using za.co.grindrodbank.a3s.Managers;
using za.co.grindrodbank.a3sidentityserver.Quickstart.UI;

namespace za.co.grindrodbank.a3sidentityserver.Quickstart.TermsOfService
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
    }
}
