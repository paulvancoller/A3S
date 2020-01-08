/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Repositories;
using AutoMapper;
using za.co.grindrodbank.a3s.A3SApiResources;

namespace za.co.grindrodbank.a3s.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository permissionRepository;
        private readonly IMapper mapper;

        public PermissionService(IPermissionRepository permissionRepository, IMapper mapper)
        {
            this.permissionRepository = permissionRepository;
            this.mapper = mapper;
        }

        public async Task<Permission> GetByIdAsync(Guid permissionId)
        {
            return mapper.Map<Permission>(await permissionRepository.GetByIdAsync(permissionId));
        }

        public async Task<List<Permission>> GetListAsync()
        {
            return mapper.Map<List<Permission>>(await permissionRepository.GetListAsync());
        }
    }
}