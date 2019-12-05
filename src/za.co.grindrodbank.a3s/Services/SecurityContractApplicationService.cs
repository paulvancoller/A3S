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
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using IdentityServer4.EntityFramework.Entities;
using NLog;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Exceptions;

namespace za.co.grindrodbank.a3s.Services
{
    public class SecurityContractApplicationService : ISecurityContractApplicationService
    {
        private readonly IApplicationRepository applicationRepository;
        private readonly IIdentityApiResourceRepository identityApiResourceRespository;
        private readonly IPermissionRepository permissionRepository;
        private readonly IApplicationFunctionRepository applicationFunctionRepository;
        private readonly IApplicationDataPolicyRepository applicationDataPolicyRepository;
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public SecurityContractApplicationService(IApplicationRepository applicationRepository, IIdentityApiResourceRepository identityApiResourceRespository, IPermissionRepository permissionRepository, IApplicationFunctionRepository applicationFunctionRepository, IApplicationDataPolicyRepository applicationDataPolicyRepository)
        {
            this.applicationRepository = applicationRepository;
            this.identityApiResourceRespository = identityApiResourceRespository;
            this.permissionRepository = permissionRepository;
            this.applicationFunctionRepository = applicationFunctionRepository;
            this.applicationDataPolicyRepository = applicationDataPolicyRepository;
        }

        public async Task<ApplicationModel> ApplyResourceServerDefinitionAsync(SecurityContractApplication applicationSecurityContractDefinition, Guid updatedById, bool dryRun, SecurityContractDryRunResult securityContractDryRunResult)
        {
            logger.Debug($"[applications.fullname: '{applicationSecurityContractDefinition.Fullname}']: Applying application security contract definition for application: '{applicationSecurityContractDefinition.Fullname}'");
            // Attempt to load any existing application by name, as the name is essentially the API primary key.
            var application = await applicationRepository.GetByNameAsync(applicationSecurityContractDefinition.Fullname);

            if (application == null)
            {
                logger.Debug($"[applications.fullname: '{applicationSecurityContractDefinition.Fullname}']: Application '{applicationSecurityContractDefinition.Fullname}' not found in database. Creating new application.");
                return await CreateNewResourceServer(applicationSecurityContractDefinition, updatedById, dryRun, securityContractDryRunResult);
            }

            logger.Debug($"[applications.fullname: '{applicationSecurityContractDefinition.Fullname}']: Application '{applicationSecurityContractDefinition.Fullname}' already exists. Updating it.");
            return await UpdateExistingResourceServer(application, applicationSecurityContractDefinition, updatedById, dryRun, securityContractDryRunResult);
        }

        private async Task<ApplicationModel> CreateNewResourceServer(SecurityContractApplication applicationSecurityContractDefinition, Guid updatedByGuid, bool dryRun, SecurityContractDryRunResult securityContractDryRunResult)
        {
            // Note: Always added 'permission' as the user claims that need to be mapped into access tokens for this API Resource.
            ApiResource identityServerApiResource = await identityApiResourceRespository.GetByNameAsync(applicationSecurityContractDefinition.Fullname);

            if(identityServerApiResource == null)
            {
                await identityApiResourceRespository.CreateAsync(applicationSecurityContractDefinition.Fullname, new[] { "permission" });
            }
            else
            {
                logger.Debug($"[applications.fullname: '{applicationSecurityContractDefinition.Fullname}']: The API Resource with name '{applicationSecurityContractDefinition.Fullname}' already exists on the Identity Server. Not creating a new one!");
            }

            // Create the A3S representation of the resource.
            ApplicationModel application = new ApplicationModel
            {
                Name = applicationSecurityContractDefinition.Fullname,
                ChangedBy = updatedByGuid,
                ApplicationFunctions = new List<ApplicationFunctionModel>(),
                ApplicationDataPolicies = new List<ApplicationDataPolicyModel>()
            };

            if (applicationSecurityContractDefinition.ApplicationFunctions != null)
            {
                foreach (var function in applicationSecurityContractDefinition.ApplicationFunctions)
                {
                    // Application functions should be unique, check that another one does not exist prior to attempting to add it to the application.
                    var existingApplicationFunction = await applicationFunctionRepository.GetByNameAsync(function.Name);

                    if(existingApplicationFunction != null)
                    {
                        var errorMessage = $"[applications.fullname: '{applicationSecurityContractDefinition.Fullname}'].[applicationFunctions.name: '{function.Name}']: Cannot create application function '{function.Name}', as there is already an application function with this nam assigned to another application.";
                        logger.Error(errorMessage);
                        if (dryRun)
                        {
                            securityContractDryRunResult.ValidationErrors.Add(errorMessage);
                            // Attempting to add the function anyway would result in a uniqueness contraint violation and break the transaction.
                            continue;
                        }

                        throw new ItemNotProcessableException(errorMessage);
                    }

                    logger.Error($"Adding function {function.Name} to application.");
                    application.ApplicationFunctions.Add(CreateNewFunctionFromResourceServerFunction(function, updatedByGuid, applicationSecurityContractDefinition.Fullname, dryRun, securityContractDryRunResult));
                }
            }
            // Set an initial value to the un-saved model.
            ApplicationModel newApplication = await applicationRepository.CreateAsync(application);

            return await SynchroniseApplicationDataPoliciesWithSecurityContract(newApplication, applicationSecurityContractDefinition, updatedByGuid, dryRun, securityContractDryRunResult);
        }

        private async Task<ApplicationModel> SynchroniseApplicationDataPoliciesWithSecurityContract(ApplicationModel application, SecurityContractApplication applicationSecurityContractDefinition, Guid updatedById, bool dryRun, SecurityContractDryRunResult securityContractDryRunResult)
        {
            await RemoveApplicationDataPoliciesCurrentlyAssignedToApplicationThatAreNoLongerInSecurityContract(application, applicationSecurityContractDefinition, dryRun, securityContractDryRunResult);
            return await AddApplicationDataPoliciesFromSecurityContractToApplication(application, applicationSecurityContractDefinition, updatedById, dryRun, securityContractDryRunResult);
        }

        private async Task<ApplicationModel> RemoveApplicationDataPoliciesCurrentlyAssignedToApplicationThatAreNoLongerInSecurityContract(ApplicationModel application, SecurityContractApplication applicationSecurityContractDefinition, bool dryRun, SecurityContractDryRunResult securityContractDryRunResult)
        {
            if(application.ApplicationDataPolicies != null && application.ApplicationDataPolicies.Any())
            {
                for (int i = application.ApplicationDataPolicies.Count - 1; i >= 0; i--)
                {
                    if (applicationSecurityContractDefinition.DataPolicies == null || !applicationSecurityContractDefinition.DataPolicies.Exists(dp => dp.Name == application.ApplicationDataPolicies[i].Name))
                    {
                        logger.Debug($"[applications.fullname: '{application.Name}'].[dataPolicies.name]: Data Policy: '{application.ApplicationDataPolicies[i].Name}' was historically assigned to application '{application.Name}', but no longer is within thse security contract being processed. Removing dataPolicy '{application.ApplicationDataPolicies[i].Name}' from application '{application.Name}'!");
                        await applicationDataPolicyRepository.DeleteAsync(application.ApplicationDataPolicies[i]); 
                    }
                }
            }

            return application;
        }

        private async Task<ApplicationModel> AddApplicationDataPoliciesFromSecurityContractToApplication(ApplicationModel application, SecurityContractApplication applicationSecurityContractDefinition, Guid updatedById, bool dryRun, SecurityContractDryRunResult securityContractDryRunResult)
        {
            if (applicationSecurityContractDefinition.DataPolicies != null && applicationSecurityContractDefinition.DataPolicies.Any())
            {
                foreach (var dataPolicyToAdd in applicationSecurityContractDefinition.DataPolicies)
                {
                    logger.Debug($"[applications.fullname: '{application.Name}'].[dataPolicies.name: '{dataPolicyToAdd.Name}']: Adding data policy '{dataPolicyToAdd.Name}' to application '{application.Name}'.");
                    var existingDataPolicy = application.ApplicationDataPolicies.Find(adp => adp.Name == dataPolicyToAdd.Name);

                    if(existingDataPolicy == null)
                    {
                        //check that the data policy does not exist within other applications.
                        var dataPolicyAttachedToOtherApplication = await applicationDataPolicyRepository.GetByNameAsync(dataPolicyToAdd.Name);
                        if(dataPolicyAttachedToOtherApplication != null)
                        {
                            var errorMessage = $"[applications.fullname: '{application.Name}'].[dataPolicies.name: '{dataPolicyToAdd.Name}']: Data policy with name alreay exists in another application. Not adding it!";
                            if (dryRun)
                            {
                                securityContractDryRunResult.ValidationErrors.Add(errorMessage);
                                continue;
                            }

                            throw new ItemNotProcessableException(errorMessage);
                        }

                        logger.Debug($"[applications.fullname: '{application.Name}'].[dataPolicies.name: '{dataPolicyToAdd.Name}']: Data policy '{dataPolicyToAdd.Name}' was not assigned to application '{application.Name}'. Adding it.");
                        application.ApplicationDataPolicies.Add(new ApplicationDataPolicyModel
                        {
                            Name = dataPolicyToAdd.Name,
                            Description = dataPolicyToAdd.Description,
                            ChangedBy = updatedById
                        });
                    }
                    else
                    {
                        logger.Debug($"[applications.fullname: '{application.Name}'].[dataPolicies.name: '{dataPolicyToAdd.Name}']: Data policy '{dataPolicyToAdd.Name}' is currently assigned to application '{application.Name}'. Updating it.");
                        // Bind possible changes to the editable components of the data policy.
                        existingDataPolicy.Description = dataPolicyToAdd.Description;
                        existingDataPolicy.ChangedBy = updatedById;
                    }
                }
            }
            else
            {
                logger.Debug($"[applications.fullname: '{application.Name}'].[dataPolicies]: No application data policies defined for application '{application.Name}'.");
            }

            return await applicationRepository.Update(application);
        }

        private async Task<ApplicationModel> UpdateExistingResourceServer(ApplicationModel application, SecurityContractApplication applicationSecurityContractDefinition, Guid updatedById, bool dryRun, SecurityContractDryRunResult securityContractDryRunResult)
        {
            var updatedApplication = await SynchroniseFunctions(application, applicationSecurityContractDefinition, updatedById, dryRun, securityContractDryRunResult);

            await permissionRepository.DeletePermissionsNotAssignedToApplicationFunctionsAsync();
            await SynchroniseApplicationDataPoliciesWithSecurityContract(application, applicationSecurityContractDefinition, updatedById, dryRun, securityContractDryRunResult);

            return updatedApplication;
        }

        private async Task<ApplicationModel> SynchroniseFunctions(ApplicationModel application, SecurityContractApplication applicationSecurityContractDefinition, Guid updatedByGuid, bool dryRun, SecurityContractDryRunResult securityContractDryRunResult)
        {
            await SynchroniseFunctionsFromResourceServerDefinitionToApplication(application, applicationSecurityContractDefinition, updatedByGuid, dryRun, securityContractDryRunResult);
            await DetectApplicationFunctionsRemovedFromSecurityContractAndRemoveFromApplication(application, applicationSecurityContractDefinition, dryRun, securityContractDryRunResult);

            return application;
        }

        private async Task<ApplicationModel> SynchroniseFunctionsFromResourceServerDefinitionToApplication(ApplicationModel application, SecurityContractApplication applicationSecurityContractDefinition, Guid updatedByGuid, bool dryRun, SecurityContractDryRunResult securityContractDryRunResult)
        {
            if (applicationSecurityContractDefinition.ApplicationFunctions == null)
                return application;

            foreach (var functionResource in applicationSecurityContractDefinition.ApplicationFunctions)
            {
                var applicationFunction = application.ApplicationFunctions.Find(af => af.Name == functionResource.Name);

                if (applicationFunction == null)
                {
                    logger.Debug($"[applications.fullname: '{application.Name}'].[applicationFunctions.name: '{functionResource.Name}']: Application function with name '{functionResource.Name}' does not exist. Creating it.");
                    //We now know this application does not have a function with the name assigned. However, another one might, check for this.
                    var existingApplicationFunction = await applicationFunctionRepository.GetByNameAsync(functionResource.Name);

                    if(existingApplicationFunction != null)
                    {
                        var errorMessage = $"[applications.fullname: '{application.Name}'].[applicationFunctions.name: '{functionResource.Name}']: Application function with name '{functionResource.Name}' already exists in another application. Cannot assign it to application: '{application.Name}'";
                        if (dryRun)
                        {
                            securityContractDryRunResult.ValidationErrors.Add(errorMessage);
                            continue;
                        }

                        throw new ItemNotProcessableException(errorMessage);
                    }

                    application.ApplicationFunctions.Add(CreateNewFunctionFromResourceServerFunction(functionResource, updatedByGuid, applicationSecurityContractDefinition.Fullname, dryRun, securityContractDryRunResult));
                }
                else
                {
                    logger.Debug($"[applications.fullname: '{application.Name}'].[applicationFunctions.name: '{functionResource.Name}']: Application function with name '{functionResource.Name}' already exists. Updating it.");
                    // Edit an existing function.
                    applicationFunction.Name = functionResource.Name;
                    applicationFunction.Description = functionResource.Description;
                    applicationFunction.ChangedBy = updatedByGuid;

                    if (functionResource.Permissions != null)
                    {
                        // Add any new permissions to the function.
                        foreach (var permission in functionResource.Permissions)
                        {
                            AddResourcePermissionToFunctionAndUpdatePermissionIfChanged(applicationFunction, permission, updatedByGuid, applicationSecurityContractDefinition.Fullname, dryRun, securityContractDryRunResult);
                        }

                        DetectAndUnassignPermissionsRemovedFromFunctions(applicationFunction, functionResource);
                    }
                    else
                    {
                        // Remove any possible permissions that are assigned to the application function.
                        applicationFunction.ApplicationFunctionPermissions.Clear();
                    }
                }
            }

            return await applicationRepository.Update(application);
        }

        private async Task<ApplicationModel> DetectApplicationFunctionsRemovedFromSecurityContractAndRemoveFromApplication(ApplicationModel application, SecurityContractApplication applicationSecurityContractDefinition, bool dryRun, SecurityContractDryRunResult securityContractDryRunResult)
        {
            if (application.ApplicationFunctions.Count > 0)
            {
                for (int i = application.ApplicationFunctions.Count - 1; i >= 0; i--)
                {
                    if (applicationSecurityContractDefinition.ApplicationFunctions == null || !applicationSecurityContractDefinition.ApplicationFunctions.Exists(f => f.Name == application.ApplicationFunctions[i].Name))
                    {
                        logger.Debug($"[applications.fullname: '{application.Name}'].[applicationFunctions.name: '{application.ApplicationFunctions[i].Name}']: ApplicationFunction: '{application.ApplicationFunctions[i].Name}' was previously assigned to application '{application.Name}' but no longer is within the security contract being processed. Un-assigning ApplicationFunction '{application.ApplicationFunctions[i].Name}' from application '{application.Name}'!");
                        // Note: This only removes the application function permissions association. The permission will still exist. We cannot remove the permission here, as it may be assigned to other functions.
                        await applicationFunctionRepository.DeleteAsync(application.ApplicationFunctions[i]);
                    }
                }
            }

            return application;
        }

        private void DetectAndUnassignPermissionsRemovedFromFunctions(ApplicationFunctionModel applicationFunction, SecurityContractFunction functionResource)
        {
            // Remove any permissions from the application function that are not within the updated definition.
            // Note! We are deleting items from the List so we cannot use a foreach.
            for (int i = applicationFunction.ApplicationFunctionPermissions.Count - 1; i >= 0; i--)
            {
                if (!functionResource.Permissions.Exists(fp => fp.Name == applicationFunction.ApplicationFunctionPermissions[i].Permission.Name))
                {
                    logger.Debug($"[applications.fullname: '{applicationFunction.Application.Name}'].[applicationFunctions.name: '{applicationFunction.Name}'].[permissions.name: '{applicationFunction.ApplicationFunctionPermissions[i].Permission.Name}']: Permission: {applicationFunction.ApplicationFunctionPermissions[i].Permission.Name} was previously assigned to applicationFunction: '{applicationFunction.Name}' but is no longer assigned in the security contract being processed. Removing permission '{applicationFunction.ApplicationFunctionPermissions[i].Permission.Name}' from function '{applicationFunction.Name}'!");
                    // Note: This only removes the function permissions association. The permission will still exist.
                    applicationFunction.ApplicationFunctionPermissions.Remove(applicationFunction.ApplicationFunctionPermissions[i]);
                }
            }
        }

        private ApplicationFunctionModel CreateNewFunctionFromResourceServerFunction(SecurityContractFunction functionResource, Guid updatedByGuid, string applicationName, bool dryRun, SecurityContractDryRunResult securityContractDryRunResult)
        {
            logger.Debug($"[applications.fullname: '{applicationName}'].[applicationFunctions.name: '{functionResource.Name}']: Adding function '{functionResource.Name}' to application '{applicationName}'.");
            ApplicationFunctionModel newFunction = new ApplicationFunctionModel
            {
                Name = functionResource.Name,
                Description = functionResource.Description,
                ChangedBy = updatedByGuid
            };

            newFunction.ApplicationFunctionPermissions = new List<ApplicationFunctionPermissionModel>();

            if (functionResource.Permissions != null)
            {
                foreach (var permission in functionResource.Permissions)
                {
                    AddResourcePermissionToFunctionAndUpdatePermissionIfChanged(newFunction, permission, updatedByGuid, applicationName, dryRun, securityContractDryRunResult);
                }
            }

            return newFunction;
        }

        private void AddResourcePermissionToFunctionAndUpdatePermissionIfChanged(ApplicationFunctionModel applicationFunction, SecurityContractPermission permission, Guid updatedByGuid, string applicationName, bool dryRun, SecurityContractDryRunResult securityContractDryRunResult)
        {
            logger.Debug($"[applications.fullname: '{applicationName}'].[applicationFunctions.name: '{applicationFunction.Name}'].[permissions.name: '{permission.Name}']: Attempting to assign permission '{permission.Name}' to function: {applicationFunction.Name}.");
            // Check if there is an existing permission within the database. Add this one if found, but only if it is assigned to the current application, else create a new one and add it.
            var existingPermission = permissionRepository.GetByName(permission.Name, true);

            if (existingPermission != null)
            {
                // Check that the existing permission is not assigned to another application.
                var existingPermissionApplication = existingPermission.ApplicationFunctionPermissions.Find(afp => afp.ApplicationFunction.Application.Name == applicationName);
                if (existingPermissionApplication == null)
                {
                    var errorMessage = $"[applications.fullname: '{applicationName}'].[applicationFunctions.name: '{applicationFunction.Name}'].[permissions.name: '{permission.Name}']: Permission name exists, but is not assigned to application '{applicationName}'. Cannot assign it to application '{applicationName}', as permissions can only be assigned to a single application";
                    if (dryRun)
                    {
                        securityContractDryRunResult.ValidationErrors.Add(errorMessage);
                        return;
                    }

                    throw new ItemNotProcessableException(errorMessage);
                }

                logger.Debug($"[applications.fullname: '{applicationName}'].[applicationFunctions.name: '{applicationFunction.Name}'].[permissions.name: '{permission.Name}']: Permission '{permission.Name}' already assigned to application '{applicationName}'. Updating it.");
                var applicationFunctionPermission = applicationFunction.ApplicationFunctionPermissions.Find(fp => fp.Permission.Name == permission.Name);

                // This check will be true if the permission is assigned to another function attached to the application.
                if(applicationFunctionPermission == null)
                {
                    logger.Debug($"[applications.fullname: '{applicationName}'].[applicationFunctions.name: '{applicationFunction.Name}'].[permissions.name: '{permission.Name}']: Permission '{permission.Name}' already assigned to another function within '{applicationName}'. Adding it to additional function '{applicationFunction.Name}'");

                    // Still check if the permission is to be updated.
                    if (existingPermission.Description != permission.Description)
                    {
                        existingPermission.Description = permission.Description;
                        existingPermission.ChangedBy = updatedByGuid;
                    }

                    applicationFunction.ApplicationFunctionPermissions.Add(new ApplicationFunctionPermissionModel
                    {
                        ApplicationFunction = applicationFunction,
                        Permission = existingPermission,
                        ChangedBy = updatedByGuid
                    });
                }
                // Still check if the permission is to be updated.
                else if (applicationFunctionPermission.Permission.Description != permission.Description)
                {
                    applicationFunctionPermission.Permission.Description = permission.Description;
                    applicationFunctionPermission.Permission.ChangedBy = updatedByGuid;
                }
            }
            else
            {
                logger.Debug($"[applications.fullname: '{applicationName}'].[applicationFunctions.name: '{applicationFunction.Name}'].[permissions.name: '{permission.Name}']: Permission '{permission.Name}' does not exist in A3S. Adding it.");
                PermissionModel permissionToAdd = new PermissionModel
                {
                    Name = permission.Name,
                    Description = permission.Description,
                    ChangedBy = updatedByGuid
                };

                applicationFunction.ApplicationFunctionPermissions.Add(new ApplicationFunctionPermissionModel
                {
                    ApplicationFunction = applicationFunction,
                    Permission = permissionToAdd,
                    ChangedBy = updatedByGuid
                });
            }
        }

        public async Task<List<SecurityContractApplication>> GetResourceServerDefinitionsAsync()
        {
            logger.Debug($"Retrieving application security contract definitions.");

            var contractApplications = new List<SecurityContractApplication>();
            List<ApplicationModel> applications = await applicationRepository.GetListAsync();

            foreach (var application in applications.OrderBy(o => o.SysPeriod.LowerBound))
            {
                logger.Debug($"Retrieving application security contract definition for Application [{application.Name}].");

                var contractApplication = new SecurityContractApplication()
                {
                    Fullname = application.Name,
                    ApplicationFunctions = new List<SecurityContractFunction>()
                };

                foreach (var applicationFunction in application.ApplicationFunctions.OrderBy(o => o.SysPeriod.LowerBound))
                {
                    logger.Debug($"Retrieving application security contract definition for ApplicationFunction [{applicationFunction.Name}].");

                    var contractAppFunction = new SecurityContractFunction()
                    {
                        Name = applicationFunction.Name,
                        Description = applicationFunction.Description,
                        Permissions = new List<SecurityContractPermission>()
                    };

                    foreach (var applicationPermission in applicationFunction.ApplicationFunctionPermissions.OrderBy(o => o.Permission.SysPeriod.LowerBound))
                    {
                        logger.Debug($"Retrieving application security contract definition for ApplicationPermission [{applicationPermission.Permission.Name}].");

                        contractAppFunction.Permissions.Add(new SecurityContractPermission()
                        {
                            Name = applicationPermission.Permission.Name,
                            Description = applicationPermission.Permission.Description
                        });
                    }

                    contractApplication.ApplicationFunctions.Add(contractAppFunction);

                    AddApplicationDataPoliciesToSecurityContractDefinintionFromApplication(contractApplication, application);
                }

                contractApplications.Add(contractApplication);
            }

            return contractApplications;
        }

        private void AddApplicationDataPoliciesToSecurityContractDefinintionFromApplication(SecurityContractApplication contractApplication, ApplicationModel application)
        {
            logger.Debug($"Retrieving application data policies for application '{application.Name}'");
            contractApplication.DataPolicies = new List<SecurityContractApplicationDataPolicy>();

            if (application.ApplicationDataPolicies != null && application.ApplicationDataPolicies.Any())
            {
                foreach (var applicationDataPolicy in application.ApplicationDataPolicies)
                {
                    logger.Debug($"Found data policy '{applicationDataPolicy.Name}' for application '{application.Name}'");
                    contractApplication.DataPolicies.Add(new SecurityContractApplicationDataPolicy
                    {
                        Name = applicationDataPolicy.Name,
                        Description = applicationDataPolicy.Description
                    });
                }
            }
        }

        public void InitSharedTransaction()
        {
            applicationRepository.InitSharedTransaction();
            identityApiResourceRespository.InitSharedTransaction();
            permissionRepository.InitSharedTransaction();
            applicationFunctionRepository.InitSharedTransaction();
            applicationDataPolicyRepository.InitSharedTransaction();
        }

        public void CommitTransaction()
        {
            applicationRepository.CommitTransaction();
            identityApiResourceRespository.CommitTransaction();
            permissionRepository.CommitTransaction();
            applicationFunctionRepository.CommitTransaction();
            applicationDataPolicyRepository.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            applicationRepository.RollbackTransaction();
            identityApiResourceRespository.RollbackTransaction();
            permissionRepository.RollbackTransaction();
            applicationFunctionRepository.RollbackTransaction();
            applicationDataPolicyRepository.RollbackTransaction();
        }
    }
}
