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
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Models;
using AutoMapper;

namespace za.co.grindrodbank.a3s.Controllers
{
    public class RoleController : RoleApiController
    {
        private readonly IRoleService roleService;
        private readonly IOrderByHelper orderByHelper;
        private readonly IPaginationHelper paginationHelper;
        private readonly IMapper mapper;

        public RoleController(IRoleService roleService, IOrderByHelper orderByHelper, IPaginationHelper paginationHelper, IMapper mapper)
        {
            this.roleService = roleService;
            this.orderByHelper = orderByHelper;
            this.paginationHelper = paginationHelper;
            this.mapper = mapper;
        }

        public async override Task<IActionResult> ApproveRoleAsync([FromRoute, Required] Guid roleId)
        {
            return Ok(await roleService.ApproveRole(roleId, ClaimsHelper.GetUserId(User)));
        }

        [Authorize(Policy = "permission:a3s.roles.create")]
        public async override Task<IActionResult> CreateRoleAsync([FromBody] RoleSubmit roleSubmit)
        {
            var loggedOnUser = ClaimsHelper.GetScalarClaimValue<Guid>(User, ClaimTypes.NameIdentifier, Guid.Empty);
            return Ok(await roleService.CreateAsync(roleSubmit, loggedOnUser));
        }

        public async override Task<IActionResult> DeclineRoleAsync([FromRoute, Required] Guid roleId)
        {
            return Ok(await roleService.DeclineRole(roleId, ClaimsHelper.GetUserId(User)));
        }

        [Authorize(Policy = "permission:a3s.roles.delete")]
        public async override Task<IActionResult> DeleteRoleAsync([FromRoute, Required] Guid roleId)
        {
            return Ok(await roleService.DeleteAsync(roleId, ClaimsHelper.GetUserId(User)));
        }

        [Authorize(Policy = "permission:a3s.roles.read")]
        public async override Task<IActionResult> GetRoleAsync([FromRoute, Required] Guid roleId)
        {
            if (roleId == Guid.Empty)
                return BadRequest();

            var role = await roleService.GetByIdAsync(roleId);

            if(role == null)
                return NotFound();

            return Ok(role);
        }

        public async override Task<IActionResult> GetRoleTransientsAsync([FromRoute, Required] Guid roleId)
        {
            return Ok(await roleService.GetLatestRoleTransientsAsync(roleId));
        }

        [Authorize(Policy = "permission:a3s.roles.read")]
        public async override Task<IActionResult> ListRolesAsync([FromQuery]bool includeRelations, [FromQuery]int page, [FromQuery][Range(1, 20)]int size, [FromQuery][StringLength(255, MinimumLength = 0)]string filterName, [FromQuery]string orderBy)
        {
            List<KeyValuePair<string, string>> orderByKeyValueList = orderByHelper.ConvertCommaSeparateOrderByStringToKeyValuePairList(orderBy);
            // Validate only correct order by components were supplied.
            orderByHelper.ValidateOrderByListOnlyContainsCertainElements(orderByKeyValueList, new List<string> { "name" });
            PaginatedResult<RoleModel> paginatedResult = await roleService.GetPaginatedListAsync(page, size, includeRelations, filterName, orderByKeyValueList);
            // Generate a K-V pair of all the current applied filters sent to the controller so that pagination header URLs can include them.
            List<KeyValuePair<string, string>> currrentFilters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("includeRelations", includeRelations ? "true" : "false"),
                new KeyValuePair<string, string>("filterName", filterName)
            };

            paginationHelper.AddPaginationHeaderMetaDataToResponse(paginatedResult, currrentFilters, orderBy, "ListRoles", Url, Response);

            return Ok(mapper.Map<List<Role>>(paginatedResult.Results));
        }

        [Authorize(Policy = "permission:a3s.roles.update")]
        public async override Task<IActionResult> UpdateRoleAsync([FromRoute, Required] Guid roleId, [FromBody] RoleSubmit roleSubmit)
        {
            if (roleId == Guid.Empty || roleSubmit.Uuid == Guid.Empty)
                return BadRequest();

            return Ok(await roleService.UpdateAsync(roleSubmit, roleId, ClaimsHelper.GetUserId(User)));
        }
    }
}
