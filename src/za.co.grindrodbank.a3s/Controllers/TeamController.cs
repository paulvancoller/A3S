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
    public class TeamController : TeamApiController
    {
        private readonly ITeamService teamService;
        private readonly IOrderByHelper orderByHelper;
        private readonly IPaginationHelper paginationHelper;
        private readonly IMapper mapper;

        public TeamController(ITeamService teamService, IOrderByHelper orderByHelper, IPaginationHelper paginationHelper, IMapper mapper)
        {
            this.teamService = teamService;
            this.orderByHelper = orderByHelper;
            this.paginationHelper = paginationHelper;
            this.mapper = mapper;
        }

        [Authorize(Policy = "permission:a3s.teams.create")]
        public async override Task<IActionResult> CreateTeamAsync([FromBody] TeamSubmit teamSubmit)
        {
            var loggedOnUser = ClaimsHelper.GetScalarClaimValue<Guid>(User, ClaimTypes.NameIdentifier, Guid.Empty);
            return Ok(await teamService.CreateAsync(teamSubmit, loggedOnUser));
        }

        [Authorize(Policy = "permission:a3s.teams.read")]
        public async override Task<IActionResult> GetTeamAsync([FromRoute, Required] Guid teamId)
        {
            if (teamId == Guid.Empty)
                return BadRequest();

            var team = await teamService.GetByIdAsync(teamId, true);

            if(team == null)
                return NotFound();

            return Ok(team);
        }

        [Authorize(Policy = "permission:a3s.teams.read")]
        public async override Task<IActionResult> ListTeamsAsync([FromQuery]bool includeRelations, [FromQuery]int page, [FromQuery][Range(1, 20)]int size, [FromQuery][StringLength(255, MinimumLength = 0)]string filterName, [FromQuery]string orderBy)
        {
            List<KeyValuePair<string, string>> orderByKeyValueList = orderByHelper.ConvertCommaSeparateOrderByStringToKeyValuePairList(orderBy);
            // Validate only correct order by components were supplied.
            orderByHelper.ValidateOrderByListOnlyContainsCertainElements(orderByKeyValueList, new List<string> { "name" });

            PaginatedResult<TeamModel> paginatedResult = ClaimsHelper.GetDataPolicies(User).Contains("a3s.viewYourTeamsOnly")
                ? await teamService.GetPaginatedListForMemberUserAsync(ClaimsHelper.GetUserId(User), page, size, includeRelations, filterName, orderByKeyValueList)
                : await teamService.GetPaginatedListAsync(page, size, includeRelations, filterName, orderByKeyValueList);

            // Generate a K-V pair of all the current applied filters sent to the controller so that pagination header URLs can include them.
            List <KeyValuePair<string, string>> currrentFilters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("includeRelations", includeRelations ? "true" : "false"),
                new KeyValuePair<string, string>("filterName", filterName)
            };

            paginationHelper.AddPaginationHeaderMetaDataToResponse(paginatedResult, currrentFilters, orderBy, "ListTeams", Url, Response);

            return Ok(mapper.Map<List<Team>>(paginatedResult.Results));
        }

        [Authorize(Policy = "permission:a3s.teams.update")]
        public async override Task<IActionResult> UpdateTeamAsync([FromRoute, Required] Guid teamId, [FromBody] TeamSubmit teamSubmit)
        {
            if (teamId == Guid.Empty || teamSubmit.Uuid == Guid.Empty)
                return BadRequest();

            var loggedOnUser = ClaimsHelper.GetScalarClaimValue<Guid>(User, ClaimTypes.NameIdentifier, Guid.Empty);
            return Ok(await teamService.UpdateAsync(teamSubmit, loggedOnUser));
        }
    }
}
