using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Drools.NET.Core.Implementation;

namespace Drools.NET.Core
{
    /// <summary>
    /// Default implementation of IPackageBuilder
    /// </summary>
    public class PackageBuilder : IPackageBuilder
    {
        private readonly List<IPackage> _packages = new();
        private readonly List<ICompilationError> _errors = new();
        private readonly List<ICompilationError> _warnings = new();
        private readonly IPackageBuilderConfiguration _configuration;

        public PackageBuilder() : this(PackageBuilderConfiguration.Default)
        {
        }

        public PackageBuilder(IPackageBuilderConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IReadOnlyList<IPackage> Packages => _packages.ToList();
        
        public bool HasErrors => _errors.Count > 0;
        
        public IReadOnlyList<ICompilationError> Errors => _errors.ToList();
        
        public IReadOnlyList<ICompilationError> Warnings => _warnings.ToList();
        
        public IPackageBuilderConfiguration Configuration => _configuration;

        public event EventHandler<CompilationErrorEventArgs>? ErrorOccurred;
        public event EventHandler<CompilationErrorEventArgs>? WarningOccurred;

        public void AddPackageFromDrl(Stream drlStream)
        {
            AddPackageFromDrl("stream", drlStream);
        }

        public void AddPackageFromDrl(string resourceName, Stream drlStream)
        {
            if (drlStream == null) throw new ArgumentNullException(nameof(drlStream));
            
            using var reader = new StreamReader(drlStream);
            var drlContent = reader.ReadToEnd();
            AddPackageFromDrl(resourceName, drlContent);
        }

        public async Task AddPackageFromDrlAsync(string resourceName, Stream drlStream, CancellationToken cancellationToken = default)
        {
            if (drlStream == null) throw new ArgumentNullException(nameof(drlStream));
            
            using var reader = new StreamReader(drlStream);
            var drlContent = await reader.ReadToEndAsync();
            AddPackageFromDrl(resourceName, drlContent);
        }

        public void AddPackageFromDrl(string drlContent)
        {
            AddPackageFromDrl("inline", drlContent);
        }

        public void AddPackageFromDrl(string resourceName, string drlContent)
        {
            if (string.IsNullOrEmpty(drlContent))
            {
                AddError("DRL content cannot be null or empty", resourceName);
                return;
            }

            try
            {
                // This is a basic implementation - a full implementation would include
                // a complete DRL parser (ANTLR-based or similar)
                var package = ParseDrlContent(resourceName, drlContent);
                if (package != null)
                {
                    _packages.Add(package);
                }
            }
            catch (Exception ex)
            {
                AddError($"Failed to parse DRL content: {ex.Message}", resourceName, exception: ex);
            }
        }

        public void AddPackageFromDecisionTable(Stream dtableStream, DecisionTableInputType inputType)
        {
            AddPackageFromDecisionTable("decision-table", dtableStream, inputType);
        }

        public void AddPackageFromDecisionTable(string resourceName, Stream dtableStream, DecisionTableInputType inputType)
        {
            if (dtableStream == null) throw new ArgumentNullException(nameof(dtableStream));
            
            try
            {
                // This would implement decision table compilation
                // For now, add a placeholder error
                AddError($"Decision table compilation not yet implemented for {inputType}", resourceName);
            }
            catch (Exception ex)
            {
                AddError($"Failed to compile decision table: {ex.Message}", resourceName, exception: ex);
            }
        }

        public IPackage? GetPackage()
        {
            // Return the first valid package, or null if none exist
            return _packages.FirstOrDefault(p => p.IsValid);
        }

        public void Clear()
        {
            _packages.Clear();
            _errors.Clear();
            _warnings.Clear();
        }

        /// <summary>
        /// Basic DRL parser - this is a simplified implementation
        /// A production version would use ANTLR or similar for full DRL syntax support
        /// </summary>
        private IPackage? ParseDrlContent(string resourceName, string drlContent)
        {
            // This is a very basic parser that looks for package declarations
            // A real implementation would have full DRL grammar support
            
            var lines = drlContent.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrEmpty(l) && !l.StartsWith("//"))
                .ToArray();

            string packageName = "default";
            
            // Look for package declaration
            foreach (var line in lines)
            {
                if (line.StartsWith("package "))
                {
                    packageName = line.Substring(8).Trim().TrimEnd(';');
                    break;
                }
            }

            var package = new Package(packageName);

            // For now, create a simple example rule to demonstrate the structure
            // A full implementation would parse the complete DRL syntax
            if (drlContent.Contains("rule "))
            {
                try
                {
                    var exampleRule = CreateExampleRule(packageName, drlContent);
                    package.AddRule(exampleRule);
                }
                catch (Exception ex)
                {
                    AddError($"Failed to parse rule: {ex.Message}", resourceName, exception: ex);
                }
            }

            // Add default imports
            foreach (var import in _configuration.DefaultImports)
            {
                package.AddImport(import);
            }

            return package;
        }

        /// <summary>
        /// Creates an example rule for demonstration purposes
        /// A real implementation would parse the actual DRL syntax
        /// </summary>
        private IRule CreateExampleRule(string packageName, string drlContent)
        {
            // This is a placeholder - real implementation would parse actual rule syntax
            return new SimpleRule(
                name: "ExampleRule",
                packageName: packageName,
                salience: 0,
                leftHandSide: new SimpleCondition(),
                rightHandSide: new SimpleConsequence()
            );
        }

        private void AddError(string message, string? resource = null, int line = 0, int column = 0, string? code = null, Exception? exception = null)
        {
            var error = new CompilationError(message, true, resource, line, column, code, exception);
            _errors.Add(error);
            ErrorOccurred?.Invoke(this, new CompilationErrorEventArgs(error));
        }

        private void AddWarning(string message, string? resource = null, int line = 0, int column = 0, string? code = null)
        {
            var warning = new CompilationError(message, false, resource, line, column, code);
            _warnings.Add(warning);
            WarningOccurred?.Invoke(this, new CompilationErrorEventArgs(warning));
        }
    }
}