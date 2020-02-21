/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.Controllers;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Services;

namespace za.co.grindrodbank.a3s.tests.Controllers
{
    public class ConsentOfServiceController_Tests
    {
        public ConsentOfServiceController_Tests()
        {
            consentOfServiceService = Substitute.For<IConsentOfServiceService>();
        }

        private readonly IConsentOfServiceService consentOfServiceService;

        [Fact]
        public async Task GetCurrentConsentOfServiceAsync_NeverUploaded_ReturnsNotFoundResult()
        {
            // Arrange
            var controller = new ConsentOfServiceController(consentOfServiceService);
            ConsentOfService consentOfService = null;
            consentOfServiceService.GetCurrentConsentAsync().Returns(consentOfService);

            // Act
            var actionResult = await controller.GetCurrentConsentOfServiceAsync();

            // Assert
            var notFoundResult = actionResult as NotFoundResult;
            Assert.NotNull(notFoundResult);
            Assert.Null(consentOfService);
        }

        [Fact]
        public async Task GetCurrentConsentOfServiceAsync_OnceUploaded_ReturnsOkResult()
        {
            // Arrange
            var controller = new ConsentOfServiceController(consentOfServiceService);
            var consentOfService = new ConsentOfService {ConsentFileData = "fileData"};
            consentOfServiceService.GetCurrentConsentAsync().Returns(consentOfService);

            // Act
            var actionResult = await controller.GetCurrentConsentOfServiceAsync();

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.NotNull(consentOfService);
            Assert.NotNull(consentOfService.ConsentFileData);
        }

        [Fact]
        public async Task UpdateConsentOfServiceAsync_ConsentFileDataNull_ReturnsBadRequest()
        {
            // Arrange
            var consentService = Substitute.For<IConsentOfServiceService>();
            var inputModel = new ConsentOfService {ConsentFileData = null};
            var controller = new ConsentOfServiceController(consentService);

            // Act
            var actionResult = await controller.UpdateConsentOfServiceAsync(inputModel);

            // Assert
            var badRequestResult = actionResult as BadRequestResult;
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task UpdateConsentOfServiceAsync_InputRequestNull_ReturnsBadRequest()
        {
            // Arrange
            var consentService = Substitute.For<IConsentOfServiceService>();
            ConsentOfService inputModel = null;
            var controller = new ConsentOfServiceController(consentService);

            // Act
            var actionResult = await controller.UpdateConsentOfServiceAsync(inputModel);

            // Assert
            var badRequestResult = actionResult as BadRequestResult;
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task UpdateConsentOfServiceAsync_ValidConsentContent_ReturnsNoContent()
        {
            // Arrange
            var consentService = Substitute.For<IConsentOfServiceService>();
            var inputModel = new ConsentOfService
            {
                ConsentFileData = "validConsentFileData"
            };

            consentService.UpdateCurrentConsentAsync(inputModel, Arg.Any<Guid>())
                .Returns(true);

            var controller = new ConsentOfServiceController(consentService);

            // Act
            var actionResult = await controller.UpdateConsentOfServiceAsync(inputModel);

            // Assert
            var noContentResult = actionResult as NoContentResult;
            Assert.NotNull(noContentResult);
            consentService.Received(1);
        }
    }
}