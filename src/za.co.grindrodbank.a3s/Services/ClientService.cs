/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Repositories;

namespace za.co.grindrodbank.a3s.Services
{
    public class ClientService : IClientService
    {
        private readonly IIdentityClientRepository identityClientRepository;
        private readonly IMapper mapper;

        public ClientService(IIdentityClientRepository identityClientRepository, IMapper mapper)
        {
            this.identityClientRepository = identityClientRepository;
            this.mapper = mapper;
        }

        public async Task<List<Oauth2Client>> GetListAsync()
        {
            return mapper.Map<List<Oauth2Client>>(await identityClientRepository.GetListAsync());
        }
    }
}
