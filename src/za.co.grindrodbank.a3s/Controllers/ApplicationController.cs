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
using za.co.grindrodbank.a3s.AbstractApiControllers;
using AutoMapper;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Helpers;

namespace za.co.grindrodbank.a3s.Controllers
{
    public class ApplicationController : ApplicationApiController
    {
        private readonly IApplicationService applicationService;
        private readonly IMapper mapper;

        //private readonly IUrlHelper urlHelper;
        private readonly IPaginationHelper paginationHelper;

        public ApplicationController(IApplicationService applicationService, IMapper mapper, IPaginationHelper paginationHelper)
        {
            this.applicationService = applicationService;
            this.mapper = mapper;
            this.paginationHelper = paginationHelper;
        }

        [Authorize(Policy = "permission:a3s.applications.read")]
        public async override Task<IActionResult> ListApplicationsAsync([FromQuery] int page, [FromQuery, Range(1, 20)] int size, [FromQuery, StringLength(255, MinimumLength = 0)] string filterName, [FromQuery] List<string> orderBy)
        {
            PaginatedResult paginatedResult = await applicationService.GetListAsync(page, size, filterName, orderBy);
            // The paginated result contains additional information about the current pagination state. Generate  header from this.
            //var paginationHeader = new PaginationHeaderResponse
            //{
            //    Count = paginatedResult.RowCount
            //};

            //Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationHeader) );
            paginationHelper.AddHeaderMetaData(paginatedResult, "applications", Url, Response);

            return Ok(mapper.Map<List<Application>>(paginatedResult.Results));
        }
    }
}
