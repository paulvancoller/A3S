/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Threading.Tasks;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using za.co.grindrodbank.a3s.ContentFormatters;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using za.co.grindrodbank.a3s.tests.Fakes;

namespace za.co.grindrodbank.a3s.tests.ContentFormatters
{
    public class YamlOutputFormatter_Tests
    {
        public static TheoryData<string, string, bool> WriteCorrectCharacterEncoding
        {
            get
            {
                var data = new TheoryData<string, string, bool>
                {
                    { "This is a test 激光這兩個字是甚麼意思 string written using utf-8", "utf-8", true },
                    { "This is a test 激光這兩個字是甚麼意思 string written using utf-16", "utf-16", true },
                    { "This is a test 激光這兩個字是甚麼意思 string written using utf-32", "utf-32", false },
                    { "This is a test æøå string written using iso-8859-1", "iso-8859-1", false },
                };

                return data;
            }
        }

        protected static Encoding CreateOrGetSupportedEncoding(
            YamlOutputFormatter formatter,
            string encodingAsString,
            bool isDefaultEncoding)
        {
            Encoding encoding = null;
            if (isDefaultEncoding)
            {
                encoding = formatter
                    .SupportedEncodings
                    .First((e) => e.WebName.Equals(encodingAsString, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                encoding = Encoding.GetEncoding(encodingAsString);
                formatter.SupportedEncodings.Add(encoding);
            }

            return encoding;
        }

        protected static ActionContext GetActionContext(
            MediaTypeHeaderValue contentType,
            Stream responseStream = null)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.ContentType = contentType.ToString();
            httpContext.Request.Headers[HeaderNames.AcceptCharset] = contentType.Charset.ToString();


            httpContext.Response.Body = responseStream ?? new MemoryStream();
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }

        [Theory]
        [MemberData(nameof(WriteCorrectCharacterEncoding))]
        public async Task WriteToStreamAsync_UsesCorrectCharacterEncoding(string content, string encodingAsString, bool isDefaultEncoding)
        {
            // Arrange
            var formatter = new YamlOutputFormatter((Serializer)new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
                .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
                .Build());

            var expectedContent = $"\"{content}\"";
            var mediaType = MediaTypeHeaderValue.Parse(string.Format("application/json; charset={0}", encodingAsString));
            var encoding = CreateOrGetSupportedEncoding(formatter, encodingAsString, isDefaultEncoding);


            var body = new MemoryStream();
            var actionContext = GetActionContext(mediaType, body);

            var outputFormatterContext = new OutputFormatterWriteContext(
                actionContext.HttpContext,
                new HttpResponseStreamWriterFactoryFake().CreateWriter,
                typeof(string),
                content)
            {
                ContentType = new StringSegment(mediaType.ToString()),
            };

            // Act
            await formatter.WriteResponseBodyAsync(outputFormatterContext, Encoding.GetEncoding(encodingAsString));

            // Assert
            var actualContent = string.Concat("\"", encoding.GetString(body.ToArray()).TrimEnd('\n'), "\"");
            Assert.Equal(expectedContent, actualContent, StringComparer.OrdinalIgnoreCase);
        }

        [Theory]
        [MemberData(nameof(WriteCorrectCharacterEncoding))]
        public async Task WriteToStreamAsync_EncodingNull_ReturnsArgumentNullException(string content, string encodingAsString, bool isDefaultEncoding)
        {
            // Arrange
            var formatter = new YamlOutputFormatter((Serializer)new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
                .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
                .Build());

            var expectedContent = $"\"{content}\"";
            var mediaType = MediaTypeHeaderValue.Parse(string.Format("application/json; charset={0}", encodingAsString));
            var encoding = CreateOrGetSupportedEncoding(formatter, encodingAsString, isDefaultEncoding);


            var body = new MemoryStream();
            var actionContext = GetActionContext(mediaType, body);

            var outputFormatterContext = new OutputFormatterWriteContext(
                actionContext.HttpContext,
                new HttpResponseStreamWriterFactoryFake().CreateWriter,
                typeof(string),
                content)
            {
                ContentType = new StringSegment(mediaType.ToString()),
            };

            // Act
            Exception caughtException = null;

            try
            {
                await formatter.WriteResponseBodyAsync(outputFormatterContext, null);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.True(caughtException is ArgumentNullException);
        }

        [Fact]
        public async Task WriteToStreamAsync_ContextNull_ReturnsArgumentNullException()
        {
            // Arrange
            var formatter = new YamlOutputFormatter((Serializer)new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
                .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
                .Build());

            // Act
            Exception caughtException = null;

            try
            {
                await formatter.WriteResponseBodyAsync(null, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.True(caughtException is ArgumentNullException);
        }
    }
}
