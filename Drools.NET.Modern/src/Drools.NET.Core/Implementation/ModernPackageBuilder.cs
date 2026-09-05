using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Drools.NET.Core.Implementation
{
    /// <summary>
    /// Modern .NET 8 implementation of PackageBuilder, eliminating IKVM dependencies
    /// Based on decompiled org.drools.compiler.PackageBuilder
    /// </summary>
    public class ModernPackageBuilder : IPackageBuilder
    {
        private readonly List<IPackage> _packages = new();
        private readonly List<ICompilationError> _errors = new();
        private readonly List<ICompilationError> _warnings = new();
        private readonly IPackageBuilderConfiguration _configuration;
        private readonly DrlParser _drlParser;

        public ModernPackageBuilder() : this(PackageBuilderConfiguration.Default)
        {
        }

        public ModernPackageBuilder(IPackageBuilderConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _drlParser = new DrlParser(_configuration);
        }

        public IReadOnlyList<IPackage> Packages => _packages.AsReadOnly();
        public bool HasErrors => _errors.Count > 0;
        public IReadOnlyList<ICompilationError> Errors => _errors.AsReadOnly();
        public IReadOnlyList<ICompilationError> Warnings => _warnings.AsReadOnly();
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
                // Parse DRL content into PackageDescr
                var packageDescr = _drlParser.Parse(drlContent, resourceName);
                
                // Add parser errors to our error collection
                foreach (var error in _drlParser.Errors)
                {
                    _errors.Add(error);
                    ErrorOccurred?.Invoke(this, new CompilationErrorEventArgs(error));
                }

                if (packageDescr != null && !HasErrors)
                {
                    // Validate package
                    ValidatePackageName(packageDescr);
                    ValidateUniqueRuleNames(packageDescr);

                    // Create package from descriptor
                    var package = CreatePackage(packageDescr);
                    if (package != null)
                    {
                        _packages.Add(package);
                    }
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
                var decisionTableCompiler = new DecisionTableCompiler();
                var drlContent = decisionTableCompiler.Compile(dtableStream, inputType);
                AddPackageFromDrl(resourceName, drlContent);
            }
            catch (Exception ex)
            {
                AddError($"Failed to compile decision table: {ex.Message}", resourceName, exception: ex);
            }
        }

        public IPackage? GetPackage()
        {
            return _packages.FirstOrDefault(p => p.IsValid);
        }

        public void Clear()
        {
            _packages.Clear();
            _errors.Clear();
            _warnings.Clear();
        }

        private void ValidatePackageName(PackageDescriptor packageDescr)
        {
            if (string.IsNullOrWhiteSpace(packageDescr.Name))
            {
                throw new ArgumentException("Missing package name for rule package.");
            }
        }

        private void ValidateUniqueRuleNames(PackageDescriptor packageDescr)
        {
            var ruleNames = new HashSet<string>();
            var duplicateRules = new List<string>();

            foreach (var ruleDescr in packageDescr.Rules)
            {
                if (!ruleNames.Add(ruleDescr.Name))
                {
                    duplicateRules.Add(ruleDescr.Name);
                }
            }

            if (duplicateRules.Count > 0)
            {
                AddError($"Duplicate rule names found: {string.Join(", ", duplicateRules)}");
            }
        }

        private IPackage CreatePackage(PackageDescriptor packageDescr)
        {
            var package = new Package(packageDescr.Name);

            try
            {
                // Add imports
                foreach (var import in packageDescr.Imports)
                {
                    package.AddImport(import);
                }

                // Add default imports from configuration
                foreach (var import in _configuration.DefaultImports)
                {
                    package.AddImport(import);
                }

                // Add globals
                foreach (var global in packageDescr.Globals)
                {
                    package.AddGlobal(new Global(global.Name, global.Type, global.DefaultValue));
                }

                // Add functions
                foreach (var function in packageDescr.Functions)
                {
                    package.AddFunction(CreateFunction(function));
                }

                // Add rules
                foreach (var ruleDescr in packageDescr.Rules)
                {
                    var rule = CreateRule(ruleDescr, package.Name);
                    if (rule != null)
                    {
                        package.AddRule(rule);
                    }
                }

                return package;
            }
            catch (Exception ex)
            {
                AddError($"Failed to create package '{packageDescr.Name}': {ex.Message}", exception: ex);
                return null;
            }
        }

        private IFunction CreateFunction(FunctionDescriptor functionDescr)
        {
            // This would create a compiled function from the descriptor
            // For now, return a placeholder
            return new PlaceholderFunction(functionDescr.Name, typeof(object));
        }

        private IRule CreateRule(RuleDescriptor ruleDescr, string packageName)
        {
            try
            {
                // Compile the LHS (conditions)
                var leftHandSide = CompileConditions(ruleDescr.Lhs);
                
                // Compile the RHS (consequences)  
                var rightHandSide = CompileConsequence(ruleDescr.Rhs);

                return new ModernRule(
                    name: ruleDescr.Name,
                    packageName: packageName,
                    salience: ruleDescr.Salience,
                    leftHandSide: leftHandSide,
                    rightHandSide: rightHandSide,
                    enabled: ruleDescr.Enabled,
                    agendaGroup: ruleDescr.AgendaGroup,
                    activationGroup: ruleDescr.ActivationGroup,
                    noLoop: ruleDescr.NoLoop,
                    lockOnActive: ruleDescr.LockOnActive,
                    autoFocus: ruleDescr.AutoFocus,
                    dateEffective: ruleDescr.DateEffective,
                    dateExpires: ruleDescr.DateExpires,
                    duration: ruleDescr.Duration,
                    source: ruleDescr.Source
                );
            }
            catch (Exception ex)
            {
                AddError($"Failed to compile rule '{ruleDescr.Name}': {ex.Message}", exception: ex);
                return null;
            }
        }

        private ICondition CompileConditions(LeftHandSideDescriptor lhsDescr)
        {
            // This would compile the actual DRL conditions into executable form
            // For demonstration, return a simple condition
            return new ModernCondition(lhsDescr);
        }

        private IConsequence CompileConsequence(RightHandSideDescriptor rhsDescr)
        {
            // This would compile the actual DRL consequence code into executable form
            // For demonstration, return a simple consequence
            return new ModernConsequence(rhsDescr);
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

    /// <summary>
    /// Placeholder function implementation
    /// </summary>
    internal class PlaceholderFunction : IFunction
    {
        public PlaceholderFunction(string name, Type returnType)
        {
            Name = name;
            ReturnType = returnType;
            ParameterTypes = Array.Empty<Type>();
        }

        public string Name { get; }
        public string? Namespace => null;
        public Type ReturnType { get; }
        public IReadOnlyList<Type> ParameterTypes { get; }
        public bool IsValid => true;

        public object? Invoke(params object?[] arguments)
        {
            // Placeholder implementation
            return null;
        }
    }
}