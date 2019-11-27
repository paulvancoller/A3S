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
using za.co.grindrodbank.a3s.Services;
using za.co.grindrodbank.a3s.tests.Fakes;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.A3SApiResources;
using IdentityServer4.EntityFramework.Entities;

namespace za.co.grindrodbank.a3s.tests.Services
{
    public class SecurityContractApplicationService_Tests
    {
        private readonly ApplicationModel mockedApplication;
        private readonly Guid applicationGuid;
        private readonly Guid permissionGuid1;
        private readonly Guid permissionGuid2;
        private readonly Guid permissionGuid3;
        private readonly Guid functionGuid1;
        private readonly Guid functionGuid2;

        public SecurityContractApplicationService_Tests()
        {
            //set up some Guids.
            applicationGuid = Guid.NewGuid();
            permissionGuid1 = Guid.NewGuid();
            permissionGuid2 = Guid.NewGuid();
            permissionGuid3 = Guid.NewGuid();
            functionGuid1 = Guid.NewGuid();
            functionGuid2 = Guid.NewGuid();


            // Set up an application mock
            mockedApplication = new ApplicationModel();

            mockedApplication.Name = "Mocked Application Name";
            mockedApplication.Id = applicationGuid;

            // Set up some permissions to assign to mocked functions
            PermissionModel permissionTest1 = new PermissionModel
            {
                Id = permissionGuid1,
                Name = "Permissions 1"
            };

            PermissionModel permissionTest2 = new PermissionModel
            {
                Id = permissionGuid2,
                Name = "Permissions 2"
            };

            PermissionModel permissionTest3 = new PermissionModel
            {
                Id = permissionGuid3,
                Name = "Permissions 3"
            };

            // Define function 1 and add permission 1 to it.
            ApplicationFunctionModel applicationFunction1 = new ApplicationFunctionModel();
            applicationFunction1.Id = functionGuid1;
            applicationFunction1.Name = "Function 1";
            applicationFunction1.Description = "Function 1 Description";

            applicationFunction1.ApplicationFunctionPermissions = new List<ApplicationFunctionPermissionModel>
            {
                new ApplicationFunctionPermissionModel
                {
                    ApplicationFunction = applicationFunction1,
                    Permission = permissionTest1
                }
            };

            // Define function 2 and add permissions 2 and 3 to it.
            ApplicationFunctionModel applicationFunction2 = new ApplicationFunctionModel();
            applicationFunction2.Id = functionGuid2;
            applicationFunction2.Name = "Function 2";
            applicationFunction2.Description = "Function 2 Description";
            applicationFunction2.ApplicationFunctionPermissions = new List<ApplicationFunctionPermissionModel>
            {
                new ApplicationFunctionPermissionModel
                {
                    ApplicationFunction = applicationFunction2,
                    Permission = permissionTest2
                },
                new ApplicationFunctionPermissionModel
                {
                    ApplicationFunction = applicationFunction2,
                    Permission = permissionTest3
                }
            };

            mockedApplication.ApplicationFunctions = new List<ApplicationFunctionModel>
            {
                applicationFunction1,
                applicationFunction2
            };
        }

        [Fact]
        public async Task ApplyResourceServerDefinitionAsync_ExistingApplicationIsPresentWithNulledFunction_ReturnsUpdatedApplicationWithoutFunctions()
        {
            // Create an application repository that returns the supplied models so that they can be interrogated, rather than going to the DB.
            // The main thng we want to test here is how the service maps from the security contract onto the model, which is lost if we simply
            // mock the model that is returned from the repository.

            // The fake repository has an overloaded constructor that allows us to inject the mocked model it will return when getById or getByName functions
            // are called on the fake repository.
            var applicationRepository = new ApplicationRepositoryFake(mockedApplication);
            var identityServiceApiResourceRepository = Substitute.For<IIdentityApiResourceRepository>();
            var permissionsRepository = Substitute.For<IPermissionRepository>();
            var applicationFunctionRepository = Substitute.For<IApplicationFunctionRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();
            var securityContractApplicationService = new SecurityContractApplicationService(applicationRepository, identityServiceApiResourceRepository, permissionsRepository, applicationFunctionRepository, applicationDataPolicyRepository);

            // Define an application security contract definition.
            var applicationSecurityContract = new SecurityContractApplication();
            // The fake application respository is going to return the mocked application.
            // Also, set the functions section of the application to null (just dont define it). This should set returned application model functions association to null.
            applicationSecurityContract.Fullname = mockedApplication.Name;

            var returnedApplicationModel = await securityContractApplicationService.ApplyResourceServerDefinitionAsync(applicationSecurityContract, Guid.NewGuid());

            Assert.True(returnedApplicationModel.Name == "Mocked Application Name", $"Returned application name: '{returnedApplicationModel.Name}' does not match the expected valueL '{mockedApplication.Name}'");
            // Even though the mock application has application functions associated with it, they should have been removed owing to empty functions section defined in the app security contract.
            //Assert.True(returnedApplicationModel.ApplicationFunctions.Count == 0, $"Returned applications functions count expected to be '0'. Actual count is '{returnedApplicationModel.ApplicationFunctions.Count}'");
        }

        [Fact]
        public async Task ApplyResourceServerDefinitionAsync_NoIdentityServerAPIResourcePresent_ReturnsUpdatedApplicationWithoutFunctions()
        {
            var applicationRepository = Substitute.For<IApplicationRepository>();
            var identityServiceApiResourceRepository = Substitute.For<IIdentityApiResourceRepository>();
            var permissionsRepository = Substitute.For<IPermissionRepository>();
            var applicationFunctionRepository = Substitute.For<IApplicationFunctionRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();

            identityServiceApiResourceRepository.GetByNameAsync(Arg.Any<string>()).Returns(new ApiResource());

            var newApplication = mockedApplication;
            newApplication.ApplicationDataPolicies = new List<ApplicationDataPolicyModel>()
            {
                new ApplicationDataPolicyModel()
                {
                    Name = "Test Name"
                }
            };

            applicationRepository.CreateAsync(Arg.Any<ApplicationModel>()).Returns(newApplication);
            applicationRepository.UpdateAsync(Arg.Any<ApplicationModel>()).Returns(newApplication);

            var securityContractApplicationService = new SecurityContractApplicationService(applicationRepository, identityServiceApiResourceRepository, permissionsRepository, applicationFunctionRepository, applicationDataPolicyRepository);

            // Define an application security contract definition.
            var applicationSecurityContract = new SecurityContractApplication();
            // The fake application respository is going to return the mocked application.
            // Also, set the functions section of the application to null (just dont define it). This should set returned application model functions association to null.
            applicationSecurityContract.Fullname = mockedApplication.Name;

            var returnedApplicationModel = await securityContractApplicationService.ApplyResourceServerDefinitionAsync(applicationSecurityContract, Guid.NewGuid());

            Assert.True(returnedApplicationModel.Name == "Mocked Application Name", $"Returned application name: '{returnedApplicationModel.Name}' does not match the expected valueL '{mockedApplication.Name}'");
            // Even though the mock application has application functions associated with it, they should have been removed owing to empty functions section defined in the app security contract.
            //Assert.True(returnedApplicationModel.ApplicationFunctions.Count == 0, $"Returned applications functions count expected to be '0'. Actual count is '{returnedApplicationModel.ApplicationFunctions.Count}'");
        }

        [Fact]
        public async Task ApplyResourceServerDefinitionAsync_NoApplicationIsPresent_ReturnsNewApplicationWithoutFunctions()
        {
            // Create an application repository that returns the supplied models so that they can be interrogated, rather than going to the DB.
            // The main thng we want to test here is how the service maps from the security contract onto the model, which is lost if we simply
            // mock the model that is returned from the repository.

            // The fake repository has an overloaded constructor that allows us to inject the mocked model it will return when getById or getByName functions
            // are called on the fake repository.
            var applicationRepository = new ApplicationRepositoryFake(null);
            var identityServiceApiResourceRepository = Substitute.For<IIdentityApiResourceRepository>();
            var permissionsRepository = Substitute.For<IPermissionRepository>();
            var applicationFunctionRepository = Substitute.For<IApplicationFunctionRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();

            var securityContractApplicationService = new SecurityContractApplicationService(applicationRepository, identityServiceApiResourceRepository, permissionsRepository, applicationFunctionRepository, applicationDataPolicyRepository);

            // Define an application security contract definition.
            var applicationSecurityContract = new SecurityContractApplication();
            // The fake application respository is going to return the mocked application.
            // Also, set the functions section of the application to null (just dont define it). This should set returned application model functions association to null.
            applicationSecurityContract.Fullname = "Test Application Fullname";

            var returnedApplicationModel = await securityContractApplicationService.ApplyResourceServerDefinitionAsync(applicationSecurityContract, Guid.NewGuid());

            Assert.True(returnedApplicationModel.Name == applicationSecurityContract.Fullname, $"Returned application name: '{returnedApplicationModel.Name}' does not match the expected valueL '{applicationSecurityContract.Fullname}'");
            // Even though the mock application has application functions associated with it, they should have been removed owing to empty functions section defined in the app security contract.
            Assert.True(returnedApplicationModel.ApplicationFunctions.Count == 0, $"Returned applications functions count expected to be '0'. Actual count is '{returnedApplicationModel.ApplicationFunctions.Count}'");

        }

        [Fact]
        public async Task ApplyResourceServerDefinitionAsync_NoApplicationIsPresent_ReturnsNewApplicationWithFunctions()
        {
            // Create an application repository that returns the supplied models so that they can be interrogated, rather than going to the DB.
            // The main thng we want to test here is how the service maps from the security contract onto the model, which is lost if we simply
            // mock the model that is returned from the repository.

            // The fake repository has an overloaded constructor that allows us to inject the mocked model it will return when getById or getByName functions
            // are called on the fake repository.
            var applicationRepository = new ApplicationRepositoryFake(null);
            var identityServiceApiResourceRepository = Substitute.For<IIdentityApiResourceRepository>();
            var permissionsRepository = Substitute.For<IPermissionRepository>();
            var applicationFunctionRepository = Substitute.For<IApplicationFunctionRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();

            var securityContractApplicationService = new SecurityContractApplicationService(applicationRepository, identityServiceApiResourceRepository, permissionsRepository, applicationFunctionRepository, applicationDataPolicyRepository);

            // Define an application security contract definition.
            var applicationSecurityContract = new SecurityContractApplication();
            // The fake application respository is going to return the mocked application.
            // Also, set the functions section of the application to null (just dont define it). This should set returned application model functions association to null.
            applicationSecurityContract.Fullname = "Test Application Fullname";
            applicationSecurityContract.ApplicationFunctions = new List<SecurityContractFunction> {
                new SecurityContractFunction
                {
                    Name = "Test Function 1",
                    Description = "Test Function 1 description",
                    Permissions = new List <SecurityContractPermission>{
                        new SecurityContractPermission
                        {
                            Name = "Permission 1",
                            Description = "Permission 1 description"
                        }
                    }
                }
            };

            var returnedApplicationModel = await securityContractApplicationService.ApplyResourceServerDefinitionAsync(applicationSecurityContract, Guid.NewGuid());

            Assert.True(returnedApplicationModel.Name == applicationSecurityContract.Fullname, $"Returned application name: '{returnedApplicationModel.Name}' does not match the expected value '{applicationSecurityContract.Fullname}'");
            // Even though the mock application has application functions associated with it, they would NOT have actually been removed as this requires the actual deletion to happen in order for the collection to be udpated.
            Assert.True(returnedApplicationModel.ApplicationFunctions.Count == 1, $"Returned applications functions count expected to be '1'. Actual count is '{returnedApplicationModel.ApplicationFunctions.Count}'");
            Assert.True(returnedApplicationModel.ApplicationFunctions.First().Name == "Test Function 1", $"Returned function name expected to be 'Test Function 1'. Actual value is: '{returnedApplicationModel.ApplicationFunctions.First().Name}'");
        }

        [Fact]
        public async Task ApplyResourceServerDefinitionAsync_ExistingApplicationIsPresent_ReturnsUpdatedApplicationWithUpdatedFunctions()
        {
            // Create an application repository that returns the supplied models so that they can be interrogated, rather than going to the DB.
            // The main thng we want to test here is how the service maps from the security contract onto the model, which is lost if we simply
            // mock the model that is returned from the repository.

            // The fake repository has an overloaded constructor that allows us to inject the mocked model it will return when getById or getByName functions
            // are called on the fake repository.
            var applicationRepository = new ApplicationRepositoryFake(mockedApplication);
            var identityServiceApiResourceRepository = Substitute.For<IIdentityApiResourceRepository>();
            var permissionsRepository = Substitute.For<IPermissionRepository>();
            var applicationFunctionRepository = Substitute.For<IApplicationFunctionRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();
            var securityContractApplicationService = new SecurityContractApplicationService(applicationRepository, identityServiceApiResourceRepository, permissionsRepository, applicationFunctionRepository, applicationDataPolicyRepository);

            // Define an application security contract definition.
            var applicationSecurityContract = new SecurityContractApplication();
            // The fake application respository is going to return the mocked application.
            // Also, set the functions section of the application to null (just dont define it). This should set returned application model functions association to null.
            // The application name is the primary key when dealing with the YAML so set it to the name on the mocked model.
            applicationSecurityContract.Fullname = "Mocked Application Name";
            applicationSecurityContract.ApplicationFunctions = new List<SecurityContractFunction> {
                new SecurityContractFunction
                {
                    Name = "Test Function 1",
                    Description = "Test Function 1 description",
                    Permissions = new List <SecurityContractPermission>{
                        new SecurityContractPermission
                        {
                            Name = "Permission 1",
                            Description = "Permission 1 description"
                        }
                    }
                }
            };

            var returnedApplicationModel = await securityContractApplicationService.ApplyResourceServerDefinitionAsync(applicationSecurityContract, Guid.NewGuid());

            Assert.True(returnedApplicationModel.Name == applicationSecurityContract.Fullname, $"Returned application name: '{returnedApplicationModel.Name}' does not match the expected value '{applicationSecurityContract.Fullname}'");
            // Even though the mock application has application functions associated with it, they would NOT have actually been removed as this requires the actual deletion to happen in order for the collection to be udpated.
            // This also applies to the function elements.
            Assert.True(returnedApplicationModel.ApplicationFunctions.Count == 3, $"Returned applications functions count expected to be '3'. Actual count is '{returnedApplicationModel.ApplicationFunctions.Count}'");
            Assert.True(returnedApplicationModel.ApplicationFunctions.First().Name == "Function 1", $"Returned function name expected to be 'Function 1'. Actual value is: '{returnedApplicationModel.ApplicationFunctions.First().Name}'");
        }

        [Fact]
        public void InitSharedTransaction_Executed_ExecutesWithNoException()
        {
            var applicationRepository = Substitute.For<IApplicationRepository>();
            var identityServiceApiResourceRepository = Substitute.For<IIdentityApiResourceRepository>();
            var permissionsRepository = Substitute.For<IPermissionRepository>();
            var applicationFunctionRepository = Substitute.For<IApplicationFunctionRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();
            var securityContractApplicationService = new SecurityContractApplicationService(applicationRepository, identityServiceApiResourceRepository, permissionsRepository, applicationFunctionRepository, applicationDataPolicyRepository);

            Exception caughtException = null;

            try
            {
                securityContractApplicationService.InitSharedTransaction();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.True(caughtException == null, "InitSharedTransaction must not throw an exception.");
        }

        [Fact]
        public void CommitTransaction_Executed_ExecutesWithNoException()
        {
            var applicationRepository = Substitute.For<IApplicationRepository>();
            var identityServiceApiResourceRepository = Substitute.For<IIdentityApiResourceRepository>();
            var permissionsRepository = Substitute.For<IPermissionRepository>();
            var applicationFunctionRepository = Substitute.For<IApplicationFunctionRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();
            var securityContractApplicationService = new SecurityContractApplicationService(applicationRepository, identityServiceApiResourceRepository, permissionsRepository, applicationFunctionRepository, applicationDataPolicyRepository);

            Exception caughtException = null;

            try
            {
                securityContractApplicationService.CommitTransaction();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.True(caughtException == null, "CommitTransaction must not throw an exception.");
        }

        [Fact]
        public void RollbackTransaction_Executed_ExecutesWithNoException()
        {
            var applicationRepository = Substitute.For<IApplicationRepository>();
            var identityServiceApiResourceRepository = Substitute.For<IIdentityApiResourceRepository>();
            var permissionsRepository = Substitute.For<IPermissionRepository>();
            var applicationFunctionRepository = Substitute.For<IApplicationFunctionRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();
            var securityContractApplicationService = new SecurityContractApplicationService(applicationRepository, identityServiceApiResourceRepository, permissionsRepository, applicationFunctionRepository, applicationDataPolicyRepository);

            Exception caughtException = null;

            try
            {
                securityContractApplicationService.RollbackTransaction();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.True(caughtException == null, "RollbackTransaction must not throw an exception.");
        }
    }
}
