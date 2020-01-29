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
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.AbstractApiControllers;
using za.co.grindrodbank.a3s.Helpers;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;

namespace za.co.grindrodbank.a3s.Controllers
{
    public class TermsOfServiceController : TermsOfServiceApiController
    {
        private readonly ITermsOfServiceService termsOfServiceService;
        private readonly IOrderByHelper orderByHelper;
        private readonly IPaginationHelper paginationHelper;
        private readonly IMapper mapper;

        public TermsOfServiceController(ITermsOfServiceService termsOfServiceService, IOrderByHelper orderByHelper, IPaginationHelper paginationHelper, IMapper mapper)
        {
            this.termsOfServiceService = termsOfServiceService;
            this.orderByHelper = orderByHelper;
            this.paginationHelper = paginationHelper;
            this.mapper = mapper;
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
        public async override Task<IActionResult> ListTermsOfServicesAsync([FromQuery]int page, [FromQuery][Range(1, 20)]int size, [FromQuery]bool includeRelations, [FromQuery][StringLength(255, MinimumLength = 0)]string filterAgreementName, [FromQuery]string orderBy)
        {
            List<KeyValuePair<string, string>> orderByKeyValueList = orderByHelper.ConvertCommaSeparateOrderByStringToKeyValuePairList(orderBy);
            // Validate only correct order by components were supplied.
            orderByHelper.ValidateOrderByListOnlyContainsCertainElements(orderByKeyValueList, new List<string> { "agreementName" });
            PaginatedResult<TermsOfServiceModel> paginatedResult = await termsOfServiceService.GetPaginatedListAsync(page, size, includeRelations, filterAgreementName, orderByKeyValueList);
            // Generate a K-V pair of all the current applied filters sent to the controller so that pagination header URLs can include them.
            List<KeyValuePair<string, string>> currrentFilters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("filterAgreementName", filterAgreementName)
            };

            paginationHelper.AddPaginationHeaderMetaDataToResponse(paginatedResult, currrentFilters, orderBy, "ListTermsOfServices", Url, Response);

            return Ok(mapper.Map<List<TermsOfServiceListItem>>(paginatedResult.Results));
        }
    }
}
