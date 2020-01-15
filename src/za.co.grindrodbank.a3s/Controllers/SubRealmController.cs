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
using Microsoft.AspNetCore.Mvc;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.AbstractApiControllers;
using za.co.grindrodbank.a3s.Helpers;
using za.co.grindrodbank.a3s.Services;

namespace za.co.grindrodbank.a3s.Controllers
{
    public class SubRealmController : SubRealmApiController
    {
        private readonly ISubRealmService subRealmService;

        public SubRealmController(ISubRealmService subRealmService)
        {
            this.subRealmService = subRealmService;
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

        public async override Task<IActionResult> ListSubRealmsAsync([FromQuery] List<string> orderBy)
        {
            return Ok(await subRealmService.GetListAsync());
        }

        public async override Task<IActionResult> UpdateSubRealmAsync([FromRoute, Required] Guid subRealmId, [FromBody] SubRealmSubmit subRealmSubmit)
        {
            return Ok(await subRealmService.UpdateAsync(subRealmId, subRealmSubmit, ClaimsHelper.GetUserId(User)));
        }
    }
}