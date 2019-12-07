/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;
using za.co.grindrodbank.a3s.Helpers;

namespace za.co.grindrodbank.a3s.tests.Helpers
{
    public class ClaimsHelper_Tests
    {
        private readonly ClaimsPrincipal user;
        private readonly string userId;

        public ClaimsHelper_Tests()
        {
            userId = Guid.NewGuid().ToString();

            user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                                        new Claim(ClaimTypes.Name, "example name"),
                                        new Claim(ClaimTypes.NameIdentifier, userId),
                                        new Claim("custom-claim", "example claim value"),
            }, "mock"));
        }

        [Fact]
        public void GetScalarClaimValue_GivenNullClaimsPrinicple_ReturnsDefaulValue()
        {
            // Arrange
            string defaultValue = "12321";

            // Act
            string lookupValue = ClaimsHelper.GetScalarClaimValue<string>(null, ClaimTypes.NameIdentifier, defaultValue);

            // Assert
            Assert.True(lookupValue.Equals(defaultValue), "With a null Claims Principle the default value must be returned.");
        }

        [Fact]
        public void GetScalarClaimValue_GivenUnfindableClaim_ReturnsDefaulValue()
        {
            // Arrange
            string defaultValue = "12321";

            // Act
            string lookupValue = ClaimsHelper.GetScalarClaimValue<string>(user, ClaimTypes.Country, defaultValue);

            // Assert
            Assert.True(lookupValue.Equals(defaultValue), "With an unfindable claim the default value must be returned.");
        }

        [Fact]
        public void GetScalarClaimValue_GivenFindableClaim_ReturnsClaimValue()
        {
            // Arrange
            string defaultValue = "12321";

            // Act
            string lookupValue = ClaimsHelper.GetScalarClaimValue<string>(user, ClaimTypes.NameIdentifier, defaultValue);

            // Assert
            Assert.True(lookupValue.Equals(userId), "With a findable claim the claim value must be returned.");
            Assert.False(lookupValue.Equals(defaultValue), "With a findable claim the default value must NOT be returned.");
        }

        [Fact]
        public void GetDataPolicies_GivenNullClaimsPrinicple_ReturnsEmptyList()
        {
            // Arrange

            // Act
            List<string> lookupList = ClaimsHelper.GetDataPolicies(null);

            // Assert
            Assert.True(lookupList.Count == 0, "With a null Claims Principle an empty list must be returned.");
        }

        [Fact]
        public void GetDataPolicies_GivenUnfindableClaim_ReturnsEmptyList()
        {
            // Arrange

            // Act
            List<string> lookupList = ClaimsHelper.GetDataPolicies(user);

            // Assert
            Assert.True(lookupList.Count == 0, "With an unfindable claim an empty list must be returned.");
        }

        [Fact]
        public void GetDataPolicies_GivenFindableDataPolicyClaim_ReturnsEmptyList()
        {
            // Arrange
            user.AddIdentity(new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.Name, "example name"),
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim("custom-claim", "example claim value"),
                    new Claim("dataPolicy", "data policy value 1"),
                    new Claim("dataPolicy", "data policy value 2")
                }));

            // Act
            List<string> lookupList = ClaimsHelper.GetDataPolicies(user);

            // Assert
            Assert.True(lookupList.Count == 2, "With findable datapolicy claim a populated list must be returned.");
            Assert.True(string.Compare(lookupList[0], "data policy value 1") == 0, $"Expected datapolicy value 'data policy value 1' not found. Found value: '{lookupList[0]}'.");
            Assert.True(string.Compare(lookupList[1], "data policy value 2") == 0, $"Expected datapolicy value 'data policy value 2' not found. Found value: '{lookupList[1]}'.");
        }
    }
}
