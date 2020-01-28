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
using IdentityServer4.EntityFramework.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.AbstractApiControllers;
using za.co.grindrodbank.a3s.Helpers;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;

namespace za.co.grindrodbank.a3s.Controllers
{
    public class ClientController : ClientApiController
    {
        private readonly IClientService clientService;
        private readonly IOrderByHelper orderByHelper;
        private readonly IPaginationHelper paginationHelper;
        private readonly IMapper mapper;

        public ClientController(IClientService clientService, IOrderByHelper orderByHelper, IPaginationHelper paginationHelper, IMapper mapper)
        {
            this.clientService = clientService;
            this.orderByHelper = orderByHelper;
            this.paginationHelper = paginationHelper;
            this.mapper = mapper;
        }

        [Authorize(Policy = "permission:a3s.clients.read")]
        public async override Task<IActionResult> GetClientAsync([FromRoute, Required] string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return BadRequest();

            var client = await clientService.GetByClientIdAsync(clientId);

            if (client == null)
                return NotFound();

            return Ok(client);
        }

        [Authorize(Policy = "permission:a3s.clients.read")]
        public async override Task<IActionResult> ListClientsAsync([FromQuery]int page, [FromQuery][Range(1, 20)]int size, [FromQuery][StringLength(255, MinimumLength = 0)]string filterName, [FromQuery][StringLength(255, MinimumLength = 0)]string filterClientId, [FromQuery]string orderBy)
        {
            List<KeyValuePair<string, string>> orderByKeyValueList = orderByHelper.ConvertCommaSeparateOrderByStringToKeyValuePairList(orderBy);
            // Validate only correct order by components were supplied.
            orderByHelper.ValidateOrderByListOnlyContainsCertainElements(orderByKeyValueList, new List<string> { "name", "clientId" });

            PaginatedResult<Client> paginatedResult = await clientService.GetPaginatedListAsync(page, size, filterName, filterClientId, orderByKeyValueList);

            // Generate a K-V pair of all the current applied filters sent to the controller so that pagination header URLs can include them.
            List<KeyValuePair<string, string>> currrentFilters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("filterName", filterName),
                new KeyValuePair<string, string>("filterClientId", filterClientId)
            };

            paginationHelper.AddPaginationHeaderMetaDataToResponse(paginatedResult, currrentFilters, orderBy, "ListClients", Url, Response);

            return Ok(mapper.Map<List<Oauth2Client>>(paginatedResult.Results));
        }
    }
}
