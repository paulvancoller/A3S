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

namespace za.co.grindrodbank.a3s.Controllers
{
    public class FunctionController : FunctionApiController
    {
        private readonly IFunctionService functionService;

        public FunctionController(IFunctionService functionService)
        {
            this.functionService = functionService;
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
        public async override Task<IActionResult> ListFunctionsAsync([FromQuery] bool permissions, [FromQuery] int page, [FromQuery, Range(1, 20)] int size, [FromQuery, StringLength(255, MinimumLength = 0)] string filterDescription, [FromQuery] List<string> orderBy)
        {
            return Ok(await functionService.GetListAsync());   
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
