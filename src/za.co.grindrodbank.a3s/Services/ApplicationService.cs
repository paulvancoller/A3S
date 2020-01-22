/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Repositories;
using AutoMapper;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationRepository applicationRepository;
        private readonly IMapper mapper;

        public ApplicationService(IApplicationRepository applicationRepository, IMapper mapper)
        {
            this.applicationRepository = applicationRepository;
            this.mapper = mapper;
        }

        public async Task<Application> GetByIdAsync(Guid applicationId)
        {
            return mapper.Map<Application>(await applicationRepository.GetByIdAsync(applicationId));
        }

        public async Task<List<Application>> GetListAsync()
        {
            return mapper.Map<List<Application>>(await applicationRepository.GetListAsync());
        }

        public async Task<PaginatedResult> GetListAsync(int page, int pageSize, string filterName, List<string> orderBy)
        {
            return await applicationRepository.GetPaginatedListAsync(page, pageSize, filterName, orderBy);
        }
    }
}
