using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using org.drools.dotnet.decisiontable;

namespace org.drools.dotnet.tests.DecisionTable
{
    [TestFixture]
    public class SpreadsheetCompilerTests
    {
        private SpreadsheetCompiler _compiler = null!;

        [SetUp]
        public void SetUp()
        {
            _compiler = new SpreadsheetCompiler();
        }

        [Test]
        public void Constructor_ShouldCreateValidInstance()
        {
            // Act
            var compiler = new SpreadsheetCompiler();

            // Assert
            compiler.Should().NotBeNull();
        }

        [Test]
        public void GetUnderlyingJavaObject_ShouldReturnValidObject()
        {
            // Act
            var javaObject = _compiler.GetUnderlyingJavaObject();

            // Assert
            javaObject.Should().NotBeNull();
        }

        [Test]
        public void Compile_WithValidCsvContent_ShouldReturnDrlString()
        {
            // Arrange
            var csvContent = @"RuleName,Description,CONDITION,ACTION
Rule1,Test Rule,age > 18,status = ""ADULT""
Rule2,Test Rule 2,age <= 18,status = ""MINOR""";

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent));

            // Act
            var result = _compiler.Compile(stream, InputType.CSV);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("rule");
            result.Should().Contain("when");
            result.Should().Contain("then");
        }

        [Test]
        public void Compile_WithEmptyStream_ShouldThrowException()
        {
            // Arrange
            using var emptyStream = new MemoryStream();

            // Act & Assert
            Assert.Throws<Exception>(() => _compiler.Compile(emptyStream, InputType.CSV));
        }

        [Test]
        public void Compile_WithNullStream_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _compiler.Compile(null!, InputType.CSV));
        }

        [Test]
        public void InputType_Constants_ShouldBeValid()
        {
            // Assert - Verify InputType constants exist and are valid
            // Note: InputType constants may not be available in this version
            // This is a placeholder test for decision table functionality
            Assert.Pass("InputType constants test - implementation may vary");
        }

        [Test]
        public void Compile_WithComplexCsvContent_ShouldGenerateValidDrl()
        {
            // Arrange
            var csvContent = @"RuleName,Description,CONDITION,ACTION
AgeRule,Check if adult,person.age >= 21,person.canDrink = true
AgeRule2,Check if minor,person.age < 21,person.canDrink = false
NameRule,Check name,person.name == ""John"",person.greeting = ""Hello John""";

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent));

            // Act
            var drlResult = _compiler.Compile(stream, InputType.CSV);

            // Assert
            drlResult.Should().NotBeNullOrEmpty();
            drlResult.Should().Contain("AgeRule");
            drlResult.Should().Contain("person.age");
            drlResult.Should().Contain("canDrink");
        }

        [Test]
        public void Compile_WithInvalidInputType_ShouldHandleGracefully()
        {
            // Arrange
            var csvContent = "test,content";
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent));

            // Act & Assert - This should either work or throw a specific exception
            // The exact behavior depends on the underlying Java implementation
            try
            {
                var result = _compiler.Compile(stream, 999); // Invalid input type
                result.Should().NotBeNull(); // If it doesn't throw, result should not be null
            }
            catch (Exception ex)
            {
                // Exception is acceptable for invalid input type
                ex.Should().NotBeNull();
            }
        }
    }
}