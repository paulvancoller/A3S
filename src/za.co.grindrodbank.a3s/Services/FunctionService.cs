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
        private readonly IMapper mapper;

        public FunctionService(IFunctionRepository functionRepository, IPermissionRepository permissionRepository, IApplicationRepository applicationRepository, IMapper mapper)
        {
            this.functionRepository = functionRepository;
            this.applicationRepository = applicationRepository;
            this.permissionRepository = permissionRepository;
            this.mapper = mapper;
        }

        public async Task<Function> CreateAsync(FunctionSubmit functionSubmit, Guid createdByGuid)
        {
            // Start transactions to allow complete rollback in case of an error
            BeginAllTransactions();

            try
            {
                FunctionModel existingFunction = await functionRepository.GetByNameAsync(functionSubmit.Name);
                if (existingFunction != null)
                    throw new ItemNotProcessableException($"Function with Name '{functionSubmit.Name}' already exist.");

                var function = new FunctionModel();

                function.Name = functionSubmit.Name;
                function.Description = functionSubmit.Description;
                function.FunctionPermissions = new List<FunctionPermissionModel>();
                function.ChangedBy = createdByGuid;

                await CheckForApplicationAndAssignToFunctionIfExists(function, functionSubmit);
                await CheckThatPermissionsExistAndAssignToFunction(function, functionSubmit);

                // All successful
                CommitAllTransactions();

                return mapper.Map<Function>(await functionRepository.CreateAsync(function));
            }
            catch
            {
                RollbackAllTransactions();
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
            BeginAllTransactions();

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
                CommitAllTransactions();

                return mapper.Map<Function>(await functionRepository.UpdateAsync(function));
            }
            catch
            {
                RollbackAllTransactions();
                throw;
            }
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

            if (application == null)
            {
                throw new ItemNotFoundException($"Application with UUID: '{functionSubmit.ApplicationId}' not found. Cannot create function '{functionSubmit.Name}' with this application.");
            }

            function.Application = application;
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

                    function.FunctionPermissions.Add(new FunctionPermissionModel
                    {
                        Function = function,
                        Permission = permission
                    });
                }
            }
        }

        private void BeginAllTransactions()
        {
            permissionRepository.InitSharedTransaction();
            applicationRepository.InitSharedTransaction();
            functionRepository.InitSharedTransaction();
        }

        private void CommitAllTransactions()
        {
            permissionRepository.CommitTransaction();
            applicationRepository.CommitTransaction();
            functionRepository.CommitTransaction();
        }

        private void RollbackAllTransactions()
        {
            permissionRepository.RollbackTransaction();
            applicationRepository.RollbackTransaction();
            functionRepository.RollbackTransaction();
        }
    }
}
