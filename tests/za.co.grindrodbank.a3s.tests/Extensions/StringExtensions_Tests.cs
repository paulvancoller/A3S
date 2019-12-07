/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using Xunit;
using za.co.grindrodbank.a3s.Extensions;

namespace za.co.grindrodbank.a3s.tests.Extensions
{
    public class StringExtensions_Tests
    {
        [Fact]
        public void ToSnakeCase_GivenAllLowerCase_ReturnsAllLowerCase()
        {
            // Arrange
            string testValue = "testusertable";

            // Act 
            var returnedString = testValue.ToSnakeCase();

            // Assert
            Assert.True((returnedString == "testusertable"), "Sending all lower case to ToSnakeCase() method should return all lower case.");
        }

        [Fact]
        public void ToSnakeCase_GivenAllUpperCase_ReturnsAllLowerCase()
        {
            // Arrange
            string testValue = "TESTUSERTABLE";

            // Act 
            var returnedString = testValue.ToSnakeCase();

            // Assert
            Assert.True((returnedString == "testusertable"), "Sending all upper case to ToSnakeCase() method should return all lower case.");
        }

        [Fact]
        public void ToSnakeCase_GivenPascalCase_ReturnsAllLowerCaseWithUndersoreSeparator()
        {
            // Arrange
            string testValue = "TestUserTable";

            // Act 
            var returnedString = testValue.ToSnakeCase();

            // Assert
            Assert.True((returnedString == "test_user_table"), "Sending all upper case to ToSnakeCase() method should return all lower case.");
        }

        [Fact]
        public void ToSnakeCase_GivenCamelCase_ReturnsAllLowerCaseWithUndersoreSeparator()
        {
            // Arrange
            string testValue = "testUserTable";

            // Act 
            var returnedString = testValue.ToSnakeCase();

            // Assert
            Assert.True((returnedString == "test_user_table"), "Sending all upper case to ToSnakeCase() method should return all lower case.");
        }

        [Fact]
        public void IsBase64String_GivenBase64String_ReturnsTrue()
        {
            // Arrange
            string testValue = "VGVzdFVzZXJUYWJsZQ==";

            // Act 
            var isBase64String = testValue.IsBase64String();

            // Assert
            Assert.True(isBase64String, "Sending a base64 string to IsBase64String() method should return true.");
        }

        [Fact]
        public void IsBase64String_GivenPlainString_ReturnsTrue()
        {
            // Arrange
            string testValue = "ThisIsNotABase64String";

            // Act 
            var isBase64String = testValue.IsBase64String();

            // Assert
            Assert.False(isBase64String, "Sending a plain string to IsBase64String() method should return true.");
        }
    }
}
