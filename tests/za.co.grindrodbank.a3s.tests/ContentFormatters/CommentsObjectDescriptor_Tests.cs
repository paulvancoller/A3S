/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using NSubstitute;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using za.co.grindrodbank.a3s.ContentFormatters;

namespace za.co.grindrodbank.a3s.tests.ContentFormatters
{
    public class CommentsObjectDescriptor_Tests
    {
        [Fact]
        public void EnterMapping_ValidParameters_ReturnsFalse()
        {
            // Arrange
            var mockObjectGraphVisitor = Substitute.For<IObjectGraphVisitor<IEmitter>>();
            var mockCommentsObjectGraphVisitor = new CommentsObjectGraphVisitor(mockObjectGraphVisitor);
            var mockPropertyDescriptor = Substitute.For<IPropertyDescriptor>();
            var mockObjectDescriptor = Substitute.For<IObjectDescriptor>();
            var mockEmitter = Substitute.For<IEmitter>();
            var commentObjectDescriptor = new CommentsObjectDescriptor(mockObjectDescriptor, "comment");

            bool result = mockCommentsObjectGraphVisitor.EnterMapping(mockPropertyDescriptor, commentObjectDescriptor, mockEmitter);

            // Assert
            Assert.False(result, "EnterMapping must return false.");
            Assert.Null(commentObjectDescriptor.Value);
            Assert.Null(commentObjectDescriptor.Type);
            Assert.Null(commentObjectDescriptor.StaticType);
            Assert.True(commentObjectDescriptor.ScalarStyle == ScalarStyle.Any);
        }
    }
}
