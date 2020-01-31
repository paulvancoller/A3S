/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using za.co.grindrodbank.a3s.ContentFormatters;
using za.co.grindrodbank.a3s.tests.Fakes;

namespace za.co.grindrodbank.a3s.tests.ContentFormatters
{
    public class YamlInputFormatter_Tests
    {
        [Fact]
        public async Task ReadRequestBodyAsync_ParametersSpecified_ReturnsFormattedResult()
        {
            // Arrange
            var yamlInputFormatter = new YamlInputFormatter((Deserializer)new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build());
            var content = "[{\"op\":\"add\",\"path\":\"Customer/Name\",\"value\":\"John\"}]";
            var contentBytes = Encoding.UTF8.GetBytes(content);

            var modelState = new ModelStateDictionary();

            string userId = Guid.NewGuid().ToString();
            var httpContext = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "example name"),
                            new Claim(ClaimTypes.NameIdentifier, userId),
                            new Claim("sub", userId),
                            new Claim("custom-claim", "example claim value"),
                        }, "mock"))
            };

            var provider = new EmptyModelMetadataProvider();
            var metadata = provider.GetMetadataForType(typeof(string));
            var context = new InputFormatterContext(httpContext, string.Empty, modelState, metadata, new HttpRequestStreamReaderFactoryFake().CreateReader);

            // Act
            InputFormatterResult result = await yamlInputFormatter.ReadRequestBodyAsync(context, Encoding.UTF8);

            Assert.NotNull(result);
            Assert.True(result.IsModelSet);
        }


        [Fact]
        public async Task ReadRequestBodyAsync_ContextNull_ArgumentNullExceptionThrown()
        {
            // Arrange
            var yamlInputFormatter = new YamlInputFormatter((Deserializer)new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build());
            var content = "[{\"op\":\"add\",\"path\":\"Customer/Name\",\"value\":\"John\"}]";
            var contentBytes = Encoding.UTF8.GetBytes(content);

            var modelState = new ModelStateDictionary();

            string userId = Guid.NewGuid().ToString();
            var httpContext = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "example name"),
                            new Claim(ClaimTypes.NameIdentifier, userId),
                            new Claim("sub", userId),
                            new Claim("custom-claim", "example claim value"),
                        }, "mock"))
            };

            var provider = new EmptyModelMetadataProvider();
            var metadata = provider.GetMetadataForType(typeof(string));
            var context = new InputFormatterContext(httpContext, string.Empty, modelState, metadata, new HttpRequestStreamReaderFactoryFake().CreateReader);

            // Act
            Exception caughtException = null;
            try
            {
                InputFormatterResult result = await yamlInputFormatter.ReadRequestBodyAsync(context, null);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.True(caughtException is ArgumentNullException);
        }

        [Fact]
        public async Task ReadRequestBodyAsync_EncodingNull_ArgumentNullExceptionThrown()
        {
            // Arrange
            var yamlInputFormatter = new YamlInputFormatter((Deserializer)new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build());

            // Act
            Exception caughtException = null;
            try
            {
                InputFormatterResult result = await yamlInputFormatter.ReadRequestBodyAsync(null, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.True(caughtException is ArgumentNullException);
        }
    }
}
