/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Security.Claims;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.Controllers;

namespace za.co.grindrodbank.a3s.tests.Controllers
{
    public class HomeController_Tests
    {
        [Fact]
        public void Index_Executed_ViewReturned()
        {
            // Arrange
            var homeController = new HomeController();

            // Act
            IActionResult actionResult = homeController.Index();

            // Assert
            var viewResult = actionResult as ViewResult;
            Assert.NotNull(viewResult);

            homeController.Dispose();
        }

        [Fact]
        public void Error_Executed_ViewReturned()
        {
            // Arrange
            var homeController = new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "example name"),
                            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                            new Claim("custom-claim", "example claim value"),
                        }, "mock"))
                    }
                }
            };

            // Act
            IActionResult actionResult = homeController.Error();

            // Assert
            var viewResult = actionResult as ViewResult;
            Assert.NotNull(viewResult);

            homeController.Dispose();
        }
    }
}
