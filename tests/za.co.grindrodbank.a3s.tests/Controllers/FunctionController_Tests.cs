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
    public class FunctionController_Tests
    {
        private readonly Function functionModel;
        private readonly FunctionSubmit functionSubmitModel;

        public FunctionController_Tests()
        {
            functionModel = new Function()
            {
                Uuid = Guid.NewGuid(),
                Name = "Test Function Name",
                Description = "Test Function Description",
                ApplicationId = new Guid(),
                Permissions = new List<Permission>()
                {
                    new Permission() { Uuid = Guid.NewGuid() },
                    new Permission() { Uuid = Guid.NewGuid() }
                }
            };

            functionSubmitModel = new FunctionSubmit()
            {
                Uuid = functionModel.Uuid,
                Name = functionModel.Name,
                Description = functionModel.Description,
                ApplicationId = functionModel.ApplicationId,
                Permissions = new List<Guid>()
                {
                    functionModel.Permissions[0].Uuid,
                    functionModel.Permissions[1].Uuid
                }
            };
        }

        [Fact]
        public async Task GetFunctionAsync_WithEmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            var functionService = Substitute.For<IFunctionService>();
            var controller = new FunctionController(functionService);

            // Act
            var result = await controller.GetFunctionAsync(Guid.Empty);

            // Assert
            var badRequestResult = result as BadRequestResult;
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task GetFunctionAsync_WithRandomGuid_ReturnsNotFoundResult()
        {
            // Arrange
            var functionService = Substitute.For<IFunctionService>();
            var controller = new FunctionController(functionService);

            // Act
            var result = await controller.GetFunctionAsync(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetFunctionAsync_WithTestGuid_ReturnsCorrectResult()
        {
            // Arrange
            var functionService = Substitute.For<IFunctionService>();
            var testGuid = Guid.NewGuid();
            var testName = "TestUserName";

            functionService.GetByIdAsync(testGuid).Returns(new Function { Uuid = testGuid, Name = testName });

            var controller = new FunctionController(functionService);

            // Act
            IActionResult actionResult = await controller.GetFunctionAsync(testGuid);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var function = okResult.Value as Function;
            Assert.NotNull(function);
            Assert.True(function.Uuid == testGuid, $"Retrieved Id {function.Uuid} not the same as sample id {testGuid}.");
            Assert.True(function.Name == testName, $"Retrieved Name {function.Name} not the same as sample id {testName}.");
        }

        [Fact]
        public async Task ListFunctionsAsync_WithNoInputs_ReturnsList()
        {
            // Arrange
            var functionService = Substitute.For<IFunctionService>();

            var inList = new List<Function>();
            inList.Add(new Function { Name = "Test Functions 1", Uuid = Guid.NewGuid() });
            inList.Add(new Function { Name = "Test Functions 2", Uuid = Guid.NewGuid() });
            inList.Add(new Function { Name = "Test Functions 3", Uuid = Guid.NewGuid() });

            functionService.GetListAsync().Returns(inList);

            var controller = new FunctionController(functionService);

            // Act
            IActionResult actionResult = await controller.ListFunctionsAsync(false, 0, 50, string.Empty, null);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var outList = okResult.Value as List<Function>;
            Assert.NotNull(outList);

            for (var i = 0; i < outList.Count; i++)
            {
                Assert.Equal(outList[i].Uuid, inList[i].Uuid);
                Assert.Equal(outList[i].Name, inList[i].Name);
            }
        }

        [Fact]
        public async Task UpdateFunctionAsync_WithRandomGuid_ReturnsNotImplemented()
        {
            // Arrange
            var functionService = Substitute.For<IFunctionService>();
            var controller = new FunctionController(functionService);

            // Act
            IActionResult actionResult = await controller.UpdateFunctionAsync(Guid.NewGuid(), new FunctionSubmit());

            // Assert
            var badRequestResult = actionResult as BadRequestResult;
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task UpdateFunctionAsync_WithBlankGuid_ReturnsNotImplemented()
        {
            // Arrange
            var functionService = Substitute.For<IFunctionService>();
            var controller = new FunctionController(functionService);

            functionService.UpdateAsync(functionSubmitModel, Arg.Any<Guid>())
                .Returns(functionModel);

            // Act
            IActionResult actionResult = await controller.UpdateFunctionAsync(Guid.Empty, functionSubmitModel);

            // Assert
            var badRequestResult = actionResult as BadRequestResult;
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task UpdateFunctionAsync_WithBlankGuidInBody_ReturnsNotImplemented()
        {
            // Arrange
            var functionService = Substitute.For<IFunctionService>();
            var controller = new FunctionController(functionService);

            functionService.UpdateAsync(functionSubmitModel, Arg.Any<Guid>())
                .Returns(functionModel);

            functionSubmitModel.Uuid = Guid.Empty;

            // Act
            IActionResult actionResult = await controller.UpdateFunctionAsync(Guid.NewGuid(), functionSubmitModel);

            // Assert
            var badRequestResult = actionResult as BadRequestResult;
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task UpdateFunctionAsync_WithTestFunction_ReturnsFunctionModel()
        {
            // Arrange
            var functionService = Substitute.For<IFunctionService>();
            var controller = new FunctionController(functionService);

            functionService.UpdateAsync(functionSubmitModel, Arg.Any<Guid>())
                .Returns(functionModel);

            // Act
            IActionResult actionResult = await controller.UpdateFunctionAsync(functionSubmitModel.Uuid, functionSubmitModel);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var function = okResult.Value as Function;
            Assert.NotNull(function);
            Assert.True(function.Uuid == functionSubmitModel.Uuid, $"Retrieved Id {function.Uuid} not the same as sample id {functionSubmitModel.Uuid}.");
            Assert.True(function.Name == functionSubmitModel.Name, $"Retrieved Name {function.Name} not the same as sample Name {functionSubmitModel.Name}.");
            Assert.True(function.Description == functionSubmitModel.Description, $"Retrieved Description {function.Description} not the same as sample Description {functionSubmitModel.Description}.");
            Assert.True(function.ApplicationId == functionSubmitModel.ApplicationId, $"Retrieved ApplicationId {function.ApplicationId} not the same as sample ApplicationId {functionSubmitModel.ApplicationId}.");
            Assert.True(function.Permissions[0].Uuid == functionSubmitModel.Permissions[0], $"Retrieved Permissions id {function.Permissions[0].Uuid} not the same as sample Permissions id {functionSubmitModel.Permissions[0]}.");
            Assert.True(function.Permissions[1].Uuid == functionSubmitModel.Permissions[1], $"Retrieved Permissions id {function.Permissions[1].Uuid} not the same as sample Permissions id {functionSubmitModel.Permissions[1]}.");
        }
    }
}
