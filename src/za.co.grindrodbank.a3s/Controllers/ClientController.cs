using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using za.co.grindrodbank.a3s.AbstractApiControllers;

namespace za.co.grindrodbank.a3s.Controllers
{
    public class ClientController : ClientApiController
    {
        public ClientController()
        {
        }

        public override Task<IActionResult> ListClientsAsync([FromQuery] int page, [FromQuery, Range(1, 20)] int size, [FromQuery, StringLength(255, MinimumLength = 0)] string filterName, [FromQuery] List<string> orderBy)
        {
            throw new NotImplementedException();
        }
    }
}
