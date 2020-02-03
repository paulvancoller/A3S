/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.ComponentModel;
using System.Globalization;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.Converters;

namespace za.co.grindrodbank.a3s.tests.Converters
{
    public class CustomEnumConverter_Tests
    {
        public CustomEnumConverter_Tests()
        {
        }

        [Fact]
        public void CanConvertFrom_StringSourceType_ReturnsTrue()
        {
            // Arrange
            var customEnumConverter = new CustomEnumConverter<string>();
            var mockTypeDescriptorContext = Substitute.For<ITypeDescriptorContext>();

            // Act
            bool result = customEnumConverter.CanConvertFrom(mockTypeDescriptorContext, typeof(string));

            // Assert
            Assert.True(result, "Type string can be converted from.");
        }

        [Fact]
        public void CanConvertFrom_NonSourceType_ReturnsFalse()
        {
            // Arrange
            var customEnumConverter = new CustomEnumConverter<string>();
            var mockTypeDescriptorContext = Substitute.For<ITypeDescriptorContext>();

            // Act
            bool result = customEnumConverter.CanConvertFrom(mockTypeDescriptorContext, typeof(int));

            // Assert
            Assert.False(result, "Non-string type cannot be converted from.");
        }

        [Fact]
        public void ConvertFrom_EmptyString_ReturnsNull()
        {
            // Arrange
            var customEnumConverter = new CustomEnumConverter<string>();
            var mockTypeDescriptorContext = Substitute.For<ITypeDescriptorContext>();

            // Act
            object result = customEnumConverter.ConvertFrom(mockTypeDescriptorContext, CultureInfo.CurrentCulture, string.Empty);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ConvertFrom_String_ReturnsNull()
        {
            // Arrange
            var customEnumConverter = new CustomEnumConverter<string>();
            var mockTypeDescriptorContext = Substitute.For<ITypeDescriptorContext>();

            // Act
            object result = customEnumConverter.ConvertFrom(mockTypeDescriptorContext, CultureInfo.CurrentCulture, "Test String");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test String", result);
        }
    }
}
