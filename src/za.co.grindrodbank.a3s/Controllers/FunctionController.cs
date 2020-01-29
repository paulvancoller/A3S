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
using za.co.grindrodbank.a3s.Helpers;
using System.Security.Claims;
using AutoMapper;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Controllers
{
    public class FunctionController : FunctionApiController
    {
        private readonly IFunctionService functionService;
        private readonly IOrderByHelper orderByHelper;
        private readonly IPaginationHelper paginationHelper;
        private readonly IMapper mapper;

        public FunctionController(IFunctionService functionService, IOrderByHelper orderByHelper, IPaginationHelper paginationHelper, IMapper mapper)
        {
            this.functionService = functionService;
            this.orderByHelper = orderByHelper;
            this.paginationHelper = paginationHelper;
            this.mapper = mapper;
        }

        [Authorize(Policy = "permission:a3s.functions.create")]
        public async override Task<IActionResult> CreateFunctionAsync([FromBody] FunctionSubmit functionSubmit)
        {
            var loggedOnUser = ClaimsHelper.GetScalarClaimValue<Guid>(User, ClaimTypes.NameIdentifier, Guid.Empty);
            return Ok(await functionService.CreateAsync(functionSubmit, loggedOnUser));
        }

        [Authorize(Policy = "permission:a3s.functions.read")]
        public override async Task<IActionResult> GetFunctionAsync([FromRoute, Required] Guid functionId)
        {
            if (functionId == Guid.Empty)
                return BadRequest();

            var function = await functionService.GetByIdAsync(functionId);

            if(function == null)
                return NotFound();

            return Ok(function);
        }

        [Authorize(Policy = "permission:a3s.functions.read")]
        public async override Task<IActionResult> ListFunctionsAsync([FromQuery]int page, [FromQuery][Range(1, 20)]int size, [FromQuery]bool includeRelations, [FromQuery][StringLength(255, MinimumLength = 0)]string filterName, [FromQuery]string orderBy)
        {
            List<KeyValuePair<string, string>> orderByKeyValueList = orderByHelper.ConvertCommaSeparateOrderByStringToKeyValuePairList(orderBy);
            // Validate only correct order by components were supplied.
            orderByHelper.ValidateOrderByListOnlyContainsCertainElements(orderByKeyValueList, new List<string> { "name" });
            PaginatedResult<FunctionModel> paginatedResult = await functionService.GetPaginatedListAsync(page, size, includeRelations, filterName, orderByKeyValueList);
            // Generate a K-V pair of all the current applied filters sent to the controller so that pagination header URLs can include them.
            List<KeyValuePair<string, string>> currrentFilters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("filterName", filterName)
            };

            paginationHelper.AddPaginationHeaderMetaDataToResponse(paginatedResult, currrentFilters, orderBy, "ListFunctions", Url, Response);

            return Ok(mapper.Map<List<Function>>(paginatedResult.Results));
        }

        [Authorize(Policy = "permission:a3s.functions.update")]
        public async override Task<IActionResult> UpdateFunctionAsync([FromRoute, Required] Guid functionId, [FromBody] FunctionSubmit functionSubmit)
        {
            if (functionId == Guid.Empty || functionSubmit.Uuid == Guid.Empty)
                return BadRequest();

            var loggedOnUser = ClaimsHelper.GetScalarClaimValue<Guid>(User, ClaimTypes.NameIdentifier, Guid.Empty);
            return Ok(await functionService.UpdateAsync(functionSubmit, loggedOnUser));
        }

        [Authorize(Policy = "permission:a3s.functions.delete")]
        public async override Task<IActionResult> DeleteFunctionAsync([FromRoute, Required] Guid functionId)
        {
            await functionService.DeleteAsync(functionId);
            return NoContent();
        }
    }
}
