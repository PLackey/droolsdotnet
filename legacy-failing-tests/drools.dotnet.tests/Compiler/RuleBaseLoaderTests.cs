using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using org.drools.dotnet.compiler;
using org.drools.dotnet.rule;

namespace org.drools.dotnet.tests.Compiler
{
    [TestFixture]
    public class RuleBaseLoaderTests
    {
        private RuleBaseLoader _loader = null!;

        [SetUp]
        public void SetUp()
        {
            _loader = new RuleBaseLoader();
        }

        [Test]
        public void GetInstance_ShouldReturnValidLoader()
        {
            // Act
            var instance = _loader.getInstance();

            // Assert
            instance.Should().NotBeNull();
        }

        [Test]
        public void LoadFromString_WithValidDrl_ShouldReturnRuleBase()
        {
            // Arrange
            var drlContent = @"
                package org.drools.dotnet.tests
                
                rule ""String Load Test""
                    when
                        $obj : Object()
                    then
                        System.Console.WriteLine(""Rule fired"");
                end";

            // Act
            var ruleBase = _loader.loadFromString(drlContent);

            // Assert
            ruleBase.Should().NotBeNull();
        }

        [Test]
        public void LoadFromStream_WithValidDrl_ShouldReturnRuleBase()
        {
            // Arrange
            var drlContent = @"
                package org.drools.dotnet.tests
                
                rule ""Stream Load Test""
                    when
                        $obj : Object()
                    then
                        System.Console.WriteLine(""Stream rule fired"");
                end";

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(drlContent));

            // Act
            var ruleBase = _loader.loadFromStream(stream);

            // Assert
            ruleBase.Should().NotBeNull();
        }

        [Test]
        public void LoadFromString_WithEmptyString_ShouldHandleGracefully()
        {
            // Arrange
            var emptyDrl = string.Empty;

            // Act & Assert
            Assert.Throws<Exception>(() => _loader.loadFromString(emptyDrl));
        }

        [Test]
        public void LoadFromString_WithInvalidDrl_ShouldThrowException()
        {
            // Arrange
            var invalidDrl = "this is not valid DRL syntax";

            // Act & Assert
            Assert.Throws<Exception>(() => _loader.loadFromString(invalidDrl));
        }

        [Test]
        public void LoadFromStream_WithNullStream_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _loader.loadFromStream(null!));
        }
    }
}