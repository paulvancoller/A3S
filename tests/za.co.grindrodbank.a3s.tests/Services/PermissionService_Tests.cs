/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
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

namespace za.co.grindrodbank.a3s.tests.Services
{
    public class PermissionService_Tests
    {
        private readonly IMapper mapper;

        public PermissionService_Tests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new PermissionResourcePermisionModelProfile());
            });

            mapper = config.CreateMapper();
        }

        [Fact]
        public async Task GetById_GivenGuid_ReturnsPermissionResource()
        {
            var permissionsRepository = Substitute.For<IPermissionRepository>();
            var guid = Guid.NewGuid();
            permissionsRepository.GetByIdAsync(guid).Returns(new PermissionModel { Name = "Test Name", Id = guid, Description = "Test permission description" });

            var permissionService = new PermissionService(permissionsRepository, mapper);
            var permissionResource = await permissionService.GetByIdAsync(guid);

            Assert.NotNull(permissionResource);
            Assert.True(permissionResource.Name == "Test Name", "Resource name must be 'Test Name'.");
            Assert.True(permissionResource.Uuid == guid, $"Resource Uuid must be '{guid}'.");
            Assert.True(permissionResource.Description == "Test permission description", "Resource description must be 'Test permission description'.");
        }

        [Fact]
        public async Task GetById_GivenUnfindableId_ReturnsNullResource()
        {
            // Arrange
            var permissionsRepository = Substitute.For<IPermissionRepository>();
            var permissionService = new PermissionService(permissionsRepository, mapper);

            // Act
            var permissionResource = await permissionService.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(permissionResource);
        }

        [Fact]
        public async Task GetList_GivenNoInput_ReturnsPermissionResourceList()
        {
            var permissionsRepository = Substitute.For<IPermissionRepository>();
            List<PermissionModel> mockedPermissionModels = new List<PermissionModel>();
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            mockedPermissionModels.Add(new PermissionModel { Name = "Test Name 1", Id = guid1, Description = "Test permissions description 1" });
            mockedPermissionModels.Add(new PermissionModel { Name = "Test Name 2", Id = guid2, Description = "Test permissions description 2" });
            permissionsRepository.GetListAsync().Returns(mockedPermissionModels);

            var permissionService = new PermissionService(permissionsRepository, mapper);
            var permissionList = await permissionService.GetListAsync();

            var permissionResource1 = permissionList.Find(am => am.Name == "Test Name 1");
            Assert.NotNull(permissionResource1);
            Assert.True(permissionResource1.GetType() == typeof(Permission), "Resource 1 must be type 'Permission'.");
            Assert.True(permissionResource1.Uuid == guid1, $"Permission resource UUID: '{permissionResource1.Uuid }' does not match expected value: '{guid1}'");
            Assert.True(permissionResource1.Description == "Test permissions description 1", $"Permission resource Description: '{permissionResource1.Description}' does not match expected value: 'Test permission description'");

            var permissionResource2 = permissionList.Find(am => am.Name == "Test Name 2");
            Assert.NotNull(permissionResource2);
            Assert.True(permissionResource2.GetType() == typeof(Permission), "Resource 2 must be type 'Permission'.");
            Assert.True(permissionResource2.Uuid == guid2, $"Permission resource UUID: '{permissionResource2.Uuid }' does not match expected value: '{guid2}'");
            Assert.True(permissionResource2.Description == "Test permissions description 2", $"Permission resource Description: '{permissionResource2.Description}' does not match expected value: 'Test permission description'");
        }
    }
}
