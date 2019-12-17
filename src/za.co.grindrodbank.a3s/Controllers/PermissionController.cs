/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using za.co.grindrodbank.a3s.AbstractApiControllers;

namespace za.co.grindrodbank.a3s.Controllers
{
    public class PermissionController : PermissionApiController
    {
        private readonly IPermissionService permissionsService;

        public PermissionController(IPermissionService permissionsService)
        {
            this.permissionsService = permissionsService;
        }

        [Authorize(Policy = "permission:a3s.permissions.read")]
        public override async Task<IActionResult> GetPermissionAsync([FromRoute, Required] Guid permissionId)
        {
            if (permissionId == Guid.Empty)
                return BadRequest();

            var permission = await permissionsService.GetByIdAsync(permissionId);

            if(permission == null)
                return NotFound();

            return Ok(permission);
        }

        [Authorize(Policy = "permission:a3s.permissions.read")]
        public async override Task<IActionResult> ListPermissionsAsync([FromQuery] int page, [FromQuery, Range(1, 20)] int size, [FromQuery, StringLength(255, MinimumLength = 0)] string filterDescription, [FromQuery] List<string> orderBy)
        {
            return Ok(await permissionsService.GetListAsync());
        }
    }
}
