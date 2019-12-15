/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Controllers;
using za.co.grindrodbank.a3s.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.A3SApiResources;

namespace za.co.grindrodbank.a3s.tests.Controllers
{
    public class PermissionController_Tests
    {
        [Fact]
        public async Task GetPermissionAsync_WithEmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            var permissionService = Substitute.For<IPermissionService>();
            var controller = new PermissionController(permissionService);

            // Act
            var result = await controller.GetPermissionAsync(Guid.Empty);

            // Assert
            var badRequestResult = result as BadRequestResult;
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task GetPermissionAsync_WithRandomGuid_ReturnsNotFoundResult()
        {
            // Arrange
            var permissionService = Substitute.For<IPermissionService>();
            var controller = new PermissionController(permissionService);

            // Act
            var result = await controller.GetPermissionAsync(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetPermissionAsync_WithTestGuid_ReturnsCorrectResult()
        {
            // Arrange
            var permissionService = Substitute.For<IPermissionService>();
            var testGuid = Guid.NewGuid();
            var testName = "TestUserName";

            permissionService.GetByIdAsync(testGuid).Returns(new Permission { Uuid = testGuid, Name = testName });

            var controller = new PermissionController(permissionService);

            // Act
            IActionResult actionResult = await controller.GetPermissionAsync(testGuid);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var permission = okResult.Value as Permission;
            Assert.NotNull(permission);
            Assert.True(permission.Uuid == testGuid, $"Retrieved Id {permission.Uuid} not the same as sample id {testGuid}.");
            Assert.True(permission.Name == testName, $"Retrieved Name {permission.Name} not the same as sample id {testName}.");
        }

        [Fact]
        public async Task ListPermissionsAsync_WithNoInputs_ReturnsList()
        {
            // Arrange
            var permissionService = Substitute.For<IPermissionService>();

            var inList = new List<Permission>();
            inList.Add(new Permission { Name = "Test Permissions 1", Uuid = Guid.NewGuid() });
            inList.Add(new Permission { Name = "Test Permissions 2", Uuid = Guid.NewGuid() });
            inList.Add(new Permission { Name = "Test Permissions 3", Uuid = Guid.NewGuid() });

            permissionService.GetListAsync().Returns(inList);

            var controller = new PermissionController(permissionService);

            // Act
            IActionResult actionResult = await controller.ListPermissionsAsync(0, 50, string.Empty, null);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var outList = okResult.Value as List<Permission>;
            Assert.NotNull(outList);

            for (var i = 0; i < outList.Count; i++)
            {
                Assert.Equal(outList[i].Uuid, inList[i].Uuid);
                Assert.Equal(outList[i].Name, inList[i].Name);
            }
        }
    }
}
