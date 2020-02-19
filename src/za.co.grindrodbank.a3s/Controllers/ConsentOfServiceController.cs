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
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using za.co.grindrodbank.a3s.Helpers;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;

namespace za.co.grindrodbank.a3s.Controllers
{
    public class ConsentOfServiceController : ConsentOfServiceApiController
    {
        private readonly IConsentOfServiceService consentOfServiceService;
        private readonly IMapper mapper;
        private readonly IOrderByHelper orderByHelper;
        private readonly IPaginationHelper paginationHelper;

        public ConsentOfServiceController(IConsentOfServiceService consentOfServiceService,
            IOrderByHelper orderByHelper, IPaginationHelper paginationHelper, IMapper mapper)
        {
            this.consentOfServiceService = consentOfServiceService;
            this.orderByHelper = orderByHelper;
            this.paginationHelper = paginationHelper;
            this.mapper = mapper;
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
            var loggedOnUser = ClaimsHelper.GetScalarClaimValue(User, ClaimTypes.NameIdentifier, Guid.Empty);
            return Ok(await consentOfServiceService.UpdateCurrentConsentAsync(consentOfService, loggedOnUser));
        }
    }
}