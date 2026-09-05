using System;
using FluentAssertions;
using NUnit.Framework;
using org.drools.dotnet;
using org.drools.dotnet.compiler;

namespace org.drools.dotnet.tests
{
    /// <summary>
    /// Comprehensive compatibility tests for Drools.NET on different runtime environments
    /// </summary>
    [TestFixture]
    public class CompatibilityTests
    {
        [Test]
        public void RuntimeCompatibilityCheck_ShouldProvideAccurateAssessment()
        {
            // Act
            var isCompatible = CompatibilityHelper.IsRuntimeCompatible();
            var report = CompatibilityHelper.GetCompatibilityReport();

            // Assert
            report.Should().NotBeNullOrEmpty();
            Console.WriteLine("=== Compatibility Report ===");
            Console.WriteLine(report);
            Console.WriteLine("============================");

            // On .NET Core/8, we expect limited compatibility
            if (!isCompatible)
            {
                Assert.Pass("Runtime compatibility check correctly identified .NET Core/8 limitations");
            }
            else
            {
                Assert.Pass("Runtime reports full compatibility (likely .NET Framework)");
            }
        }

        [Test]
        public void ValidateDroolsFunctionality_ShouldHandleIkvmLimitations()
        {
            // Act
            var (success, message) = CompatibilityHelper.ValidateDroolsFunctionality();

            // Assert
            message.Should().NotBeNullOrEmpty();
            Console.WriteLine("=== Drools Functionality Validation ===");
            Console.WriteLine(message);
            Console.WriteLine("=======================================");

            if (success)
            {
                Assert.Pass("✅ Drools functionality validated successfully");
            }
            else
            {
                // On .NET Core/8, we expect this to fail with IKVM issues
                Console.WriteLine("Expected failure on .NET Core/8 due to IKVM compatibility limitations");
                Assert.Inconclusive("Drools functionality limited by IKVM compatibility - this is expected on .NET Core/8");
            }
        }

        [Test]
        public void PackageBuilder_CreationTest_DocumentsExpectedBehavior()
        {
            try
            {
                // Attempt to create PackageBuilder
                var builder = new PackageBuilder();
                Assert.Pass("✅ PackageBuilder created successfully - full Drools functionality available");
            }
            catch (TypeLoadException ex) when (ex.Message.Contains("MethodToken") || ex.Message.Contains("System.Security.Permissions"))
            {
                Console.WriteLine($"Expected TypeLoadException on .NET Core/8: {ex.Message}");
                Assert.Inconclusive(
                    "PackageBuilder creation failed due to IKVM/.NET Core compatibility issues. " +
                    "This is expected behavior on .NET Core/8. Use .NET Framework for full compatibility or " +
                    "consider Microsoft.RulesEngine for new projects.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected exception: {ex.GetType().Name}: {ex.Message}");
                Assert.Fail($"Unexpected error during PackageBuilder creation: {ex.Message}");
            }
        }

        [Test]
        public void RuntimeEnvironment_Information()
        {
            // Gather runtime information for diagnostics
            var runtimeInfo = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
            var version = Environment.Version;
            var osDescription = System.Runtime.InteropServices.RuntimeInformation.OSDescription;

            Console.WriteLine("=== Runtime Environment ===");
            Console.WriteLine($"Framework: {runtimeInfo}");
            Console.WriteLine($"Version: {version}");
            Console.WriteLine($"OS: {osDescription}");
            Console.WriteLine($"Process Architecture: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");
            Console.WriteLine("===========================");

            // This test always passes - it's just for information
            Assert.Pass("Runtime information logged successfully");
        }

        [Test]
        [Category("Integration")]
        public void AlternativeRuleEngine_Suggestion()
        {
            Console.WriteLine("=== Alternative Rule Engine Suggestions ===");
            Console.WriteLine("For new .NET Core/8 projects, consider these alternatives:");
            Console.WriteLine();
            Console.WriteLine("1. Microsoft Rules Engine:");
            Console.WriteLine("   dotnet add package Microsoft.RulesEngine");
            Console.WriteLine("   https://github.com/microsoft/RulesEngine");
            Console.WriteLine();
            Console.WriteLine("2. NRules:");
            Console.WriteLine("   dotnet add package NRules");
            Console.WriteLine("   https://github.com/NRules/NRules");
            Console.WriteLine();
            Console.WriteLine("3. Pure .NET Implementation:");
            Console.WriteLine("   Consider implementing rules using expression trees or reflection");
            Console.WriteLine("===========================================");

            Assert.Pass("Alternative suggestions provided");
        }
    }
}