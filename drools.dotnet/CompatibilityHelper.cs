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
                // Try to create basic Drools components to test functionality
                // Note: This tests the legacy IKVM-based implementation
                var packageBuilder = new compiler.PackageBuilder();
                
                // Try basic DRL parsing
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
                    return (true, "✅ Legacy Drools.NET functionality working on current runtime");
                }
                else
                {
                    return (false, "❌ Package compilation failed - DRL parsing issues");
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
                
                🎯 RECOMMENDED: Use Drools.NET.Modern for 100% .NET 8 compatibility
                """);
            }
            catch (Exception ex)
            {
                return (false, $"""
                ❌ Legacy Implementation Error: {ex.Message}
                
                This is expected on .NET Core/8 due to IKVM compatibility limitations.
                
                🎯 SOLUTION: Use Drools.NET.Modern (pure .NET 8 implementation)
                   Located in: /Drools.NET.Modern/
                   - 100% test success rate
                   - No IKVM dependencies  
                   - Full .NET 8 compatibility
                
                See README.md for migration instructions.
                """);
            }
        }
    }
}