/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.AbstractApiControllers;
using za.co.grindrodbank.a3s.Helpers;
using za.co.grindrodbank.a3s.Services;

namespace za.co.grindrodbank.a3s.Controllers
{
    public class TermsOfServiceController : TermsOfServiceApiController
    {
        private readonly ITermsOfServiceService termsOfServiceService;

        public TermsOfServiceController(ITermsOfServiceService termsOfServiceService)
        {
            this.termsOfServiceService = termsOfServiceService;
        }

        [Authorize(Policy = "permission:a3s.termsOfService.create")]
        public async override Task<IActionResult> CreateTermsOfServiceAsync([FromBody] TermsOfServiceSubmit termsOfServiceSubmit)
        {
            var loggedOnUser = ClaimsHelper.GetScalarClaimValue<Guid>(User, ClaimTypes.NameIdentifier, Guid.Empty);
            return Ok(await termsOfServiceService.CreateAsync(termsOfServiceSubmit, loggedOnUser));
        }

        [Authorize(Policy = "permission:a3s.termsOfService.delete")]
        public async override Task<IActionResult> DeleteTermsOfServiceAsync([FromRoute, Required] Guid termsOfServiceId)
        {
            await termsOfServiceService.DeleteAsync(termsOfServiceId);
            return NoContent();
        }

        [Authorize(Policy = "permission:a3s.termsOfService.read")]
        public async override Task<IActionResult> GetTermsOfServiceAsync([FromRoute, Required] Guid termsOfServiceId)
        {
            if (termsOfServiceId == Guid.Empty)
                return BadRequest();

            var termsOfService = await termsOfServiceService.GetByIdAsync(termsOfServiceId, true);

            if (termsOfService == null)
                return NotFound();

            return Ok(termsOfService);
        }

        [Authorize(Policy = "permission:a3s.termsOfService.read")]
        public async override Task<IActionResult> ListTermsOfServicesAsync([FromQuery] int page, [FromQuery, Range(1, 20)] int size, [FromQuery] List<string> orderBy)
        {
            return Ok(await termsOfServiceService.GetListAsync());
        }
    }
}
