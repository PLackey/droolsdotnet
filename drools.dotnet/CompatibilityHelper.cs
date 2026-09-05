using System;

namespace org.drools.dotnet
{
    /// <summary>
    /// Provides compatibility helpers and guidance for .NET 8 usage
    /// </summary>
    public static class CompatibilityHelper
    {
        /// <summary>
        /// Checks if the current runtime is compatible with IKVM-based Drools functionality
        /// </summary>
        public static bool IsRuntimeCompatible()
        {
            // Check if we're running on .NET Framework (compatible) or .NET Core (limited compatibility)
            return Environment.Version.Major < 5; // .NET Framework versions are < 5
        }

        /// <summary>
        /// Gets a compatibility report for the current runtime
        /// </summary>
        public static string GetCompatibilityReport()
        {
            var frameworkVersion = Environment.Version;
            var runtimeVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;

            if (IsRuntimeCompatible())
            {
                return $"✅ Runtime Compatible: {runtimeVersion}";
            }
            else
            {
                return $"""
                ⚠️  Runtime Compatibility Issue: {runtimeVersion}
                
                The legacy IKVM-based Drools library has limited compatibility with .NET Core/8.
                
                Known Issues:
                - PackageBuilder initialization may fail
                - Runtime errors with System.Reflection.Emit.MethodToken
                - IKVM.Runtime compatibility issues
                
                Recommendations:
                1. Use Microsoft.RulesEngine for new projects
                2. Consider migrating to pure .NET rule engine implementations
                3. Use this library for API compatibility testing only
                
                For more information, see: https://github.com/your-repo/droolsdotnet#compatibility
                """;
            }
        }

        /// <summary>
        /// Attempts to validate Drools functionality and provides helpful error messages
        /// </summary>
        public static (bool Success, string Message) ValidateDroolsFunctionality()
        {
            try
            {
                // Try to create a modern PackageBuilder and RuleBase to test basic functionality
                var packageBuilder = new Implementation.ModernPackageBuilder();
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
                
                if (package != null)
                {
                    ruleBase.AddPackage(package);
                    return (true, "✅ Modern Drools.NET functionality validated successfully - pure .NET 8 implementation working");
                }
                else
                {
                    return (false, "❌ Package compilation failed in modern implementation");
                }
            }
            catch (TypeLoadException ex) when (ex.Message.Contains("MethodToken") || ex.Message.Contains("mscorlib"))
            {
                return (false, $"""
                ❌ IKVM Compatibility Issue: {ex.Message}
                
                This error occurs because the legacy IKVM library is trying to access
                .NET Framework-specific types that don't exist in .NET Core/8.
                
                Solutions:
                1. Use a .NET Framework target instead of .NET 8
                2. Use the modern Drools.NET implementation (Drools.NET.Modern package)
                3. Use Microsoft.RulesEngine: dotnet add package Microsoft.RulesEngine
                4. Implement rules using pure .NET logic
                """);
            }
            catch (Exception ex)
            {
                return (false, $"❌ Unexpected error: {ex.Message}\n\nSee compatibility documentation for guidance.");
            }
        }
    }
}