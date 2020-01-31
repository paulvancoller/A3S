using System;
using System.Collections.Generic;
using NSubstitute;
using Xunit;
using YamlDotNet.Serialization;
using za.co.grindrodbank.a3s.ContentFormatters;

namespace za.co.grindrodbank.a3s.tests.ContentFormatters
{
    public class CommentGatheringTypeInspector_Tests
    {
        private readonly ITypeInspector mockTypeInspector;

        public CommentGatheringTypeInspector_Tests()
        {
            mockTypeInspector = Substitute.For<ITypeInspector>();
        }

        [Fact]
        public void Constructor_NullIinerTypeDescriptor_ArgumentNullThrown()
        {
            // Arrange

            // Act
            Exception caughException = null;

            try
            {
                new CommentGatheringTypeInspector(null);
            }
            catch (Exception ex)
            {
                caughException = ex;
            }

            // Assert
            Assert.True(caughException is ArgumentNullException, "Null file specified must throw ArgumentNullException.");
        }

        [Fact]
        public void GetProperties_TypeAndContainerSpecified_PropertiesReturned()
        {
            // Arrange
            var commentGatheringTypeInspector = new CommentGatheringTypeInspector(mockTypeInspector);
            mockTypeInspector.GetProperties(Arg.Any<Type>(), Arg.Any<object>()).Returns(new List<PropertyDescriptor>());
            // Act
            IEnumerable<IPropertyDescriptor> results = commentGatheringTypeInspector.GetProperties(Type.GetType("string", false), null);

            // Assert
            Assert.True(results != null, "Property Descriptors must be returned.");
        }
    }
}
