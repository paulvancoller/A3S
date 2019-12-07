/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using Microsoft.AspNetCore.Identity;
using Xunit;
using za.co.grindrodbank.a3s.Extensions;

namespace za.co.grindrodbank.a3s.tests.Extensions
{
    public class TokenExtensions_Tests
    {
        private readonly TokenOptions tokenOptions = new TokenOptions();

        [Fact]
        public void GetAspNetUserStoreProviderName_Executed_ReturnsAspNetUserStoreProviderName()
        {
            // Arrange
            var expectedValue = "[AspNetUserStore]";

            // Act 
            var lookedUpValue = tokenOptions.GetAspNetUserStoreProviderName();

            // Assert
            Assert.True((lookedUpValue == expectedValue), $"GetAspNetUserStoreProviderName() should return {expectedValue}.");
        }

        [Fact]
        public void GetAuthenticatorKeyName_Executed_ReturnsAspNetUserStoreProviderName()
        {
            // Arrange
            var expectedValue = "AuthenticatorKey";

            // Act 
            var lookedUpValue = tokenOptions.GetAuthenticatorKeyName();

            // Assert
            Assert.True((lookedUpValue == expectedValue), $"GetAuthenticatorKeyName() should return {expectedValue}.");
        }

        [Fact]
        public void GetRecoverCodesName_Executed_ReturnsAspNetUserStoreProviderName()
        {
            // Arrange
            var expectedValue = "RecoveryCodes";

            // Act 
            var lookedUpValue = tokenOptions.GetRecoverCodesName();

            // Assert
            Assert.True((lookedUpValue == expectedValue), $"GetRecoverCodesName() should return {expectedValue}.");
        }
    }
}
