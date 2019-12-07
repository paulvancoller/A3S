/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Threading.Tasks;
using GlobalErrorHandling.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace za.co.grindrodbank.a3s.tests.Extensions
{
    public class ExceptionMiddlewareExtensions_Tests
    {
        [Fact]
        public void ConfigureExceptionHandler_Executed_ThrowsNoException()
        {
            var mockApplicationBuilder = Substitute.For<IApplicationBuilder>();

            try
            {
                ExceptionMiddlewareExtensions.ConfigureExceptionHandler(mockApplicationBuilder);
            }
            catch (Exception ex)
            {
                Assert.True(false, $"Executing ConfigureExceptionHandler method should not return an exception. Exception Type {ex.GetType().ToString()} with message '{ex.Message}' caught.");
            }
        }

        [Fact]
        public void WriteException_GivenException_ThrowsNoException()
        {
            try
            {
                ExceptionMiddlewareExtensions.WriteException(new Exception("Test Exception"));
            }
            catch (Exception ex)
            {
                Assert.True(false, $"Executing WriteException method should not return an exception. Exception Type {ex.GetType().ToString()} with message '{ex.Message}' caught.");
            }
        }

        [Fact]
        public void WriteException_GivenNullException_ThrowsNoException()
        {
            var mockConfiguration = Substitute.For<IConfiguration>();

            try
            {
                ExceptionMiddlewareExtensions.WriteException(null);
            }
            catch (Exception ex)
            {
                Assert.True(false, $"Executing WriteException method with null exception should not return an exception. Exception Type {ex.GetType().ToString()} with message '{ex.Message}' caught.");
            }

            Assert.True(true, $"Executing WriteException method with null exception should not return an exception.");
        }
    }
}
