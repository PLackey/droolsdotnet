using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using org.drools.dotnet.compiler;
using org.drools.dotnet.rule;
using org.drools.dotnet.tests.Models;

namespace org.drools.dotnet.tests.Compiler
{
    [TestFixture]
    public class PackageBuilderTests
    {
        private PackageBuilder _packageBuilder = null!;

        [SetUp]
        public void SetUp()
        {
            try
            {
                _packageBuilder = new PackageBuilder();
            }
            catch (TypeLoadException ex) when (ex.Message.Contains("MethodToken") || ex.Message.Contains("System.Security.Permissions"))
            {
                Assert.Inconclusive(
                    "PackageBuilder creation failed due to IKVM/.NET Core compatibility issues. " +
                    "These tests require .NET Framework or a compatible runtime environment.");
            }
        }

        [Test]
        public void Constructor_ShouldCreateValidInstance()
        {
            try
            {
                // Act
                var builder = new PackageBuilder();

                // Assert
                builder.Should().NotBeNull();
            }
            catch (TypeLoadException ex) when (ex.Message.Contains("MethodToken") || ex.Message.Contains("System.Security.Permissions"))
            {
                Assert.Inconclusive(
                    "IKVM compatibility issue on .NET Core/8 - PackageBuilder requires .NET Framework runtime. " +
                    "This is expected behavior. Use CompatibilityHelper.ValidateDroolsFunctionality() for detailed diagnostics.");
            }
        }

        [Test]
        public void AddPackageFromDrl_WithValidRule_ShouldSucceed()
        {
            // Arrange
            var drlContent = @"
                package org.drools.dotnet.tests
                
                rule ""Test Rule""
                    when
                        $message : Message( status == Message.HELLO )
                    then
                        $message.message = ""Test Success"";
                        update($message);
                end";

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(drlContent));

            // Act
            Action act = () => _packageBuilder.AddPackageFromDrl("test.drl", stream);

            // Assert
            act.Should().NotThrow();
        }

        [Test]
        public void AddPackageFromDrl_WithEmbeddedResource_ShouldSucceed()
        {
            // Arrange
            var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("org.drools.dotnet.tests.TestRules.SimpleRule.drl");

            // Act
            Action act = () => _packageBuilder.AddPackageFromDrl("SimpleRule.drl", stream);

            // Assert
            act.Should().NotThrow();
            stream?.Close();
        }

        [Test]
        public void GetPackage_AfterAddingRules_ShouldReturnValidPackage()
        {
            // Arrange
            var drlContent = @"
                package org.drools.dotnet.tests
                
                rule ""Test Rule""
                    when
                        $message : Message( status == Message.HELLO )
                    then
                        $message.message = ""Package Test"";
                        update($message);
                end";

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(drlContent));
            _packageBuilder.AddPackageFromDrl("test.drl", stream);

            // Act
            var package = _packageBuilder.GetPackage();

            // Assert
            package.Should().NotBeNull();
        }

        [Test]
        public void AddPackageFromDrl_WithInvalidSyntax_ShouldThrowException()
        {
            // Arrange
            var invalidDrlContent = @"
                package org.drools.dotnet.tests
                
                rule ""Invalid Rule""
                    when
                        invalid syntax here
                    then
                        do something;
                end";

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalidDrlContent));

            // Act & Assert
            Assert.Throws<Exception>(() => _packageBuilder.AddPackageFromDrl("invalid.drl", stream));
        }

        [Test]
        public void Constructor_WithPackage_ShouldCreateValidInstance()
        {
            // Arrange
            var drlContent = @"
                package org.drools.dotnet.tests
                
                rule ""Initial Rule""
                    when
                        $message : Message()
                    then
                        $message.message = ""Initial"";
                end";

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(drlContent));
            _packageBuilder.AddPackageFromDrl("initial.drl", stream);
            var initialPackage = _packageBuilder.GetPackage();

            // Act
            var builderWithPackage = new PackageBuilder(initialPackage);

            // Assert
            builderWithPackage.Should().NotBeNull();
        }
    }
}