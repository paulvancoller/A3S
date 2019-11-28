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
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using IdentityServer4.EntityFramework.Entities;
using NLog;
using za.co.grindrodbank.a3s.A3SApiResources;

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

        public async Task<ApplicationModel> ApplyResourceServerDefinitionAsync(SecurityContractApplication applicationSecurityContractDefinition, Guid updatedById, bool dryRun, List<string> validationErrors)
        {
            logger.Debug($"Applying application security contract definition for application: '{applicationSecurityContractDefinition.Fullname}'");
            // Attempt to load any existing application by name, as the name is essentially the API primary key.
            var application = await applicationRepository.GetByNameAsync(applicationSecurityContractDefinition.Fullname);

            if (application == null)
            {
                logger.Debug($"Application '{applicationSecurityContractDefinition.Fullname}' not found in database. Creating new application.");
                return await CreateNewResourceServer(applicationSecurityContractDefinition, updatedById, dryRun, validationErrors);
            }
            logger.Debug($"Application '{applicationSecurityContractDefinition.Fullname}' already exists. Updating it.");
            return await UpdateExistingResourceServer(application, applicationSecurityContractDefinition, updatedById, dryRun, validationErrors);
        }

        private async Task<ApplicationModel> CreateNewResourceServer(SecurityContractApplication applicationSecurityContractDefinition, Guid updatedByGuid, bool dryRun, List<string> validationErrors)
        {
            try
            {
                // Note: Always added 'permission' as the user claims that need to be mapped into access tokens for this API Resource.
                ApiResource identityServerApiResource = await identityApiResourceRespository.GetByNameAsync(applicationSecurityContractDefinition.Fullname);

                if(identityServerApiResource == null)
                {
                    await identityApiResourceRespository.CreateAsync(applicationSecurityContractDefinition.Fullname, new[] { "permission" });
                }
                else
                {
                    logger.Warn($"The API Resource with name '{applicationSecurityContractDefinition.Fullname}' already exists on the Identity Server. Not creating a new one!");
                }
            }
            catch (Exception e)
            {
                string errMessage = String.Format($"Error creating new resource on Identity Server: {e.Message}");
                logger.Error(errMessage);

                if (dryRun)
                {
                    validationErrors.Add(errMessage);
                }
                else
                {
                    throw;
                }
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
                    application.ApplicationFunctions.Add(CreateNewFunctionFromResourceServerFunction(function, updatedByGuid));
                }
            }

            // Set an initial value to the un-saved model.
            var newApplication = application;

            try
            {
                newApplication = await applicationRepository.CreateAsync(application);
            } catch(Exception e)
            {
                if (dryRun)
                {
                    validationErrors.Add($"Error saving new application with name: '{application.Name}'. Exception: {e.Message}");
                }
                else
                {
                    throw;
                }
            }
            
            return await SynchroniseApplicationDataPoliciesWithSecurityContract(newApplication, applicationSecurityContractDefinition, updatedByGuid, dryRun, validationErrors);
        }

        private async Task<ApplicationModel> SynchroniseApplicationDataPoliciesWithSecurityContract(ApplicationModel application, SecurityContractApplication applicationSecurityContractDefinition, Guid updatedById, bool dryRun, List<string> validationErrors)
        {
            await RemoveApplicationDataPoliciesCurrentlyAssignedToApplicationThatAreNoLongerInSecurityContract(application, applicationSecurityContractDefinition, dryRun, validationErrors);
            return await AddApplicationDataPoliciesFromSecurityContractToApplication(application, applicationSecurityContractDefinition, updatedById, dryRun, validationErrors);
        }

        private async Task<ApplicationModel> RemoveApplicationDataPoliciesCurrentlyAssignedToApplicationThatAreNoLongerInSecurityContract(ApplicationModel application, SecurityContractApplication applicationSecurityContractDefinition, bool dryRun, List<string> validationErrors)
        {
            if(application.ApplicationDataPolicies != null && application.ApplicationDataPolicies.Any())
            {
                for (int i = application.ApplicationDataPolicies.Count - 1; i >= 0; i--)
                {
                    logger.Debug($"Checking whether application data policy: '{application.ApplicationDataPolicies[i].Name}' should unassigned from application '{application.Name}'.");
                    if (applicationSecurityContractDefinition.DataPolicies == null || !applicationSecurityContractDefinition.DataPolicies.Exists(dp => dp.Name == application.ApplicationDataPolicies[i].Name))
                    {
                        logger.Debug($"Data Policy: '{application.ApplicationDataPolicies[i].Name}' is being unassigned from application '{application.Name}'!");
                        try {
                            await applicationDataPolicyRepository.DeleteAsync(application.ApplicationDataPolicies[i]);
                        }
                        catch (Exception e)
                        {
                            if (dryRun)
                            {
                                validationErrors.Add($"Error deleting data-policy {application.ApplicationDataPolicies[i].Name} from application '{application.Name}'. Exception: '{e.Message}'");
                            }
                            else
                            {
                                throw;
                            }
                        }
                        
                    }
                }
            }

            return application;
        }

        private async Task<ApplicationModel> AddApplicationDataPoliciesFromSecurityContractToApplication(ApplicationModel application, SecurityContractApplication applicationSecurityContractDefinition, Guid updatedById, bool dryRun, List<string> validationErrors)
        {
            if (applicationSecurityContractDefinition.DataPolicies != null && applicationSecurityContractDefinition.DataPolicies.Any())
            {
                foreach (var dataPolicyToAdd in applicationSecurityContractDefinition.DataPolicies)
                {
                    logger.Debug($"Adding data policy from security contract: {dataPolicyToAdd.Name}");
                    var existingDataPolicy = application.ApplicationDataPolicies.Find(adp => adp.Name == dataPolicyToAdd.Name);

                    if(existingDataPolicy == null)
                    {
                        logger.Debug($"Data policy '{dataPolicyToAdd.Name}' was not assigned to application '{application.Name}'. Adding it.");
                        application.ApplicationDataPolicies.Add(new ApplicationDataPolicyModel
                        {
                            Name = dataPolicyToAdd.Name,
                            Description = dataPolicyToAdd.Description,
                            ChangedBy = updatedById
                        });
                    }
                    else
                    {
                        logger.Debug($"Data policy '{dataPolicyToAdd.Name}' is currently assigned to application '{application.Name}'. Updating it.");
                        // Bind possible changes to the editable components of the data policy.
                        existingDataPolicy.Description = dataPolicyToAdd.Description;
                        existingDataPolicy.ChangedBy = updatedById;
                    }
                }
            }
            else
            {
                logger.Debug($"No application data policies defined for application '{application.Name}'.");
            }

            try
            {
                return await applicationRepository.Update(application);
            } catch (Exception e)
            {
                if (dryRun)
                {
                    validationErrors.Add($"Error updating application '{application.Name}' with data-policies. Error: '{e.Message}'");
                    // The best we can do is return the non-updated application in an attempt to carry on.
                    return application;
                }

                throw;
            }
            
        }

        private async Task<ApplicationModel> UpdateExistingResourceServer(ApplicationModel application, SecurityContractApplication applicationSecurityContractDefinition, Guid updatedById, bool dryRun, List<string> validationErrors)
        {
            var updatedApplication = await SynchroniseFunctions(application, applicationSecurityContractDefinition, updatedById, dryRun, validationErrors);

            try
            {
                await permissionRepository.DeletePermissionsNotAssignedToApplicationFunctionsAsync();
            } catch(Exception e)
            {
                if (dryRun)
                {
                    validationErrors.Add($"Error deleting permissions not assigned to applications. Error: '{e.Message}'");
                }
                else
                {
                    throw;
                }
            }
            
            await SynchroniseApplicationDataPoliciesWithSecurityContract(application, applicationSecurityContractDefinition, updatedById, dryRun, validationErrors);

            return updatedApplication;
        }

        private async Task<ApplicationModel> SynchroniseFunctions(ApplicationModel application, SecurityContractApplication applicationSecurityContractDefinition, Guid updatedByGuid, bool dryRun, List<string> validationErrors)
        {
            await SynchroniseFunctionsFromResourceServerDefinitionToApplication(application, applicationSecurityContractDefinition, updatedByGuid, dryRun, validationErrors);
            await DetectApplicationFunctionsRemovedFromSecurityContractAndRemoveFromApplication(application, applicationSecurityContractDefinition, dryRun, validationErrors);

            return application;
        }

        private async Task<ApplicationModel> SynchroniseFunctionsFromResourceServerDefinitionToApplication(ApplicationModel application, SecurityContractApplication applicationSecurityContractDefinition, Guid updatedByGuid, bool dryRun, List<string> validationErrors)
        {
            if (applicationSecurityContractDefinition.ApplicationFunctions == null)
                return application;

            foreach (var functionResource in applicationSecurityContractDefinition.ApplicationFunctions)
            {
                var applicationFunction = application.ApplicationFunctions.Find(af => af.Name == functionResource.Name);

                if (applicationFunction == null)
                {
                    application.ApplicationFunctions.Add(CreateNewFunctionFromResourceServerFunction(functionResource, updatedByGuid));
                }
                else
                {
                    // Edit an existing function.
                    applicationFunction.Name = functionResource.Name;
                    applicationFunction.Description = functionResource.Description;
                    applicationFunction.ChangedBy = updatedByGuid;

                    if (functionResource.Permissions != null)
                    {
                        // Add any new permissions to the function.
                        foreach (var permission in functionResource.Permissions)
                        {
                            AddPermissionToFunctionIfNotAlreadyAssigned(applicationFunction, permission, updatedByGuid);
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

            try
            {
                return await applicationRepository.Update(application);
            } catch (Exception e)
            {
                if (dryRun)
                {
                    validationErrors.Add($"Error updating functions for application '{application.Name}'. Error: '{e.Message}'");
                    // The best we can do is return the application that was not updated.
                    return application;
                }
            
                throw;
            }
            
        }

        private async Task<ApplicationModel> DetectApplicationFunctionsRemovedFromSecurityContractAndRemoveFromApplication(ApplicationModel application, SecurityContractApplication applicationSecurityContractDefinition, bool dryRun, List<string> validationErrors)
        {
            if (application.ApplicationFunctions.Count > 0)
            {
                for (int i = application.ApplicationFunctions.Count - 1; i >= 0; i--)
                {
                    logger.Debug($"Checking whether application function: '{application.ApplicationFunctions[i].Name}' should unassigned from application '{application.Name}'.");
                    if (applicationSecurityContractDefinition.ApplicationFunctions == null || !applicationSecurityContractDefinition.ApplicationFunctions.Exists(f => f.Name == application.ApplicationFunctions[i].Name))
                    {
                        logger.Debug($"Function: '{application.ApplicationFunctions[i].Name}' is being unassigned from application '{application.Name}' !");
                        // Note: This only removes the application function permissions association. The permission will still exist. We cannot remove the permission here, as it may be assigned to other functions.
                        try
                        {
                            await applicationFunctionRepository.DeleteAsync(application.ApplicationFunctions[i]);
                        } catch (Exception e)
                        {
                            if (dryRun)
                            {
                                validationErrors.Add($"Error deleting application function '{application.ApplicationFunctions[i].Name}' for application '{application.Name}'. Error: {e.Message} ");
                            }
                            else
                            {
                                throw;
                            }
                        }
                        
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
                logger.Debug($"Checking whether permission: {applicationFunction.ApplicationFunctionPermissions[i].Permission.Name} should unassigned from function '" + applicationFunction.Name + "'.");
                if (!functionResource.Permissions.Exists(fp => fp.Name == applicationFunction.ApplicationFunctionPermissions[i].Permission.Name))
                {
                    logger.Debug($"Permission: {applicationFunction.ApplicationFunctionPermissions[i].Permission.Name} is being unassigned from function {applicationFunction.Name}!");
                    // Note: This only removes the function permissions association. The permission will still exist.
                    applicationFunction.ApplicationFunctionPermissions.Remove(applicationFunction.ApplicationFunctionPermissions[i]);
                }
            }
        }

        private ApplicationFunctionModel CreateNewFunctionFromResourceServerFunction(SecurityContractFunction functionResource, Guid updatedByGuid)
        {
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
                    AddResourcePermissionToFunction(newFunction, permission, updatedByGuid);
                }
            }

            return newFunction;
        }

        private void AddPermissionToFunctionIfNotAlreadyAssigned(ApplicationFunctionModel applicationFunction, SecurityContractPermission permission, Guid updatedByGuid)
        {
            // add the permission if it does not exist.
            var applicationPermission = applicationFunction.ApplicationFunctionPermissions.Find(fp => fp.Permission.Name == permission.Name);

            if (applicationPermission == null)
            {
                AddResourcePermissionToFunction(applicationFunction, permission, updatedByGuid);
            }
        }

        private void AddResourcePermissionToFunction(ApplicationFunctionModel applicationFuction, SecurityContractPermission permission, Guid updatedByGuid)
        {
            logger.Debug($"Assinging permission {permission.Name} to function: {applicationFuction.Name}.");
            // Check if there is an existing permission within the database. Add this one if found, else create a new one and add it.
            var existingPermission = permissionRepository.GetByName(permission.Name);

            PermissionModel permissionToAdd = new PermissionModel
            {
                Name = permission.Name,
                Description = permission.Description,
                ChangedBy = updatedByGuid
            };

            if (existingPermission != null)
            {
                logger.Debug($"Permission {permission.Name} already exists within the database. Not adding it.");
                permissionToAdd = existingPermission;
            }
            else
            {
                logger.Debug($"Permission {permission.Name} does not exist in the database. Adding it.");
            }

            applicationFuction.ApplicationFunctionPermissions.Add(new ApplicationFunctionPermissionModel
            {
                ApplicationFunction = applicationFuction,
                Permission = permissionToAdd,
                ChangedBy = updatedByGuid
            });
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
