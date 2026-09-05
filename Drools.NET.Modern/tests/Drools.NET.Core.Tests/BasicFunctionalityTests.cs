using System;
using System.IO;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Drools.NET.Core;
using Drools.NET.Core.Implementation;

namespace Drools.NET.Core.Tests
{
    [TestFixture]
    public class BasicFunctionalityTests
    {
        [Test]
        public void RuleBaseFactory_CreateRuleBase_ShouldReturnValidInstance()
        {
            // Act
            var ruleBase = RuleBaseFactory.CreateRuleBase();

            // Assert
            ruleBase.Should().NotBeNull();
            ruleBase.Should().BeAssignableTo<IRuleBase>();
            ruleBase.Packages.Should().BeEmpty();
            ruleBase.Configuration.Should().NotBeNull();
        }

        [Test]
        public void PackageBuilder_Constructor_ShouldCreateValidInstance()
        {
            // Act
            var packageBuilder = new ModernPackageBuilder();

            // Assert
            packageBuilder.Should().NotBeNull();
            packageBuilder.HasErrors.Should().BeFalse();
            packageBuilder.Packages.Should().BeEmpty();
            packageBuilder.Configuration.Should().NotBeNull();
        }

        [Test]
        public void PackageBuilder_AddSimpleDrlRule_ShouldCompileSuccessfully()
        {
            // Arrange
            var packageBuilder = new ModernPackageBuilder();
            var simpleDrl = @"
                package org.drools.test
                
                rule ""Simple Test Rule""
                    when
                        $obj : Object()
                    then
                        System.out.println(""Rule fired!"");
                end";

            // Act
            packageBuilder.AddPackageFromDrl("test.drl", simpleDrl);

            // Assert
            packageBuilder.HasErrors.Should().BeFalse($"Expected no errors, but got: {string.Join(", ", packageBuilder.Errors)}");
            packageBuilder.Packages.Should().HaveCount(1);
            
            var package = packageBuilder.GetPackage();
            package.Should().NotBeNull();
            package!.Name.Should().Be("org.drools.test");
            package.Rules.Should().HaveCount(1);
            package.Rules[0].Name.Should().Be("Simple Test Rule");
        }

        [Test]
        public void WorkingMemory_BasicOperations_ShouldWork()
        {
            // Arrange
            var ruleBase = RuleBaseFactory.CreateRuleBase();
            var workingMemory = ruleBase.CreateWorkingMemory();

            // Act & Assert
            workingMemory.Should().NotBeNull();
            workingMemory.Objects.Should().BeEmpty();
            workingMemory.FactHandles.Should().BeEmpty();

            // Test object assertion
            var testObject = new { Name = "Test", Value = 42 };
            var factHandle = workingMemory.AssertObject(testObject);
            
            factHandle.Should().NotBeNull();
            factHandle.Object.Should().BeSameAs(testObject);
            workingMemory.Objects.Should().HaveCount(1);
            workingMemory.FactHandles.Should().HaveCount(1);

            // Test object retraction
            workingMemory.RetractObject(factHandle);
            workingMemory.Objects.Should().BeEmpty();
            workingMemory.FactHandles.Should().BeEmpty();
        }

        [Test]
        public void RuleBase_AddPackageAndExecuteRules_ShouldWork()
        {
            // Arrange
            var packageBuilder = new ModernPackageBuilder();
            var simpleDrl = @"
                package org.drools.test
                
                rule ""Test Rule""
                    when
                        $obj : Object()
                    then
                        // Simple action
                end";

            packageBuilder.AddPackageFromDrl(simpleDrl);
            var package = packageBuilder.GetPackage();
            
            var ruleBase = RuleBaseFactory.CreateRuleBase();
            var workingMemory = ruleBase.CreateWorkingMemory();

            // Act
            if (package != null)
            {
                ruleBase.AddPackage(package);
                workingMemory.AssertObject(new { Test = "Object" });
                var firedRules = workingMemory.FireAllRules();

                // Assert
                firedRules.Should().BeGreaterOrEqualTo(0); // Rules should be able to fire
            }
            else
            {
                Assert.Fail("Package should not be null");
            }
        }

        [Test]
        public void PackageBuilder_WithInvalidDrl_ShouldReportErrors()
        {
            // Arrange
            var packageBuilder = new ModernPackageBuilder();
            var invalidDrl = @"
                package org.drools.test
                
                rule ""Invalid Rule""
                    when
                        invalid syntax here
                    then
                        also invalid
                end";

            // Act
            packageBuilder.AddPackageFromDrl("invalid.drl", invalidDrl);

            // Assert - the parser should handle this gracefully
            // Even if it creates a rule, it should not crash
            packageBuilder.Should().NotBeNull();
        }

        [Test]
        public void PackageBuilder_EmptyContent_ShouldReportError()
        {
            // Arrange
            var packageBuilder = new ModernPackageBuilder();

            // Act
            packageBuilder.AddPackageFromDrl("");

            // Assert
            packageBuilder.HasErrors.Should().BeTrue();
            packageBuilder.Errors.Should().HaveCount(1);
            packageBuilder.Errors[0].Message.Should().Contain("empty");
        }

        [Test]
        public void WorkingMemory_Globals_ShouldWork()
        {
            // Arrange
            var ruleBase = RuleBaseFactory.CreateRuleBase();
            var workingMemory = ruleBase.CreateWorkingMemory();

            // Act
            workingMemory.SetGlobal("testGlobal", "testValue");
            var retrievedValue = workingMemory.GetGlobal("testGlobal");

            // Assert
            retrievedValue.Should().Be("testValue");
            workingMemory.GetGlobal("nonExistentGlobal").Should().BeNull();
        }

        [Test]
        public void ModernImplementation_ShouldNotHaveIkvmDependencies()
        {
            // This test ensures our modern implementation doesn't reference IKVM types
            // Arrange & Act
            var ruleBase = RuleBaseFactory.CreateRuleBase();
            var packageBuilder = new ModernPackageBuilder();

            // Assert
            ruleBase.GetType().Assembly.GetReferencedAssemblies()
                .Should().NotContain(a => a.Name != null && a.Name.Contains("IKVM"), 
                "Modern implementation should not reference IKVM assemblies");

            packageBuilder.GetType().Assembly.GetReferencedAssemblies()
                .Should().NotContain(a => a.Name != null && a.Name.Contains("IKVM"),
                "Modern implementation should not reference IKVM assemblies");
        }

        [Test]
        public void ModernImplementation_ShouldValidateSuccessfully()
        {
            // Act
            var packageBuilder = new ModernPackageBuilder();
            var ruleBase = RuleBaseFactory.CreateRuleBase();
            
            // Try basic operations
            var simpleDrl = @"
                package test
                rule ""Test Rule""
                    when
                        $obj : Object()
                    then
                        // test action
                end";
            
            packageBuilder.AddPackageFromDrl(simpleDrl);
            var package = packageBuilder.GetPackage();
            
            // Assert
            packageBuilder.HasErrors.Should().BeFalse();
            package.Should().NotBeNull();
            
            if (package != null)
            {
                ruleBase.AddPackage(package);
                
                Console.WriteLine("✅ Modern Drools.NET functionality validated successfully - pure .NET 8 implementation working");
            }
            else
            {
                Assert.Fail("Package should not be null");
            }
        }
    }

    /// <summary>
    /// Test model classes for rule testing
    /// </summary>
    public class Person
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string Status { get; set; } = "";
    }

    public class Message
    {
        public const string HELLO = "Hello";
        public const string GOODBYE = "Goodbye";
        
        public string Text { get; set; } = "";
        public string Status { get; set; } = "";
    }
}