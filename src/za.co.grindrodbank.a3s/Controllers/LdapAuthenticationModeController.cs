/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.AbstractApiControllers;
using System.Security.Claims;
using za.co.grindrodbank.a3s.Helpers;
using AutoMapper;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Controllers
{
    public class LdapAuthenticationModeController : LdapAuthenticationModeApiController
    {
        private readonly ILdapAuthenticationModeService authenticationModeService;
        private readonly IOrderByHelper orderByHelper;
        private readonly IPaginationHelper paginationHelper;
        private readonly IMapper mapper;

        public LdapAuthenticationModeController(ILdapAuthenticationModeService authenticationModeService, IOrderByHelper orderByHelper, IPaginationHelper paginationHelper, IMapper mapper)
        {
            this.authenticationModeService = authenticationModeService;
            this.orderByHelper = orderByHelper;
            this.paginationHelper = paginationHelper;
            this.mapper = mapper;
        }

        [Authorize(Policy = "permission:a3s.ldapAuthenticationModes.create")]
        public override async Task<IActionResult> CreateLdapAuthenticationModeAsync([FromBody] LdapAuthenticationModeSubmit ldapAuthenticationModeSubmit)
        {
            var loggedOnUser = ClaimsHelper.GetScalarClaimValue<Guid>(User, ClaimTypes.NameIdentifier, Guid.Empty);
            return Ok(await authenticationModeService.CreateAsync(ldapAuthenticationModeSubmit, loggedOnUser));
        }

        [Authorize(Policy = "permission:a3s.ldapAuthenticationModes.read")]
        public override async Task<IActionResult> GetLdapAuthenticationModeAsync([FromRoute, Required] Guid ldapAuthenticationModeId)
        {
            var authenticationMode = await authenticationModeService.GetByIdAsync(ldapAuthenticationModeId);

            if (authenticationMode == null)
                return NotFound();

            return Ok(authenticationMode);
        }

        [Authorize(Policy = "permission:a3s.ldapAuthenticationModes.read")]
        public override async Task<IActionResult> ListLdapAuthenticationModesAsync([FromQuery]int page, [FromQuery][Range(1, 20)]int size, [FromQuery][StringLength(255, MinimumLength = 0)]string filterName, [FromQuery]string orderBy)
        {
            List<KeyValuePair<string, string>> orderByKeyValueList = orderByHelper.ConvertCommaSeparateOrderByStringToKeyValuePairList(orderBy);
            // Validate only correct order by components were supplied.
            orderByHelper.ValidateOrderByListOnlyContainsCertainElements(orderByKeyValueList, new List<string> { "name" });
            PaginatedResult<LdapAuthenticationModeModel> paginatedResult = await authenticationModeService.GetPaginatedListAsync(size, page, filterName, orderByKeyValueList);
            // Generate a K-V pair of all the current applied filters sent to the controller so that pagination header URLs can include them.
            List<KeyValuePair<string, string>> currrentFilters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("filterName", filterName)
            };

            paginationHelper.AddPaginationHeaderMetaDataToResponse(paginatedResult, currrentFilters, orderBy, "ListLdapAuthenticationModes", Url, Response);

            return Ok(mapper.Map<List<LdapAuthenticationMode>>(paginatedResult.Results));
        }

        [Authorize(Policy = "permission:a3s.ldapAuthenticationModes.update")]
        public override async Task<IActionResult> TestLdapAuthenticationModeAsync([FromBody] LdapAuthenticationModeSubmit ldapAuthenticationModeSubmit)
        {
            return Ok(await authenticationModeService.TestAsync(ldapAuthenticationModeSubmit));
        }

        [Authorize(Policy = "permission:a3s.ldapAuthenticationModes.update")]
        public override async Task<IActionResult> UpdateLdapAuthenticationModeAsync([FromRoute, Required] Guid ldapAuthenticationModeId, [FromBody] LdapAuthenticationModeSubmit ldapAuthenticationModeSubmit)
        {
            if (ldapAuthenticationModeId == Guid.Empty || ldapAuthenticationModeSubmit.Uuid == Guid.Empty)
                return BadRequest();

            var loggedOnUser = ClaimsHelper.GetScalarClaimValue<Guid>(User, ClaimTypes.NameIdentifier, Guid.Empty);
            return Ok(await authenticationModeService.UpdateAsync(ldapAuthenticationModeSubmit, loggedOnUser));
        }

        [Authorize(Policy = "permission:a3s.ldapAuthenticationModes.delete")]
        public async override Task<IActionResult> DeleteLdapAuthenticationModeAsync([FromRoute, Required] Guid ldapAuthenticationModeId)
        {
            await authenticationModeService.DeleteAsync(ldapAuthenticationModeId);
            return NoContent();
        }
    }
}
