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
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.AbstractApiControllers;
using za.co.grindrodbank.a3s.Helpers;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;

namespace za.co.grindrodbank.a3s.Controllers
{
    public class SubRealmController : SubRealmApiController
    {
        private readonly ISubRealmService subRealmService;
        private readonly IOrderByHelper orderByHelper;
        private readonly IPaginationHelper paginationHelper;
        private readonly IMapper mapper;

        public SubRealmController(ISubRealmService subRealmService, IOrderByHelper orderByHelper, IPaginationHelper paginationHelper, IMapper mapper)
        {
            this.subRealmService = subRealmService;
            this.orderByHelper = orderByHelper;
            this.paginationHelper = paginationHelper;
            this.mapper = mapper;
        }

        public async override Task<IActionResult> CreateSubRealmAsync([FromBody] SubRealmSubmit subRealmSubmit)
        {
            return Ok(await subRealmService.CreateAsync(subRealmSubmit, ClaimsHelper.GetUserId(User)));
        }

        public async override Task<IActionResult> DeleteSubRealmAsync([FromRoute, Required] Guid subRealmId)
        {
            await subRealmService.DeleteAsync(subRealmId);
            return NoContent();
        }

        public async override Task<IActionResult> GetSubRealmAsync([FromRoute, Required] Guid subRealmId)
        {
            return Ok(await subRealmService.GetByIdAsync(subRealmId));
        }

        public async override Task<IActionResult> ListSubRealmsAsync([FromQuery]int page, [FromQuery][Range(1, 20)]int size, [FromQuery]bool includeRelations, [FromQuery][StringLength(255, MinimumLength = 0)]string filterName, [FromQuery]string orderBy)
        {
            List<KeyValuePair<string, string>> orderByKeyValueList = orderByHelper.ConvertCommaSeparateOrderByStringToKeyValuePairList(orderBy);
            // Validate only correct order by components were supplied.
            orderByHelper.ValidateOrderByListOnlyContainsCertainElements(orderByKeyValueList, new List<string> { "name" });
            PaginatedResult<SubRealmModel> paginatedResult = await subRealmService.GetPaginatedListAsync(page, size, includeRelations, filterName, orderByKeyValueList);
            // Generate a K-V pair of all the current applied filters sent to the controller so that pagination header URLs can include them.
            List<KeyValuePair<string, string>> currrentFilters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("filterName", filterName)
            };

            paginationHelper.AddPaginationHeaderMetaDataToResponse(paginatedResult, currrentFilters, orderBy, "ListSubRealms", Url, Response);

            return Ok(mapper.Map<List<SubRealm>>(paginatedResult.Results));
        }

        public async override Task<IActionResult> UpdateSubRealmAsync([FromRoute, Required] Guid subRealmId, [FromBody] SubRealmSubmit subRealmSubmit)
        {
            return Ok(await subRealmService.UpdateAsync(subRealmId, subRealmSubmit, ClaimsHelper.GetUserId(User)));
        }
    }
}