/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using IdentityServer4.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3sidentityserver.Quickstart.UI;

namespace za.co.grindrodbank.a3sidentityserver.tests.Quickstart.Home
{
    public class HomeController_Tests
    {
        [Fact]
        public void Index_SetupAsDevelopment_ViewReturned()
        {
            // Arrange
            var identityServerInteractionService = Substitute.For<IIdentityServerInteractionService>();
            var hostingEnvironment = Substitute.For<IWebHostEnvironment>();

            hostingEnvironment.EnvironmentName = "Development";

            var homeController = new HomeController(identityServerInteractionService, hostingEnvironment);

            // Act
            var actionResult = homeController.Index();

            // Assert
            var viewResult = actionResult as ViewResult;
            Assert.NotNull(viewResult);
        }

        [Fact]
        public void Index_SetupAsProd_NotFoundReturned()
        {
            // Arrange
            var identityServerInteractionService = Substitute.For<IIdentityServerInteractionService>();
            var hostingEnvironment = Substitute.For<IWebHostEnvironment>();

            hostingEnvironment.EnvironmentName = "Production";

            var homeController = new HomeController(identityServerInteractionService, hostingEnvironment);

            // Act
            var actionResult = homeController.Index();

            // Assert
            var notFoundResult = actionResult as NotFoundResult;
            Assert.NotNull(notFoundResult);
        }
    }
}
