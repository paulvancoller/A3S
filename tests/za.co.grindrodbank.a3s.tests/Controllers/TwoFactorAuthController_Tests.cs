/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.Controllers;
using za.co.grindrodbank.a3s.Services;

namespace za.co.grindrodbank.a3s.tests.Controllers
{
    public class TwoFactorAuthController_Tests
    {
        [Fact]
        public async Task RemoveTwoFactorAuthenticationAsync_WithEmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            var twoFactorAuthService = Substitute.For<ITwoFactorAuthService>();
            var controller = new TwoFactorAuthController(twoFactorAuthService);

            // Act
            IActionResult actionResult = await controller.RemoveTwoFactorAuthenticationAsync(Guid.Empty);

            // Assert
            var badRequestResult = actionResult as BadRequestResult;
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task RemoveTwoFactorAuthenticationAsync_WithValidGuid_ReturnsNoData()
        {
            // Arrange
            var twoFactorAuthService = Substitute.For<ITwoFactorAuthService>();
            var controller = new TwoFactorAuthController(twoFactorAuthService);

            // Act
            IActionResult actionResult = await controller.RemoveTwoFactorAuthenticationAsync(Guid.NewGuid());

            // Assert
            var notContentResult = actionResult as NoContentResult;
            Assert.NotNull(notContentResult);
        }
    }
}
