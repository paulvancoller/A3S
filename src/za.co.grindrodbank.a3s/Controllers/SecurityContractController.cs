/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.AbstractApiControllers;
using System;
using za.co.grindrodbank.a3s.Helpers;
using System.Security.Claims;

namespace za.co.grindrodbank.a3s.Controllers
{
    [ApiController]
    public class SecurityContractController : SecurityContractApiController
    {
        private readonly ISecurityContractService securityContractService;

        public SecurityContractController(ISecurityContractService securityContractService)
        {
            this.securityContractService = securityContractService;
        }

        [Authorize(Policy = "permission:a3s.securityContracts.update")]
        public async override Task<IActionResult> ApplySecurityContractAsync([FromBody] SecurityContract securityContract)
        {
            if(securityContract == null)
                return BadRequest();

            var loggedOnUser = ClaimsHelper.GetScalarClaimValue<Guid>(User, ClaimTypes.NameIdentifier, Guid.Empty);
            await securityContractService.ApplySecurityContractDefinitionAsync(securityContract, loggedOnUser);

            return NoContent();
        }

        [Authorize(Policy = "permission:a3s.securityContracts.read")]
        public async override Task<IActionResult> GetSecurityContractAsync()
        {
            return Ok(await securityContractService.GetSecurityContractDefinitionAsync());
        }

        public async override Task<IActionResult> ValidateSecurityContractAsync([FromBody] SecurityContract securityContract)
        {
            if (securityContract == null)
                return BadRequest();

            var loggedOnUser = ClaimsHelper.GetScalarClaimValue<Guid>(User, ClaimTypes.NameIdentifier, Guid.Empty);
            // This service will throw a custom exception (which is not really an exception) for returning the response.
            await securityContractService.ApplySecurityContractDefinitionAsync(securityContract, loggedOnUser, true);

            return NoContent();
        }
    }
}