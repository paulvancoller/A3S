/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using za.co.grindrodbank.a3s.AbstractApiControllers;

namespace za.co.grindrodbank.a3s.Controllers
{
    public class ApplicationController : ApplicationApiController
    {
        private readonly IApplicationService applicationService;

        public ApplicationController(IApplicationService applicationService)
        {
            this.applicationService = applicationService;
        }

        [Authorize(Policy = "permission:a3s.applications.read")]
        public async override Task<IActionResult> ListApplicationsAsync([FromQuery] bool permissions, [FromQuery] int page, [FromQuery, Range(1, 20)] int size, [FromQuery, StringLength(255, MinimumLength = 0)] string filterDescription, [FromQuery] List<string> orderBy)
        {
            return Ok(await applicationService.GetListAsync());
        }
    }
}
