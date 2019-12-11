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
using za.co.grindrodbank.a3s.MappingProfiles;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;
using AutoMapper;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Exceptions;

namespace za.co.grindrodbank.a3s.tests.Services
{
    public class FunctionService_Tests
    {
        private readonly IMapper mapper;
        private readonly FunctionModel mockedFunctionModel;
        private readonly Guid guid;
        private readonly FunctionSubmit mockedFunctionSubmitModel;

        public FunctionService_Tests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new FunctionResourceFunctionModelProfile());
                cfg.AddProfile(new PermissionResourcePermisionModelProfile());
            });

            mapper = config.CreateMapper();

            guid = Guid.NewGuid();
            var applicationGuid = Guid.NewGuid();
            var permissionsGuid = Guid.NewGuid();

            mockedFunctionModel = new FunctionModel();
            mockedFunctionModel.Id = guid;
            mockedFunctionModel.Name = "Test function name";
            mockedFunctionModel.Description = "Test description";
            mockedFunctionModel.Application = new ApplicationModel
            {
                Name = "Test Application",
                Id = applicationGuid
            };

            mockedFunctionModel.FunctionPermissions = new List<FunctionPermissionModel>
            {
                new FunctionPermissionModel
                {
                    Function = mockedFunctionModel,
                    Permission = new PermissionModel
                    {
                        Name = "Test permission",
                        Description = "Test permissions description",
                        Id = permissionsGuid,
                        ApplicationFunctionPermissions = new List<ApplicationFunctionPermissionModel>()
                        {
                            new ApplicationFunctionPermissionModel()
                            {
                                ApplicationFunctionId = mockedFunctionModel.Application.Id,
                                PermissionId = permissionsGuid,
                                ApplicationFunction = new ApplicationFunctionModel()
                                {
                                    Application = mockedFunctionModel.Application
                                }
                            }
                        }
                    }
                }
            };

            mockedFunctionSubmitModel = new FunctionSubmit()
            {
                Uuid = mockedFunctionModel.Id,
                Name = mockedFunctionModel.Name,
                ApplicationId = mockedFunctionModel.Application.Id,
                Permissions = new List<Guid>()
            };

            foreach (var permission in mockedFunctionModel.FunctionPermissions)
                mockedFunctionSubmitModel.Permissions.Add(permission.PermissionId);
        }

        [Fact]
        public async Task GetById_GivenGuid_ReturnsFunctionResource()
        {
            var functionRepository = Substitute.For<IFunctionRepository>();
            var permissionRepository = Substitute.For<IPermissionRepository>();
            var applicationRepository = Substitute.For<IApplicationRepository>();

            functionRepository.GetByIdAsync(guid).Returns(mockedFunctionModel);

            var functionService = new FunctionService(functionRepository, permissionRepository, applicationRepository, mapper);
            var functionResource = await functionService.GetByIdAsync(guid);

            Assert.True(functionResource.Name == "Test function name", $"Function Resource name: '{functionResource.Name}' not the expected value: 'Test Function name'");
            Assert.True(functionResource.Uuid == guid, $"Function Resource UUId: '{functionResource.Uuid}' not the expected value: '{guid}'");
        }

        [Fact]
        public async Task CreateAsync_GivenUnfindableApplication_ThrowsItemNotFoundException()
        {
            // Arrange
            var functionRepository = Substitute.For<IFunctionRepository>();
            var permissionRepository = Substitute.For<IPermissionRepository>();
            var applicationRepository = Substitute.For<IApplicationRepository>();

            permissionRepository.GetByIdWithApplicationAsync(mockedFunctionModel.FunctionPermissions[0].PermissionId)
                .Returns(mockedFunctionModel.FunctionPermissions[0].Permission);

            var functionService = new FunctionService(functionRepository, permissionRepository, applicationRepository, mapper);

            // Act
            Exception caughEx = null;

            try
            {
                var functionResource = await functionService.CreateAsync(mockedFunctionSubmitModel, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughEx = ex;
            }

            // Assert
            Assert.True(caughEx is ItemNotFoundException, "Unfindable Applications must throw an ItemNotFoundException");
        }

        [Fact]
        public async Task CreateAsync_GivenUnfindablePermission_ThrowsItemNotFoundException()
        {
            // Arrange
            var functionRepository = Substitute.For<IFunctionRepository>();
            var permissionRepository = Substitute.For<IPermissionRepository>();
            var applicationRepository = Substitute.For<IApplicationRepository>();

            applicationRepository.GetByIdAsync(mockedFunctionModel.Application.Id)
                .Returns(mockedFunctionModel.Application);

            var functionService = new FunctionService(functionRepository, permissionRepository, applicationRepository, mapper);

            // Act
            Exception caughEx = null;

            try
            {
                var functionResource = await functionService.CreateAsync(mockedFunctionSubmitModel, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughEx = ex;
            }

            // Assert
            Assert.True(caughEx is ItemNotFoundException, "Unfindable Permissions must throw an ItemNotFoundException.");
        }

        [Fact]
        public async Task CreateAsync_GivenUnlinkedPermissionAndApplication_ThrowsItemNotProcessableException()
        {
            // Arrange
            var functionRepository = Substitute.For<IFunctionRepository>();
            var permissionRepository = Substitute.For<IPermissionRepository>();
            var applicationRepository = Substitute.For<IApplicationRepository>();

            // Change ApplicationId to break link between the permission and Application
            mockedFunctionModel.FunctionPermissions[0].Permission.ApplicationFunctionPermissions[0].ApplicationFunction.Application.Id = Guid.NewGuid();

            applicationRepository.GetByIdAsync(Arg.Any<Guid>())
                .Returns(mockedFunctionModel.Application);
            permissionRepository.GetByIdWithApplicationAsync(mockedFunctionModel.FunctionPermissions[0].PermissionId)
                .Returns(mockedFunctionModel.FunctionPermissions[0].Permission);
            functionRepository.CreateAsync(Arg.Any<FunctionModel>()).Returns(mockedFunctionModel);

            var functionService = new FunctionService(functionRepository, permissionRepository, applicationRepository, mapper);

            // Act
            Exception caughEx = null;
            try
            {
                var functionResource = await functionService.CreateAsync(mockedFunctionSubmitModel, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughEx = ex;
            }

            // Assert
            Assert.True(caughEx is ItemNotProcessableException, "Unlinked permissions and applications must throw an ItemNotProcessableException.");
        }

        [Fact]
        public async Task CreateAsync_GivenFullProcessableModel_ReturnsCreatedModel()
        {
            // Arrange
            var functionRepository = Substitute.For<IFunctionRepository>();
            var permissionRepository = Substitute.For<IPermissionRepository>();
            var applicationRepository = Substitute.For<IApplicationRepository>();

            applicationRepository.GetByIdAsync(mockedFunctionModel.Application.Id)
                .Returns(mockedFunctionModel.Application);
            permissionRepository.GetByIdWithApplicationAsync(mockedFunctionModel.FunctionPermissions[0].PermissionId)
                .Returns(mockedFunctionModel.FunctionPermissions[0].Permission);
            functionRepository.CreateAsync(Arg.Any<FunctionModel>()).Returns(mockedFunctionModel);

            var functionService = new FunctionService(functionRepository, permissionRepository, applicationRepository, mapper);

            // Act
            var functionResource = await functionService.CreateAsync(mockedFunctionSubmitModel, Guid.NewGuid());

            // Assert
            Assert.True(functionResource.Name == mockedFunctionSubmitModel.Name, $"Function Resource name: '{functionResource.Name}' not the expected value: '{mockedFunctionSubmitModel.Name}'");
            Assert.True(functionResource.ApplicationId == mockedFunctionSubmitModel.ApplicationId, $"Function Resource name: '{functionResource.ApplicationId}' not the expected value: '{mockedFunctionSubmitModel.ApplicationId}'");
            Assert.True(functionResource.Permissions.Count == mockedFunctionSubmitModel.Permissions.Count, $"Function Resource Permission Count: '{functionResource.Permissions.Count}' not the expected value: '{mockedFunctionSubmitModel.Permissions.Count}'");
        }

        [Fact]
        public async Task CreateAsync_GivenAlreadyUsedName_ThrowsItemNotProcessableException()
        {
            var functionRepository = Substitute.For<IFunctionRepository>();
            var permissionRepository = Substitute.For<IPermissionRepository>();
            var applicationRepository = Substitute.For<IApplicationRepository>();

            applicationRepository.GetByIdAsync(mockedFunctionModel.Application.Id)
                .Returns(mockedFunctionModel.Application);
            permissionRepository.GetByIdWithApplicationAsync(mockedFunctionModel.FunctionPermissions[0].PermissionId)
                .Returns(mockedFunctionModel.FunctionPermissions[0].Permission);
            functionRepository.GetByIdAsync(mockedFunctionModel.Id).Returns(mockedFunctionModel);
            functionRepository.GetByNameAsync(mockedFunctionSubmitModel.Name).Returns(mockedFunctionModel);
            functionRepository.CreateAsync(Arg.Any<FunctionModel>()).Returns(mockedFunctionModel);

            var functionService = new FunctionService(functionRepository, permissionRepository, applicationRepository, mapper);

            // Act
            Exception caughEx = null;
            try
            {
                var functionResource = await functionService.CreateAsync(mockedFunctionSubmitModel, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughEx = ex;
            }

            // Assert
            Assert.True(caughEx is ItemNotProcessableException, "Attempted create with an already used name must throw an ItemNotProcessableException.");
        }

        [Fact]
        public async Task GetListAsync_Executed_ReturnsList()
        {
            // Arrange
            var functionRepository = Substitute.For<IFunctionRepository>();
            var permissionRepository = Substitute.For<IPermissionRepository>();
            var applicationRepository = Substitute.For<IApplicationRepository>();

            functionRepository.GetListAsync().Returns(
                new List<FunctionModel>()
                {
                    mockedFunctionModel,
                    mockedFunctionModel
                });

            var functionService = new FunctionService(functionRepository, permissionRepository, applicationRepository, mapper);

            // Act
            var functionList = await functionService.GetListAsync();

            // Assert
            Assert.True(functionList.Count == 2, "Expected list count is 2");
            Assert.True(functionList[0].Name == mockedFunctionModel.Name, $"Expected applicationFunction name: '{functionList[0].Name}' does not equal expected value: '{mockedFunctionModel.Name}'");
            Assert.True(functionList[0].Uuid == mockedFunctionModel.Id, $"Expected applicationFunction UUID: '{functionList[0].Uuid}' does not equal expected value: '{mockedFunctionModel.Id}'");
        }

        [Fact]
        public async Task UpdateAsync_GivenFullProcessableModel_ReturnsUpdatedModel()
        {
            // Arrange
            var functionRepository = Substitute.For<IFunctionRepository>();
            var permissionRepository = Substitute.For<IPermissionRepository>();
            var applicationRepository = Substitute.For<IApplicationRepository>();

            applicationRepository.GetByIdAsync(mockedFunctionModel.Application.Id)
                .Returns(mockedFunctionModel.Application);
            permissionRepository.GetByIdWithApplicationAsync(mockedFunctionModel.FunctionPermissions[0].PermissionId)
                .Returns(mockedFunctionModel.FunctionPermissions[0].Permission);
            functionRepository.GetByIdAsync(mockedFunctionModel.Id).Returns(mockedFunctionModel);
            functionRepository.UpdateAsync(Arg.Any<FunctionModel>()).Returns(mockedFunctionModel);

            var functionService = new FunctionService(functionRepository, permissionRepository, applicationRepository, mapper);

            // Act
            var functionResource = await functionService.UpdateAsync(mockedFunctionSubmitModel, Guid.NewGuid());

            // Assert
            Assert.True(functionResource.Name == mockedFunctionSubmitModel.Name, $"Function Resource name: '{functionResource.Name}' not the expected value: '{mockedFunctionSubmitModel.Name}'");
            Assert.True(functionResource.ApplicationId == mockedFunctionSubmitModel.ApplicationId, $"Function Resource name: '{functionResource.ApplicationId}' not the expected value: '{mockedFunctionSubmitModel.ApplicationId}'");
            Assert.True(functionResource.Permissions.Count == mockedFunctionSubmitModel.Permissions.Count, $"Function Resource Permission Count: '{functionResource.Permissions.Count}' not the expected value: '{mockedFunctionSubmitModel.Permissions.Count}'");
        }

        [Fact]
        public async Task UpdateAsync_GivenUnfindableFunction_ThrowsItemNotFoundException()
        {
            // Arrange
            var functionRepository = Substitute.For<IFunctionRepository>();
            var permissionRepository = Substitute.For<IPermissionRepository>();
            var applicationRepository = Substitute.For<IApplicationRepository>();

            applicationRepository.GetByIdAsync(mockedFunctionModel.Application.Id)
                .Returns(mockedFunctionModel.Application);
            permissionRepository.GetByIdWithApplicationAsync(mockedFunctionModel.FunctionPermissions[0].PermissionId)
                .Returns(mockedFunctionModel.FunctionPermissions[0].Permission);
            functionRepository.UpdateAsync(Arg.Any<FunctionModel>()).Returns(mockedFunctionModel);

            var functionService = new FunctionService(functionRepository, permissionRepository, applicationRepository, mapper);

            // Act
            Exception caughEx = null;
            try
            {
                var functionResource = await functionService.UpdateAsync(mockedFunctionSubmitModel, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughEx = ex;
            }

            // Assert
            Assert.True(caughEx is ItemNotFoundException, "Unfindable functions must throw an ItemNotFoundException");
        }

        [Fact]
        public async Task UpdateAsync_GivenNewTakenName_ThrowsItemNotProcessableException()
        {
            // Arrange
            var functionRepository = Substitute.For<IFunctionRepository>();
            var permissionRepository = Substitute.For<IPermissionRepository>();
            var applicationRepository = Substitute.For<IApplicationRepository>();

            mockedFunctionSubmitModel.Name += "_changed_name";

            applicationRepository.GetByIdAsync(mockedFunctionModel.Application.Id)
                .Returns(mockedFunctionModel.Application);
            permissionRepository.GetByIdWithApplicationAsync(mockedFunctionModel.FunctionPermissions[0].PermissionId)
                .Returns(mockedFunctionModel.FunctionPermissions[0].Permission);
            functionRepository.GetByIdAsync(mockedFunctionModel.Id).Returns(mockedFunctionModel);
            functionRepository.GetByNameAsync(mockedFunctionSubmitModel.Name).Returns(mockedFunctionModel);
            functionRepository.UpdateAsync(Arg.Any<FunctionModel>()).Returns(mockedFunctionModel);

            var functionService = new FunctionService(functionRepository, permissionRepository, applicationRepository, mapper);

            // Act
            Exception caughEx = null;
            try
            {
                var functionResource = await functionService.UpdateAsync(mockedFunctionSubmitModel, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughEx = ex;
            }

            // Assert
            Assert.True(caughEx is ItemNotProcessableException, "New taken name must throw an ItemNotProcessableException");
        }

        [Fact]
        public async Task UpdateAsync_GivenNewTUntakenName_ReturnsUpdatedFunction()
        {
            // Arrange
            var functionRepository = Substitute.For<IFunctionRepository>();
            var permissionRepository = Substitute.For<IPermissionRepository>();
            var applicationRepository = Substitute.For<IApplicationRepository>();

            mockedFunctionSubmitModel.Name += "_changed_name";

            applicationRepository.GetByIdAsync(mockedFunctionModel.Application.Id)
                .Returns(mockedFunctionModel.Application);
            permissionRepository.GetByIdWithApplicationAsync(mockedFunctionModel.FunctionPermissions[0].PermissionId)
                .Returns(mockedFunctionModel.FunctionPermissions[0].Permission);
            functionRepository.GetByIdAsync(mockedFunctionModel.Id).Returns(mockedFunctionModel);
            functionRepository.UpdateAsync(Arg.Any<FunctionModel>()).Returns(mockedFunctionModel);

            var functionService = new FunctionService(functionRepository, permissionRepository, applicationRepository, mapper);

            // Act
            var functionResource = await functionService.UpdateAsync(mockedFunctionSubmitModel, Guid.NewGuid());

            // Assert
            Assert.True(functionResource.Name == mockedFunctionSubmitModel.Name, $"Function Resource name: '{functionResource.Name}' not the expected value: '{mockedFunctionSubmitModel.Name}'");
            Assert.True(functionResource.ApplicationId == mockedFunctionSubmitModel.ApplicationId, $"Function Resource name: '{functionResource.ApplicationId}' not the expected value: '{mockedFunctionSubmitModel.ApplicationId}'");
            Assert.True(functionResource.Permissions.Count == mockedFunctionSubmitModel.Permissions.Count, $"Function Resource Permission Count: '{functionResource.Permissions.Count}' not the expected value: '{mockedFunctionSubmitModel.Permissions.Count}'");
        }

        [Fact]
        public async Task DeleteAsync_GivenFindableGuid_ExecutesSuccessfully()
        {
            // Arrange
            var functionRepository = Substitute.For<IFunctionRepository>();
            var permissionRepository = Substitute.For<IPermissionRepository>();
            var applicationRepository = Substitute.For<IApplicationRepository>();

            functionRepository.GetByIdAsync(mockedFunctionModel.Id).Returns(mockedFunctionModel);

            var functionService = new FunctionService(functionRepository, permissionRepository, applicationRepository, mapper);

            // Act
            Exception caughEx = null;
            try
            {
                await functionService.DeleteAsync(mockedFunctionModel.Id);
            }
            catch (Exception ex)
            {
                caughEx = ex;
            }

            // Assert
            Assert.True(caughEx is null, "Delete on a findable GUID must execute successfully.");
        }

        [Fact]
        public async Task DeleteAsync_GivenUnfindableGuid_ThrowsItemNotFoundException()
        {
            // Arrange
            var functionRepository = Substitute.For<IFunctionRepository>();
            var permissionRepository = Substitute.For<IPermissionRepository>();
            var applicationRepository = Substitute.For<IApplicationRepository>();

            var functionService = new FunctionService(functionRepository, permissionRepository, applicationRepository, mapper);

            // Act
            Exception caughEx = null;
            try
            {
                await functionService.DeleteAsync(mockedFunctionModel.Id);
            }
            catch (Exception ex)
            {
                caughEx = ex;
            }

            // Assert
            Assert.True(caughEx is ItemNotFoundException, "Delete on a findable GUID must throw an ItemNotFoundException.");
        }
    }
}
