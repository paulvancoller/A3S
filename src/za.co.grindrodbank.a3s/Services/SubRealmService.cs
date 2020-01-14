/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Linq;
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
                // Set a new relations list.
                newSubRealm.SubRealmPermissions = new List<SubRealmPermissionModel>();
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
            // We want to track which permissions were added so that their changedById can be updated, but leave the permissions that already exist un-touched.
            List <SubRealmPermissionModel> newSubRealmPermissionsState = new List<SubRealmPermissionModel>();

            foreach(var permissionId in permissionIds)
            {
                // Search existing sub-realm permissions state for the permission.
                var existingSubRealmPermission = subRealm.SubRealmPermissions.Where(SubRealmPermissionModel => SubRealmPermissionModel.PermissionId == permissionId).FirstOrDefault();

                if(existingSubRealmPermission != null)
                {
                    newSubRealmPermissionsState.Add(existingSubRealmPermission);
                    continue;
                }

                // If the permissions is new, attempt to add it, but perform some checks first.
                PermissionModel existingPermission = await permissionRepository.GetByIdAsync(permissionId);

                if(existingPermission == null)
                {
                    throw new ItemNotFoundException($"Permission with ID '{permissionId}' not found when attempting to assign it to a sub-realm");
                }

                newSubRealmPermissionsState.Add(new SubRealmPermissionModel
                {
                    Permission = existingPermission,
                    SubRealm = subRealm,
                    ChangedBy = changedById
                });

                // If a permission was added, it indicates that parent sub-realm was changed.
                subRealm.ChangedBy = changedById;
            }

            subRealm.SubRealmPermissions = newSubRealmPermissionsState;
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

        public async Task<SubRealm> UpdateAsync(Guid subRealmId, SubRealmSubmit subRealmSubmit, Guid updatedBy)
        {
            InitSharedTransaction();

            try
            {
                SubRealmModel existingSubRealm = await subRealmRepository.GetByIdAsync(subRealmId, true);

                if (existingSubRealm != null)
                {
                    RollbackTransaction();

                    throw new ItemNotFoundException($"Sub-Realm with ID '{subRealmId}' does not exist.");
                }

                // Map any potential updates from the submit model onto the existing sub-realms model.
                if(subRealmSubmit.Name != existingSubRealm.Name)
                {
                    existingSubRealm.Name = subRealmSubmit.Name;
                    existingSubRealm.ChangedBy = updatedBy;
                }

                if(subRealmSubmit.Description != existingSubRealm.Description)
                {
                    existingSubRealm.Description = subRealmSubmit.Description;
                    existingSubRealm.ChangedBy = updatedBy;
                }

                await AssignPermissionsToSubRealmFromPermissionIdListAsync(existingSubRealm, subRealmSubmit.PermissionIds, updatedBy);

                existingSubRealm = await subRealmRepository.UpdateAsync(existingSubRealm);
                CommitTransaction();

                return mapper.Map<SubRealm>(existingSubRealm);
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
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

        public Task DeleteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
