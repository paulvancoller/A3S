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
using NLog;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;

namespace za.co.grindrodbank.a3s.Services
{
    public class SubRealmService : ISubRealmService
    {
        private readonly ISubRealmRepository subRealmRepository;
        private readonly IPermissionRepository permissionRepository;
        private readonly IMapper mapper;
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public SubRealmService(ISubRealmRepository subRealmRepository, IPermissionRepository permissionRepository, IMapper mapper)
        {
            this.subRealmRepository = subRealmRepository;
            this.permissionRepository = permissionRepository;
            this.mapper = mapper;
        }

        public async Task<SubRealm> CreateAsync(SubRealmSubmit subRealmSubmit, Guid createdById)
        {
            InitSharedTransaction();

            try
            {
                SubRealmModel existingSubRealm = await subRealmRepository.GetByNameAsync(subRealmSubmit.Name, false);

                if(existingSubRealm != null)
                {
                    RollbackTransaction();

                    throw new ItemNotProcessableException($"Sub-Realm with name '{subRealmSubmit.Name}' already exists.");
                }

                SubRealmModel newSubRealm = mapper.Map<SubRealmModel>(subRealmSubmit);
                newSubRealm.ChangedBy = createdById;
                await AssignPermissionsToSubRealmFromPermissionIdListAsync(newSubRealm, subRealmSubmit.PermissionIds, createdById);

                SubRealmModel createdSubRealm = await subRealmRepository.CreateAsync(newSubRealm);
                CommitTransaction();

                return mapper.Map<SubRealm>(createdSubRealm);

            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }

        private async Task AssignPermissionsToSubRealmFromPermissionIdListAsync(SubRealmModel subRealm, List<Guid> permissionIds, Guid changedById)
        {
            // The assignment of permissions to a sub-realm is declarative. Re-set the relation and re-build it from the permission list.
            subRealm.SubRealmPermissions = new List<SubRealmPermissionModel>();

            foreach(var permissionId in permissionIds)
            {
                PermissionModel existingPermission = await permissionRepository.GetByIdAsync(permissionId);

                if(existingPermission == null)
                {
                    throw new ItemNotFoundException($"Permission with ID '{permissionId}' not found when attempting to assign it to a sub-realm");
                }

                subRealm.SubRealmPermissions.Add(new SubRealmPermissionModel
                {
                    Permission = existingPermission,
                    SubRealm = subRealm,
                    ChangedBy = changedById
                });
            }
        }

        public async Task<SubRealm> GetByIdAsync(Guid subRealmId)
        {
            SubRealmModel existingSubRealm = await subRealmRepository.GetByIdAsync(subRealmId, true);

            if(existingSubRealm == null)
            {
                throw new ItemNotFoundException($"Sub-Realm with ID '{subRealmId}' not found.");
            }

            return mapper.Map<SubRealm>(existingSubRealm);
        }

        public Task<List<SubRealm>> GetListAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SubRealm> UpdateAsync(SubRealmSubmit subRealmSubmit)
        {
            throw new NotImplementedException();
        }

        public void InitSharedTransaction()
        {
            subRealmRepository.InitSharedTransaction();
            permissionRepository.InitSharedTransaction();
        }

        public void CommitTransaction()
        {
            subRealmRepository.CommitTransaction();
            subRealmRepository.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            subRealmRepository.RollbackTransaction();
            permissionRepository.RollbackTransaction();
        }
    }
}
