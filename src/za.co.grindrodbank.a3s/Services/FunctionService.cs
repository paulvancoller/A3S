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
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using AutoMapper;
using NLog;
using za.co.grindrodbank.a3s.A3SApiResources;

namespace za.co.grindrodbank.a3s.Services
{
    public class FunctionService : IFunctionService
    {
        private readonly IFunctionRepository functionRepository;
        private readonly IPermissionRepository permissionRepository;
        private readonly IApplicationRepository applicationRepository;
        private readonly ISubRealmRepository subRealmRepository;
        private readonly IMapper mapper;

        public FunctionService(IFunctionRepository functionRepository, IPermissionRepository permissionRepository, IApplicationRepository applicationRepository, ISubRealmRepository subRealmRepository, IMapper mapper)
        {
            this.functionRepository = functionRepository;
            this.applicationRepository = applicationRepository;
            this.permissionRepository = permissionRepository;
            this.subRealmRepository = subRealmRepository;
            this.mapper = mapper;
        }

        public async Task<Function> CreateAsync(FunctionSubmit functionSubmit, Guid createdByGuid)
        {
            // Start transactions to allow complete rollback in case of an error
            InitSharedTransaction();

            try
            {
                FunctionModel existingFunction = await functionRepository.GetByNameAsync(functionSubmit.Name);
                if (existingFunction != null)
                    throw new ItemNotProcessableException($"Function with Name '{functionSubmit.Name}' already exist.");

                var function = new FunctionModel
                {
                    Name = functionSubmit.Name,
                    Description = functionSubmit.Description,
                    FunctionPermissions = new List<FunctionPermissionModel>(),
                    ChangedBy = createdByGuid
                };

                await CheckForApplicationAndAssignToFunctionIfExists(function, functionSubmit);
                await CheckForSubRealmAndAssignToFunctionIfExists(function, functionSubmit);
                await CheckThatPermissionsExistAndAssignToFunction(function, functionSubmit);

                // All successful
                CommitTransaction();

                return mapper.Map<Function>(await functionRepository.CreateAsync(function));
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }

        public async Task<Function> GetByIdAsync(Guid functionId)
        {
            return mapper.Map<Function>(await functionRepository.GetByIdAsync(functionId));
        }

        public async Task<List<Function>> GetListAsync()
        {
            return mapper.Map<List<Function>>(await functionRepository.GetListAsync());
        }

        public async Task<Function> UpdateAsync(FunctionSubmit functionSubmit, Guid updatedByGuid)
        {
            // Start transactions to allow complete rollback in case of an error
            InitSharedTransaction();

            try
            {
                var function = await functionRepository.GetByIdAsync(functionSubmit.Uuid);

                if (function == null)
                    throw new ItemNotFoundException($"Function {functionSubmit.Uuid} not found!");

                if (function.Name != functionSubmit.Name)
                {
                    // Confirm the new name is available
                    var checkExistingNameModel = await functionRepository.GetByNameAsync(functionSubmit.Name);
                    if (checkExistingNameModel != null)
                        throw new ItemNotProcessableException($"Function with name '{functionSubmit.Name}' already exists.");
                }

                function.Name = functionSubmit.Name;
                function.Description = functionSubmit.Description;
                function.FunctionPermissions = new List<FunctionPermissionModel>();
                function.ChangedBy = updatedByGuid;

                await CheckForApplicationAndAssignToFunctionIfExists(function, functionSubmit);
                await CheckThatPermissionsExistAndAssignToFunction(function, functionSubmit);

                // All successful
                CommitTransaction();

                return mapper.Map<Function>(await functionRepository.UpdateAsync(function));
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }

        private async Task CheckForSubRealmAndAssignToFunctionIfExists(FunctionModel function, FunctionSubmit functionSubmit)
        {
            // Recall that submit models with empty GUIDs will not be null but rather Guid.Empty.
            if (functionSubmit.SubRealmId == null || functionSubmit.SubRealmId == Guid.Empty)
            {
                return;
            }

            var existingSubRealm = await subRealmRepository.GetByIdAsync(functionSubmit.SubRealmId, false);
            function.SubRealm = existingSubRealm ?? throw new ItemNotFoundException($"Sub-realm with ID '{functionSubmit.SubRealmId}' does not exist.");
        }

        public async Task DeleteAsync(Guid functionId)
        {
            var function = await functionRepository.GetByIdAsync(functionId);

            if (function == null)
            {
                throw new ItemNotFoundException($"Function with UUID '{functionId}' not found.");
            }

            await functionRepository.DeleteAsync(function);
        }

        private async Task CheckForApplicationAndAssignToFunctionIfExists(FunctionModel function, FunctionSubmit functionSubmit)
        {
            var application = await applicationRepository.GetByIdAsync(functionSubmit.ApplicationId);
            function.Application = application ?? throw new ItemNotFoundException($"Application with UUID: '{functionSubmit.ApplicationId}' not found. Cannot create function '{functionSubmit.Name}' with this application.");
        }

        private async Task CheckThatPermissionsExistAndAssignToFunction(FunctionModel function, FunctionSubmit functionSubmit)
        {
            if (functionSubmit.Permissions != null && functionSubmit.Permissions.Count > 0)
            {
                foreach (var permissionId in functionSubmit.Permissions)
                {
                    var permission = await permissionRepository.GetByIdWithApplicationAsync(permissionId);

                    if (permission == null)
                    {
                        throw new ItemNotFoundException($"Permission with UUID: '{permissionId}' not found. Not adding it to function '{functionSubmit.Name}'.");
                    }

                    // NB!! Must check that the permission actually attached to an application function where the application is the same as the Funciton
                    // application. Functions cannot be created from permissions across applications.
                    if (permission.ApplicationFunctionPermissions.First().ApplicationFunction.Application.Id != functionSubmit.ApplicationId)
                    {
                        throw new ItemNotProcessableException($"Permission with UUID: '{permissionId}' does not belong to application with ID: {functionSubmit.ApplicationId}. Not adding it to function '{functionSubmit.Name}'.");
                    }

                    // If there is a Sub-Realm associated with function, we must ensure that the permission is associated with the same sub realm.
                    if (function.SubRealm != null)
                    {
                        var subRealmPermission = permission.SubRealmPermissions.Where(psrp => psrp.SubRealm.Id == function.SubRealm.Id).FirstOrDefault();

                        if (subRealmPermission == null)
                        {
                            throw new ItemNotProcessableException($"Attempting to add a permission with ID '{permission.Id}' to a function within the '{function.SubRealm.Name}' sub-realm but the permission does not exist within that sub-realm.");
                        }
                    }

                    function.FunctionPermissions.Add(new FunctionPermissionModel
                    {
                        Function = function,
                        Permission = permission
                    });
                }
            }
        }

        public async Task<PaginatedResult<FunctionModel>> GetPaginatedListAsync(int page, int pageSize, bool includeRelations, string filterName, List<KeyValuePair<string, string>> orderBy)
        {
            return await functionRepository.GetPaginatedListAsync(page, pageSize, includeRelations, filterName, orderBy);
        }

        public void InitSharedTransaction()
        {
            permissionRepository.InitSharedTransaction();
            applicationRepository.InitSharedTransaction();
            functionRepository.InitSharedTransaction();
            subRealmRepository.InitSharedTransaction();
        }

        public void CommitTransaction()
        {
            permissionRepository.CommitTransaction();
            applicationRepository.CommitTransaction();
            functionRepository.CommitTransaction();
            subRealmRepository.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            permissionRepository.RollbackTransaction();
            applicationRepository.RollbackTransaction();
            functionRepository.RollbackTransaction();
            subRealmRepository.RollbackTransaction();
        }
    }
}
