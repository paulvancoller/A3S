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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using za.co.grindrodbank.a3s.AbstractApiControllers;
using za.co.grindrodbank.a3s.Services;

namespace za.co.grindrodbank.a3s.Controllers
{
    public class ClientController : ClientApiController
    {
        private readonly IClientService clientService;

        public ClientController(IClientService clientService)
        {
            this.clientService = clientService;
        }

        [Authorize(Policy = "permission:a3s.clients.read")]
        public async override Task<IActionResult> ListClientsAsync([FromQuery] int page, [FromQuery, Range(1, 20)] int size, [FromQuery, StringLength(255, MinimumLength = 0)] string filterName, [FromQuery] List<string> orderBy)
        {
            return Ok(await clientService.GetListAsync());
        }
    }
}
