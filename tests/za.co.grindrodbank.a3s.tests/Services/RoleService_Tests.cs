/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
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
using static za.co.grindrodbank.a3s.Models.TransientStateMachineRecord;

namespace za.co.grindrodbank.a3s.tests.Services
{
    public class RoleService_Tests
    {
        private readonly IMapper mapper;
        private readonly RoleModel mockedRoleModel;
        private readonly RoleSubmit mockedRoleSubmitModel;
        private readonly Guid roleGuid;
        private readonly Guid functionGuid;
        private readonly Guid childRoleGuid;

        private readonly IRoleTransientRepository roleTransientRepository;
        private readonly IRoleFunctionTransientRepository roleFunctionTransientRepository;
        private readonly IRoleRoleTransientRepository roleRoleTransientRepository;
        private readonly IRoleRepository roleRepository;
        private readonly IUserRepository userRepository;
        private readonly IFunctionRepository functionRepository;
        private readonly ISubRealmRepository subRealmRepository;

        public RoleService_Tests()
        {
            roleRepository = Substitute.For<IRoleRepository>();
            userRepository = Substitute.For<IUserRepository>();
            functionRepository = Substitute.For<IFunctionRepository>();
            subRealmRepository = Substitute.For<ISubRealmRepository>();
            roleTransientRepository = Substitute.For<IRoleTransientRepository>();
            roleFunctionTransientRepository = Substitute.For<IRoleFunctionTransientRepository>();
            roleRoleTransientRepository = Substitute.For<IRoleRoleTransientRepository>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new RoleSubmitResourceRoleModelProfile());
                cfg.AddProfile(new RoleTransientResourceRoleTransientModelProfile());
                cfg.AddProfile(new RoleResourceRoleModelProfile());
                cfg.AddProfile(new RoleFunctionTransientResourceRoleFunctionTransientModelProfile());
                cfg.AddProfile(new RoleRoleTransientResourceRoleRoleTransientModelProfile());
            });

            mapper = config.CreateMapper();
            roleGuid = Guid.NewGuid();
            functionGuid = Guid.NewGuid();
            childRoleGuid = Guid.NewGuid();

            mockedRoleModel = new RoleModel
            {
                Name = "Test Role",
                Id = roleGuid
            };

            mockedRoleModel.RoleFunctions = new List<RoleFunctionModel>
            {
                new RoleFunctionModel
                {
                    Role = mockedRoleModel,
                    Function = new FunctionModel
                    {
                        Id = functionGuid,
                        Name = "Test function model",
                        Description = "Test function description model"
                    }
                }
            };

            mockedRoleModel.ChildRoles = new List<RoleRoleModel>()
            {
                new RoleRoleModel()
                {
                    ChildRoleId = childRoleGuid,
                    ParentRoleId = roleGuid,
                    ChildRole = new RoleModel()
                    {
                        Id = childRoleGuid
                    },
                    ParentRole = mockedRoleModel
                }
            };

            mockedRoleSubmitModel = new RoleSubmit()
            {
                Uuid = mockedRoleModel.Id,
                Name = mockedRoleModel.Name,
                FunctionIds = new List<Guid>(),
                RoleIds = new List<Guid>()
            };

            foreach (var function in mockedRoleModel.RoleFunctions)
                mockedRoleSubmitModel.FunctionIds.Add(function.FunctionId);

            foreach (var childRole in mockedRoleModel.ChildRoles)
                mockedRoleSubmitModel.RoleIds.Add(childRole.ChildRoleId);
        }

        [Fact]
        public async Task GetById_GivenGuid_ReturnsRoleResource()
        {
            roleRepository.GetByIdAsync(roleGuid).Returns(mockedRoleModel);

            var roleService = new RoleService(roleRepository, userRepository, functionRepository, subRealmRepository, roleTransientRepository, roleFunctionTransientRepository, roleRoleTransientRepository, mapper);
            var roleResource = await roleService.GetByIdAsync(roleGuid);

            Assert.NotNull(roleResource);
            Assert.True(roleResource.Name == "Test Role", $"Role resource Name: '{roleResource.Name}' does not match expected value: 'Test Role'");
            Assert.True(roleResource.Uuid == roleGuid, $"Role resource UUID: '{roleResource.Uuid}' does not match expected value: '{roleGuid}'");
        }

        [Fact]
        public async Task CreateAsync_GivenFullProcessableModel_ReturnsCreatedTransientModel()
        {
            // Arrange
            functionRepository.GetByIdAsync(mockedRoleModel.RoleFunctions[0].FunctionId)
                .Returns(mockedRoleModel.RoleFunctions[0].Function);
            roleRepository.CreateAsync(Arg.Any<RoleModel>()).Returns(mockedRoleModel);
            roleRepository.GetByIdAsync(Arg.Any<Guid>())
                .Returns(new RoleModel()
                {
                    Id = Guid.NewGuid(),
                    ChildRoles = new List<RoleRoleModel>()
                });

            var changeByGuid = Guid.NewGuid();

            roleTransientRepository.CreateAsync(Arg.Any<RoleTransientModel>()).Returns(new RoleTransientModel
            {
                Action = TransientAction.Create,
                ChangedBy = changeByGuid,
                ApprovalCount = 0,
                // Pending is the initial state of the state machine for all transient records.
                R_State = DatabaseRecordState.Captured,
                Name = mockedRoleModel.Name,
                Description = mockedRoleModel.Description,
                SubRealmId = Guid.Empty,
                RoleId = mockedRoleModel.Id
            });

            roleFunctionTransientRepository.GetAllTransientFunctionRelationsForRoleAsync(mockedRoleModel.Id).Returns(new List<RoleFunctionTransientModel>());
            roleFunctionTransientRepository.GetTransientFunctionRelationsForRoleAsync(mockedRoleModel.Id, Arg.Any<Guid>()).Returns(new List<RoleFunctionTransientModel>());
            roleRoleTransientRepository.GetAllTransientChildRoleRelationsForRoleAsync(mockedRoleModel.Id).Returns(new List<RoleRoleTransientModel>());
            roleRoleTransientRepository.GetTransientChildRoleRelationsForRoleAsync(mockedRoleModel.Id, Arg.Any<Guid>()).Returns(new List<RoleRoleTransientModel>());

            var roleService = new RoleService(roleRepository, userRepository, functionRepository, subRealmRepository, roleTransientRepository, roleFunctionTransientRepository, roleRoleTransientRepository, mapper);

            // Act
            var roleTransientResource = await roleService.CreateAsync(mockedRoleSubmitModel, Guid.NewGuid());

            // Assert
            Assert.NotNull(roleTransientResource);
            Assert.True(roleTransientResource is RoleTransient, $"Expecting returned type to be 'RoleTransient', but actual type is {roleTransientResource}");
            Assert.True(roleTransientResource.Name == mockedRoleSubmitModel.Name, $"Role Resource name: '{roleTransientResource.Name}' not the expected value: '{mockedRoleSubmitModel.Name}'");
        }

        [Fact]
        public async Task CreateAsync_GivenUnfindableFunction_ThrowsItemNotFoundException()
        {
            // Arrange
            //functionRepository.When(x => x.GetByIdAsync(Arg.Any<Guid>())).Do(x => throw new ItemNotFoundException());
            //functionRepository.GetByIdAsync(Arg.Any<Guid>()).Returns();

            roleRepository.CreateAsync(Arg.Any<RoleModel>()).Returns(mockedRoleModel);
            // Add a random new Guid as a function to the role submit.
            mockedRoleSubmitModel.FunctionIds.Add(Guid.NewGuid());

            var changedByGuid = Guid.NewGuid();

            roleTransientRepository.CreateAsync(Arg.Any<RoleTransientModel>()).Returns(new RoleTransientModel
            {
                Action = TransientAction.Create,
                ChangedBy = changedByGuid,
                ApprovalCount = 0,
                // Pending is the initial state of the state machine for all transient records.
                R_State = DatabaseRecordState.Captured,
                Name = mockedRoleModel.Name,
                Description = mockedRoleModel.Description,
                SubRealmId = Guid.Empty,
                RoleId = mockedRoleModel.Id
            });

            roleFunctionTransientRepository.GetAllTransientFunctionRelationsForRoleAsync(mockedRoleModel.Id).Returns(new List<RoleFunctionTransientModel>());
            roleFunctionTransientRepository.GetTransientFunctionRelationsForRoleAsync(mockedRoleModel.Id, Arg.Any<Guid>()).Returns(new List<RoleFunctionTransientModel>());
            roleRoleTransientRepository.GetAllTransientChildRoleRelationsForRoleAsync(mockedRoleModel.Id).Returns(new List<RoleRoleTransientModel>());
            roleRepository.GetByIdAsync(mockedRoleModel.ChildRoles[0].ChildRoleId).Returns((RoleModel)null);
            functionRepository.GetByIdAsync(mockedRoleModel.RoleFunctions[0].FunctionId).Returns((FunctionModel)null);

            var roleService = new RoleService(roleRepository, userRepository, functionRepository, subRealmRepository, roleTransientRepository, roleFunctionTransientRepository, roleRoleTransientRepository, mapper);

            // Act
            Exception caughtException = null;

            try
            {
                var roleResource = await roleService.CreateAsync(mockedRoleSubmitModel, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.True(caughtException is ItemNotFoundException, $"Unfindable functions must throw and ItemNotFoundException.");
            Assert.Contains($"Function with ID '{mockedRoleModel.RoleFunctions[0].FunctionId}' not found when attempting to assign it to a role.", caughtException.Message);
        }


        [Fact]
        public async Task CreateAsync_GivenAlreadyUsedName_ThrowsItemNotProcessableException()
        {
            functionRepository.GetByIdAsync(mockedRoleModel.RoleFunctions[0].FunctionId)
                .Returns(mockedRoleModel.RoleFunctions[0].Function);
            roleRepository.GetByIdAsync(mockedRoleModel.Id).Returns(mockedRoleModel);
            roleRepository.GetByNameAsync(mockedRoleSubmitModel.Name).Returns(mockedRoleModel);
            roleRepository.CreateAsync(Arg.Any<RoleModel>()).Returns(mockedRoleModel);


            var roleService = new RoleService(roleRepository, userRepository, functionRepository, subRealmRepository, roleTransientRepository, roleFunctionTransientRepository, roleRoleTransientRepository, mapper);

            // Act
            Exception caughEx = null;
            try
            {
                var roleResource = await roleService.CreateAsync(mockedRoleSubmitModel, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughEx = ex;
            }

            // Assert
            Assert.True(caughEx is ItemNotProcessableException, "Attempted create with an already used name must throw an ItemNotProcessableException.");
            Assert.Contains($"Role with Name '{mockedRoleSubmitModel.Name}' already exist.", caughEx.Message);
        }

        [Fact]
        public async Task CreateAsync_GivenUnfindableChildRole_ThrowsItemNotFoundException()
        {
            // Arrange
            functionRepository.GetByIdAsync(mockedRoleModel.RoleFunctions[0].FunctionId)
                .Returns(mockedRoleModel.RoleFunctions[0].Function);
            roleRepository.CreateAsync(Arg.Any<RoleModel>()).Returns(mockedRoleModel);
            var changedByGuid = Guid.NewGuid();

            roleTransientRepository.CreateAsync(Arg.Any<RoleTransientModel>()).Returns(new RoleTransientModel
            {
                Action = TransientAction.Create,
                ChangedBy = changedByGuid,
                ApprovalCount = 0,
                // Pending is the initial state of the state machine for all transient records.
                R_State = DatabaseRecordState.Captured,
                Name = mockedRoleModel.Name,
                Description = mockedRoleModel.Description,
                SubRealmId = Guid.Empty,
                RoleId = mockedRoleModel.Id
            });

            roleFunctionTransientRepository.GetAllTransientFunctionRelationsForRoleAsync(mockedRoleModel.Id).Returns(new List<RoleFunctionTransientModel>());
            roleFunctionTransientRepository.GetTransientFunctionRelationsForRoleAsync(mockedRoleModel.Id, Arg.Any<Guid>()).Returns(new List<RoleFunctionTransientModel>());
            roleRoleTransientRepository.GetAllTransientChildRoleRelationsForRoleAsync(mockedRoleModel.Id).Returns(new List<RoleRoleTransientModel>());
            roleRepository.GetByIdAsync(mockedRoleModel.ChildRoles[0].ChildRoleId).Returns((RoleModel)null);

            var roleService = new RoleService(roleRepository, userRepository, functionRepository, subRealmRepository, roleTransientRepository, roleFunctionTransientRepository, roleRoleTransientRepository, mapper);

            // Act
            Exception caughtException = null;

            try
            {
                var roleResource = await roleService.CreateAsync(mockedRoleSubmitModel, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.True(caughtException is ItemNotFoundException, $"Unfindable child roles must throw and ItemNotFoundException.");
            Assert.Contains("not found when attempting to assign it as a child role of role with ID", caughtException.Message);
        }

        [Fact]
        public async Task CreateAsync_GivenCompoundChildRole_ThrowsItemNotProcessableException()
        {
            // Arrange
            functionRepository.GetByIdAsync(mockedRoleModel.RoleFunctions[0].FunctionId)
                .Returns(mockedRoleModel.RoleFunctions[0].Function);
            roleRepository.CreateAsync(Arg.Any<RoleModel>()).Returns(mockedRoleModel);
            roleRepository.GetByIdAsync(mockedRoleModel.ChildRoles[0].ChildRoleId)
                .Returns(new RoleModel()
                {
                    Id = Guid.NewGuid(),
                    ChildRoles = new List<RoleRoleModel>()
                    {
                        new RoleRoleModel(),
                        new RoleRoleModel()
                    }
                });

            var changeByGuid = Guid.NewGuid();

            roleTransientRepository.CreateAsync(Arg.Any<RoleTransientModel>()).Returns(new RoleTransientModel
            {
                Action = TransientAction.Create,
                ChangedBy = changeByGuid,
                ApprovalCount = 0,
                // Pending is the initial state of the state machine for all transient records.
                R_State = DatabaseRecordState.Captured,
                Name = mockedRoleModel.Name,
                Description = mockedRoleModel.Description,
                SubRealmId = Guid.Empty,
                RoleId = mockedRoleModel.Id
            });

            roleFunctionTransientRepository.GetAllTransientFunctionRelationsForRoleAsync(mockedRoleModel.Id).Returns(new List<RoleFunctionTransientModel>());
            roleFunctionTransientRepository.GetTransientFunctionRelationsForRoleAsync(mockedRoleModel.Id, Arg.Any<Guid>()).Returns(new List<RoleFunctionTransientModel>());
            roleRoleTransientRepository.GetAllTransientChildRoleRelationsForRoleAsync(mockedRoleModel.Id).Returns(new List<RoleRoleTransientModel>());

            var roleService = new RoleService(roleRepository, userRepository, functionRepository, subRealmRepository, roleTransientRepository, roleFunctionTransientRepository, roleRoleTransientRepository, mapper);

            // Act
            Exception caughtException = null;

            try
            {
                var roleResource = await roleService.CreateAsync(mockedRoleSubmitModel, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.True(caughtException is ItemNotProcessableException, $"Compound child roles must throw and ItemNotProcessableException. Actual Exception: '{caughtException.ToString()}'");
            Assert.True(caughtException.Message.Contains("as the child role already has it's own child roles assigned"), $"Compound child roles must throw and ItemNotProcessableException. Actual Exception: '{caughtException.ToString()}'");
        }

        [Fact]
        public async Task GetListAsync_Executed_ReturnsList()
        {
            // Arrange
            roleRepository.GetListAsync().Returns(
                new List<RoleModel>()
                {
                    mockedRoleModel,
                    mockedRoleModel
                });

            var roleService = new RoleService(roleRepository, userRepository, functionRepository, subRealmRepository, roleTransientRepository, roleFunctionTransientRepository, roleRoleTransientRepository, mapper);

            // Act
            var roleList = await roleService.GetListAsync();

            // Assert
            Assert.True(roleList.Count == 2, "Expected list count is 2");
            Assert.True(roleList[0].Name == mockedRoleModel.Name, $"Expected applicationRole name: '{roleList[0].Name}' does not equal expected value: '{mockedRoleModel.Name}'");
            Assert.True(roleList[0].Uuid == mockedRoleModel.Id, $"Expected applicationRole UUID: '{roleList[0].Uuid}' does not equal expected value: '{mockedRoleModel.Id}'");
        }

        [Fact]
        public async Task UpdateAsync_GivenFullProcessableModel_ReturnsUpdatedModel()
        {
            // Arrange
            functionRepository.GetByIdAsync(mockedRoleModel.RoleFunctions[0].FunctionId)
                .Returns(mockedRoleModel.RoleFunctions[0].Function);
            roleRepository.GetByIdAsync(mockedRoleSubmitModel.Uuid).Returns(mockedRoleModel);
            roleRepository.UpdateAsync(Arg.Any<RoleModel>()).Returns(mockedRoleModel);
            roleRepository.GetByIdAsync(mockedRoleModel.ChildRoles[0].ChildRoleId)
                .Returns(new RoleModel()
                {
                    Id = Guid.NewGuid(),
                    ChildRoles = new List<RoleRoleModel>()
                });

            var changedByGuid = Guid.NewGuid();

            roleTransientRepository.CreateAsync(Arg.Any<RoleTransientModel>()).Returns(new RoleTransientModel
            {
                Action = TransientAction.Create,
                ChangedBy = changedByGuid,
                ApprovalCount = 0,
                // Pending is the initial state of the state machine for all transient records.
                R_State = DatabaseRecordState.Captured,
                Name = mockedRoleModel.Name,
                Description = mockedRoleModel.Description,
                SubRealmId = Guid.Empty,
                RoleId = mockedRoleModel.Id
            });

            roleFunctionTransientRepository.GetAllTransientFunctionRelationsForRoleAsync(mockedRoleModel.Id).Returns(new List<RoleFunctionTransientModel>());
            roleFunctionTransientRepository.GetTransientFunctionRelationsForRoleAsync(mockedRoleModel.Id, Arg.Any<Guid>()).Returns(new List<RoleFunctionTransientModel>());
            roleRoleTransientRepository.GetAllTransientChildRoleRelationsForRoleAsync(mockedRoleModel.Id).Returns(new List<RoleRoleTransientModel>());
            functionRepository.GetByIdAsync(mockedRoleSubmitModel.FunctionIds[0]).Returns(new FunctionModel
            {
                Id = functionGuid,
                Name = "Test function model",
                Description = "Test function description model"
            });

            roleTransientRepository.GetTransientsForRoleAsync(mockedRoleModel.Id).Returns(new List<RoleTransientModel> { new RoleTransientModel
            {
                Action = TransientAction.Modify,
                ChangedBy = changedByGuid,
                ApprovalCount = 0,
                // Pending is the initial state of the state machine for all transient records.
                R_State = DatabaseRecordState.Pending,
                Name = mockedRoleModel.Name,
                Description = mockedRoleModel.Description,
                SubRealmId = Guid.Empty,
                RoleId = mockedRoleModel.Id
            } });

            var roleService = new RoleService(roleRepository, userRepository, functionRepository, subRealmRepository, roleTransientRepository, roleFunctionTransientRepository, roleRoleTransientRepository, mapper);
            var updaterGuid = Guid.NewGuid();
            // Act
            var roleTransientResource = await roleService.UpdateAsync(mockedRoleSubmitModel, mockedRoleSubmitModel.Uuid, updaterGuid);

            // Assert
            Assert.NotNull(roleTransientResource);
            Assert.True(roleTransientResource.Name == mockedRoleSubmitModel.Name, $"Role Resource name: '{roleTransientResource.Name}' not the expected value: '{mockedRoleSubmitModel.Name}'");
            Assert.True(roleTransientResource is RoleTransient);
            Assert.True(roleTransientResource.Action == "Modify");
        }

        [Fact]
        public async Task UpdateAsync_GivenUnfindableRole_ThrowsItemNotFoundException()
        {
            // Arrange
            functionRepository.GetByIdAsync(mockedRoleModel.RoleFunctions[0].FunctionId)
                .Returns(mockedRoleModel.RoleFunctions[0].Function);
            roleRepository.UpdateAsync(Arg.Any<RoleModel>()).Returns(mockedRoleModel);

            var roleService = new RoleService(roleRepository, userRepository, functionRepository, subRealmRepository, roleTransientRepository, roleFunctionTransientRepository, roleRoleTransientRepository, mapper);
            var updaterGuid = Guid.NewGuid();

            // Act
            Exception caughEx = null;
            try
            {
                var roleResource = await roleService.UpdateAsync(mockedRoleSubmitModel, Guid.NewGuid(), updaterGuid);
            }
            catch (Exception ex)
            {
                caughEx = ex;
            }

            // Assert
            Assert.True(caughEx is ItemNotFoundException, "Unfindable roles must throw an ItemNotFoundException");
        }

        [Fact]
        public async Task UpdateAsync_GivenUnfindableFunction_ThrowsItemNotFoundException()
        {
            // Arrange
            functionRepository.When(x => x.GetByIdAsync(Arg.Any<Guid>())).Do(x => throw new ItemNotFoundException());
            roleRepository.UpdateAsync(Arg.Any<RoleModel>()).Returns(mockedRoleModel);
            roleRepository.GetByIdAsync(mockedRoleModel.ChildRoles[0].ChildRoleId)
                .Returns(new RoleModel()
                {
                    Id = Guid.NewGuid(),
                    ChildRoles = new List<RoleRoleModel>()
                });

            var roleService = new RoleService(roleRepository, userRepository, functionRepository, subRealmRepository, roleTransientRepository, roleFunctionTransientRepository, roleRoleTransientRepository, mapper);
            var updaterGuid = Guid.NewGuid();

            // Act
            Exception caughEx = null;
            try
            {
                var roleResource = await roleService.UpdateAsync(mockedRoleSubmitModel, Guid.NewGuid(), updaterGuid);
            }
            catch (Exception ex)
            {
                caughEx = ex;
            }

            // Assert
            Assert.True(caughEx is ItemNotFoundException, "Unfindable functions must throw an ItemNotFoundException");
        }

        [Fact]
        public async Task UpdateAsync_GivenUnfindableChildRole_ThrowsItemNotFoundException()
        {
            // Arrange
            functionRepository.GetByIdAsync(mockedRoleModel.RoleFunctions[0].FunctionId)
                .Returns(mockedRoleModel.RoleFunctions[0].Function);
            roleRepository.GetByIdAsync(mockedRoleModel.Id).Returns(mockedRoleModel);
            roleRepository.UpdateAsync(Arg.Any<RoleModel>()).Returns(mockedRoleModel);

            var roleService = new RoleService(roleRepository, userRepository, functionRepository, subRealmRepository, roleTransientRepository, roleFunctionTransientRepository, roleRoleTransientRepository, mapper);
            var updaterGuid = Guid.NewGuid();

            // Act
            Exception caughEx = null;
            try
            {
                var roleResource = await roleService.UpdateAsync(mockedRoleSubmitModel, Guid.NewGuid(), updaterGuid);
            }
            catch (Exception ex)
            {
                caughEx = ex;
            }

            // Assert
            Assert.True(caughEx is ItemNotFoundException, "Unfindable child roles must throw an ItemNotFoundException");
        }

        [Fact]
        public async Task UpdateAsync_GivenCompoundChildRole_ThrowsItemNotProcessable()
        {
            // Arrange
            functionRepository.GetByIdAsync(mockedRoleModel.RoleFunctions[0].FunctionId)
                .Returns(mockedRoleModel.RoleFunctions[0].Function);
            roleRepository.GetByIdAsync(mockedRoleSubmitModel.Uuid).Returns(mockedRoleModel);
            roleRepository.UpdateAsync(Arg.Any<RoleModel>()).Returns(mockedRoleModel);
            // Create a 'new' child role submit.
            mockedRoleSubmitModel.RoleIds[0] = Guid.NewGuid();

            roleRepository.GetByIdAsync(mockedRoleSubmitModel.RoleIds[0])
                .Returns(new RoleModel()
                {
                    Id = Guid.NewGuid(),
                    ChildRoles = new List<RoleRoleModel>()
                    {
                        new RoleRoleModel(),
                        new RoleRoleModel()
                    }
                });

            var changedByGuid = Guid.NewGuid();

            roleTransientRepository.CreateAsync(Arg.Any<RoleTransientModel>()).Returns(new RoleTransientModel
            {
                Action = TransientAction.Create,
                ChangedBy = changedByGuid,
                ApprovalCount = 0,
                // Pending is the initial state of the state machine for all transient records.
                R_State = DatabaseRecordState.Captured,
                Name = mockedRoleModel.Name,
                Description = mockedRoleModel.Description,
                SubRealmId = Guid.Empty,
                RoleId = mockedRoleModel.Id
            });

            roleFunctionTransientRepository.GetAllTransientFunctionRelationsForRoleAsync(mockedRoleModel.Id).Returns(new List<RoleFunctionTransientModel>());
            roleFunctionTransientRepository.GetTransientFunctionRelationsForRoleAsync(mockedRoleModel.Id, Arg.Any<Guid>()).Returns(new List<RoleFunctionTransientModel>());
            roleRoleTransientRepository.GetAllTransientChildRoleRelationsForRoleAsync(mockedRoleModel.Id).Returns(new List<RoleRoleTransientModel>());
            functionRepository.GetByIdAsync(mockedRoleSubmitModel.FunctionIds[0]).Returns(new FunctionModel
            {
                Id = functionGuid,
                Name = "Test function model",
                Description = "Test function description model"
            });

            roleTransientRepository.GetTransientsForRoleAsync(mockedRoleModel.Id).Returns(new List<RoleTransientModel> { new RoleTransientModel
            {
                Action = TransientAction.Modify,
                ChangedBy = changedByGuid,
                ApprovalCount = 0,
                // Pending is the initial state of the state machine for all transient records.
                R_State = DatabaseRecordState.Pending,
                Name = mockedRoleModel.Name,
                Description = mockedRoleModel.Description,
                SubRealmId = Guid.Empty,
                RoleId = mockedRoleModel.Id
            } });

            var roleService = new RoleService(roleRepository, userRepository, functionRepository, subRealmRepository, roleTransientRepository, roleFunctionTransientRepository, roleRoleTransientRepository, mapper);
            var updaterGuid = Guid.NewGuid();

            // Act
            Exception caughEx = null;
            try
            {
                var roleResource = await roleService.UpdateAsync(mockedRoleSubmitModel, mockedRoleSubmitModel.Uuid, updaterGuid);
            }
            catch (Exception ex)
            {
                caughEx = ex;
            }

            // Assert
            Assert.True(caughEx is ItemNotProcessableException, $"Compound child roles must throw an ItemNotProcessableException, but threw the following instead '{caughEx}'");
            Assert.Contains($"Cannot assign a role with ID '{mockedRoleSubmitModel.RoleIds[0]}' as a child of role with ID", caughEx.Message);
        }

        [Fact]
        public async Task UpdateAsync_GivenNewTakenName_ThrowsItemNotProcessableException()
        {
            // Arrange
            mockedRoleSubmitModel.Name += "_changed_name";

            functionRepository.GetByIdAsync(mockedRoleModel.RoleFunctions[0].FunctionId)
                .Returns(mockedRoleModel.RoleFunctions[0].Function);
            roleRepository.GetByIdAsync(mockedRoleModel.Id).Returns(mockedRoleModel);

            roleRepository.GetByNameAsync(mockedRoleSubmitModel.Name).Returns(new RoleModel {
                Id = Guid.NewGuid(),
                Name = mockedRoleSubmitModel.Name,
                Description = mockedRoleSubmitModel.Description
            });

            roleRepository.UpdateAsync(Arg.Any<RoleModel>()).Returns(mockedRoleModel);

            var roleService = new RoleService(roleRepository, userRepository, functionRepository, subRealmRepository, roleTransientRepository, roleFunctionTransientRepository, roleRoleTransientRepository, mapper);
            var updaterGuid = Guid.NewGuid();

            // Act
            Exception caughEx = null;
            try
            {
                var roleResource = await roleService.UpdateAsync(mockedRoleSubmitModel, mockedRoleSubmitModel.Uuid, updaterGuid);
            }
            catch (Exception ex)
            {
                caughEx = ex;
            }

            // Assert
            Assert.True(caughEx is ItemNotProcessableException, "New taken name must throw an ItemNotProcessableException");
            Assert.Contains($"Cannot update role with ID '{mockedRoleSubmitModel.Uuid}' as the intended update name of '{mockedRoleSubmitModel.Name}' is already taken by another role.", caughEx.Message);
        }
    }
}
