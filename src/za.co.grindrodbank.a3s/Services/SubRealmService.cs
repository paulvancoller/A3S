/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IApplicationDataPolicyRepository applicationDataPolicyRepository;
        private readonly IMapper mapper;

        public SubRealmService(ISubRealmRepository subRealmRepository, IPermissionRepository permissionRepository, IApplicationDataPolicyRepository applicationDataPolicyRepository, IMapper mapper)
        {
            this.subRealmRepository = subRealmRepository;
            this.permissionRepository = permissionRepository;
            this.applicationDataPolicyRepository = applicationDataPolicyRepository;
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
                    throw new ItemNotProcessableException($"Sub-Realm with name '{subRealmSubmit.Name}' already exists.");
                }

                SubRealmModel newSubRealm = mapper.Map<SubRealmModel>(subRealmSubmit);
                newSubRealm.ChangedBy = createdById;
                // Set a new relations list.
                newSubRealm.SubRealmPermissions = new List<SubRealmPermissionModel>();
                newSubRealm.SubRealmApplicationDataPolicies = new List<SubRealmApplicationDataPolicyModel>();
                await AssignPermissionsToSubRealmFromPermissionIdListAsync(newSubRealm, subRealmSubmit.PermissionIds, createdById);
                await AssignApplicationDataPoliciesToSubRealmFromApplicationDataPolicyIdListAsync(newSubRealm, subRealmSubmit.ApplicationDataPolicyIds, createdById);

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
                var existingSubRealmPermission = subRealm.SubRealmPermissions.FirstOrDefault(SubRealmPermissionModel => SubRealmPermissionModel.PermissionId == permissionId);

                if(existingSubRealmPermission != null)
                {
                    newSubRealmPermissionsState.Add(existingSubRealmPermission);
                    continue;
                }

                // If the permissions is new, attempt to add it, but perform some checks first.
                PermissionModel existingPermission = await permissionRepository.GetByIdAsync(permissionId);

                if(existingPermission == null)
                {
                    throw new ItemNotFoundException($"Permission with ID '{permissionId}' not found when attempting to assign it to a sub-realm.");
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

        private async Task AssignApplicationDataPoliciesToSubRealmFromApplicationDataPolicyIdListAsync(SubRealmModel subRealm, List<Guid> applicationDataPolicyIds, Guid changedById)
        {
            // We want to track which permissions were added so that their changedById can be updated, but leave the permissions that already exist un-touched.
            List<SubRealmApplicationDataPolicyModel> newApplicationDataPolicyState = new List<SubRealmApplicationDataPolicyModel>();

            foreach (var applicationDataPolicyId in applicationDataPolicyIds)
            {
                // Search existing sub-realm permissions state for the permission.
                var existingSubRealmApplicationDataPolicy = subRealm.SubRealmApplicationDataPolicies.FirstOrDefault(SubRealmApplicationDataPolicyModel => SubRealmApplicationDataPolicyModel.ApplicationDataPolicyId == applicationDataPolicyId);

                if (existingSubRealmApplicationDataPolicy != null)
                {
                    newApplicationDataPolicyState.Add(existingSubRealmApplicationDataPolicy);
                    continue;
                }

                // If the application data policy is new, attempt to add it, but perform some checks first.
                ApplicationDataPolicyModel existingApplicationDataPolicy = await applicationDataPolicyRepository.GetByIdAsync(applicationDataPolicyId);

                if (existingApplicationDataPolicy == null)
                {
                    throw new ItemNotFoundException($"Application Data Policy with ID '{applicationDataPolicyId}' not found when attempting to assign it to a sub-realm.");
                }

                newApplicationDataPolicyState.Add(new SubRealmApplicationDataPolicyModel
                {
                    ApplicationDataPolicy = existingApplicationDataPolicy,
                    SubRealm = subRealm,
                    ChangedBy = changedById
                });

                // If a permission was added, it indicates that parent sub-realm was changed.
                subRealm.ChangedBy = changedById;
            }

            subRealm.SubRealmApplicationDataPolicies = newApplicationDataPolicyState;
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

        public async Task<List<SubRealm>> GetListAsync()
        {
            return mapper.Map<List<SubRealm>>(await subRealmRepository.GetListAsync(true));
        }

        public async Task<SubRealm> UpdateAsync(Guid subRealmId, SubRealmSubmit subRealmSubmit, Guid updatedBy)
        {
            InitSharedTransaction();

            try
            {
                SubRealmModel existingSubRealm = await subRealmRepository.GetByIdAsync(subRealmId, true);

                if (existingSubRealm == null)
                {
                    throw new ItemNotFoundException($"Sub-Realm with ID '{subRealmId}' does not exist.");
                }

                // Sub-Realm names must be unique. If the sub-realm name has changed, then check that another sub-realm does have the same name.
                if(existingSubRealm.Name != subRealmSubmit.Name)
                {
                    SubRealmModel existingNamedSubRealm = await subRealmRepository.GetByNameAsync(subRealmSubmit.Name, false);

                    if (existingNamedSubRealm != null)
                    {
                        throw new ItemNotProcessableException($"Cannot update sub-Realm with name '{subRealmSubmit.Name}', as this name is already used by another sub-realm.");
                    }
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
                await AssignApplicationDataPoliciesToSubRealmFromApplicationDataPolicyIdListAsync(existingSubRealm, subRealmSubmit.ApplicationDataPolicyIds, updatedBy);

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
            applicationDataPolicyRepository.InitSharedTransaction();
        }

        public void CommitTransaction()
        {
            subRealmRepository.CommitTransaction();
            subRealmRepository.CommitTransaction();
            applicationDataPolicyRepository.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            subRealmRepository.RollbackTransaction();
            permissionRepository.RollbackTransaction();
            applicationDataPolicyRepository.RollbackTransaction();
        }

        public async Task DeleteAsync(Guid subRealmId)
        {
            // Deleting a sub-realm may need to be refined a little. A sub-realm has many profiles, which in turn, could have many functions, roles
            // and teams assgined to it. There may need to be logic to remove all the entities that are possible related to the sub-realm too.
            // For now, we are just going to delete the main sub-realm entity, and allow any immediate cascades to occur.
            var subRealmToDelete = await subRealmRepository.GetByIdAsync(subRealmId, true);

            if(subRealmToDelete == null)
            {
                throw new ItemNotFoundException($"Sub-realm with ID '{subRealmId}' does not exist.");
            }

            await subRealmRepository.DeleteAsync(subRealmToDelete);
        }

        public async Task<PaginatedResult<SubRealmModel>> GetPaginatedListAsync(int page, int pageSize, bool includeRelations, string filterName, List<KeyValuePair<string, string>> orderBy)
        {
            return await subRealmRepository.GetPaginatedListAsync(page, pageSize, includeRelations, filterName, orderBy);
        }
    }
}
