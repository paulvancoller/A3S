/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Helpers;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Services;

namespace za.co.grindrodbank.a3s.Controllers
{
    public class ConsentOfServiceController : ConsentOfServiceApiController
    {
        private readonly IConsentOfServiceService consentOfServiceService;

        public ConsentOfServiceController(IConsentOfServiceService consentOfServiceService)
        {
            this.consentOfServiceService = consentOfServiceService;
        }

        //[Authorize(Policy = "permission:a3s.ConsentOfService.read")]
        public override async Task<IActionResult> GetCurrentConsentOfServiceAsync()
        {
            var consentOfService = await consentOfServiceService.GetCurrentConsentAsync();

            if (consentOfService == null)
                return NotFound();

            return Ok(consentOfService);
        }

        //[Authorize(Policy = "permission:a3s.ConsentOfService.create")]
        public override async Task<IActionResult> UpdateConsentOfServiceAsync(ConsentOfService consentOfService)
        {
            if (consentOfService == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(consentOfService.ConsentFileData))
            {
                return BadRequest();
            }

            var loggedOnUser = ClaimsHelper.GetScalarClaimValue(User, ClaimTypes.NameIdentifier, Guid.Empty);
            var isUpdated = await consentOfServiceService.UpdateCurrentConsentAsync(consentOfService, loggedOnUser);
            return isUpdated ? NoContent() : throw new OperationFailedException("Save consent css file failed.");
        }
    }
}