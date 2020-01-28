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
    public class ApplicationFunctionService : IApplicationFunctionService
    {
        private readonly IApplicationFunctionRepository applicationFunctionRepository;
        private readonly IMapper mapper;

        public ApplicationFunctionService(IApplicationFunctionRepository applicationFunctionRepository, IMapper mapper)
        {
            this.applicationFunctionRepository = applicationFunctionRepository;
            this.mapper = mapper;
        }

        public async Task<ApplicationFunction> GetByIdAsync(Guid applicationFunctionId)
        {
            return mapper.Map<ApplicationFunction>(await applicationFunctionRepository.GetByIdAsync(applicationFunctionId));
        }

        public async Task<List<ApplicationFunction>> GetListAsync()
        {
            return mapper.Map<List<ApplicationFunction>>(await applicationFunctionRepository.GetListAsync());
        }

        public async Task<PaginatedResult<ApplicationFunctionModel>> GetPaginatedListAsync(int page, int pageSize, bool includeRelations, string filterName, List<KeyValuePair<string, string>> orderBy)
        {
            return await applicationFunctionRepository.GetPaginatedListAsync(page, pageSize, includeRelations, filterName, orderBy);
        }
    }
}
