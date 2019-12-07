/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Controllers;
using za.co.grindrodbank.a3s.Services;

namespace za.co.grindrodbank.a3s.tests.Controllers
{
    public class TermsOfServiceController_Tests
    {
        [Fact]
        public async Task GetTermsOfServiceAsync_WithEmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            var termsOfServiceService = Substitute.For<ITermsOfServiceService>();
            var controller = new TermsOfServiceController(termsOfServiceService);

            // Act
            var result = await controller.GetTermsOfServiceAsync(Guid.Empty);

            // Assert
            var badRequestResult = result as BadRequestResult;
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task GetTermsOfServiceAsync_WithRandomGuid_ReturnsNotFoundResult()
        {
            // Arrange
            var termsOfServiceService = Substitute.For<ITermsOfServiceService>();
            var controller = new TermsOfServiceController(termsOfServiceService);

            // Act
            var result = await controller.GetTermsOfServiceAsync(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetTermsOfServiceAsync_WithTestGuid_ReturnsCorrectResult()
        {
            // Arrange
            var termsOfServiceService = Substitute.For<ITermsOfServiceService>();
            var testGuid = Guid.NewGuid();
            var testName = "TestTermsOfServiceName";

            termsOfServiceService.GetByIdAsync(testGuid, true).Returns(new TermsOfService { Uuid = testGuid, AgreementName = testName });

            var controller = new TermsOfServiceController(termsOfServiceService);

            // Act
            IActionResult actionResult = await controller.GetTermsOfServiceAsync(testGuid);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var termsOfService = okResult.Value as TermsOfService;
            Assert.NotNull(termsOfService);
            Assert.True(termsOfService.Uuid == testGuid, $"Retrieved Id {termsOfService.Uuid} not the same as sample id {testGuid}.");
            Assert.True(termsOfService.AgreementName == testName, $"Retrieved Name {termsOfService.AgreementName} not the same as sample id {testName}.");
        }

        [Fact]
        public async Task ListTermsOfServicesAsync_WithNoInputs_ReturnsList()
        {
            // Arrange
            var termsOfServiceService = Substitute.For<ITermsOfServiceService>();

            var inList = new List<TermsOfService>();
            inList.Add(new TermsOfService { AgreementName = "Test TermsOfServices 1", Uuid = Guid.NewGuid() });
            inList.Add(new TermsOfService { AgreementName = "Test TermsOfServices 2", Uuid = Guid.NewGuid() });
            inList.Add(new TermsOfService { AgreementName = "Test TermsOfServices 3", Uuid = Guid.NewGuid() });

            termsOfServiceService.GetListAsync().Returns(inList);
            var controller = new TermsOfServiceController(termsOfServiceService);

            // Act
            IActionResult actionResult = await controller.ListTermsOfServicesAsync(0, 50, null);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var outList = okResult.Value as List<TermsOfService>;
            Assert.NotNull(outList);

            for (var i = 0; i < outList.Count; i++)
            {
                Assert.Equal(outList[i].Uuid, inList[i].Uuid);
                Assert.Equal(outList[i].AgreementName, inList[i].AgreementName);
            }
        }

        [Fact]
        public async Task CreateTermsOfServiceAsync_WithTestTermsOfService_ReturnsCreatedTermsOfService()
        {
            // Arrange
            var termsOfServiceService = Substitute.For<ITermsOfServiceService>();
            var inputModel = new TermsOfServiceSubmit()
            {
                Uuid = Guid.NewGuid(),
                AgreementName = "Test TermsOfService Name"
            };

            termsOfServiceService.CreateAsync(inputModel, Arg.Any<Guid>())
                .Returns(new TermsOfService()
                {
                    Uuid = inputModel.Uuid,
                    AgreementName = inputModel.AgreementName
                }
                );

            var controller = new TermsOfServiceController(termsOfServiceService);

            // Act
            IActionResult actionResult = await controller.CreateTermsOfServiceAsync(inputModel);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var termsOfService = okResult.Value as TermsOfService;
            Assert.NotNull(termsOfService);
            Assert.True(termsOfService.Uuid == inputModel.Uuid, $"Retrieved Id {termsOfService.Uuid} not the same as sample id {inputModel.Uuid}.");
            Assert.True(termsOfService.AgreementName == inputModel.AgreementName, $"Retrieved Name {termsOfService.AgreementName} not the same as sample Name {inputModel.AgreementName}.");
        }

        [Fact]
        public async Task DeleteTermsOfServiceAsync_WithGuid_ReturnsNoContent()
        {
            // Arrange
            var termsOfServiceService = Substitute.For<ITermsOfServiceService>();
            var controller = new TermsOfServiceController(termsOfServiceService);

            // Act
            IActionResult actionResult = await controller.DeleteTermsOfServiceAsync(Guid.NewGuid());

            // Assert
            var noContentResult = actionResult as NoContentResult;
            Assert.NotNull(noContentResult);
        }
    }
}
