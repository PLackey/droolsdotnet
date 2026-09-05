using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Drools.NET.Core.Implementation
{
    /// <summary>
    /// Default implementation of IRuleBase
    /// </summary>
    internal class RuleBase : IRuleBase
    {
        private readonly ConcurrentDictionary<string, IPackage> _packages = new();
        private readonly IRuleBaseConfiguration _configuration;

        public RuleBase(IRuleBaseConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IReadOnlyList<IPackage> Packages => _packages.Values.ToList();
        
        public IRuleBaseConfiguration Configuration => _configuration;

        public event EventHandler<PackageEventArgs>? PackageAdded;
        public event EventHandler<PackageEventArgs>? PackageRemoved;

        public void AddPackage(IPackage package)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));

            if (!package.IsValid)
            {
                throw new ArgumentException($"Package '{package.Name}' is not valid. Errors: {string.Join(", ", package.Errors)}");
            }

            // Check for duplicate rule names if not allowed
            if (!_configuration.AllowDuplicateRuleNames)
            {
                var existingRuleNames = _packages.Values
                    .SelectMany(p => p.Rules)
                    .Select(r => r.Name)
                    .ToHashSet();

                var duplicateRules = package.Rules
                    .Where(r => existingRuleNames.Contains(r.Name))
                    .Select(r => r.Name)
                    .ToList();

                if (duplicateRules.Any())
                {
                    throw new ArgumentException($"Duplicate rule names found: {string.Join(", ", duplicateRules)}");
                }
            }

            _packages[package.Name] = package;
            PackageAdded?.Invoke(this, new PackageEventArgs(package));
        }

        public void RemovePackage(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
                return;

            if (_packages.TryRemove(packageName, out var package))
            {
                PackageRemoved?.Invoke(this, new PackageEventArgs(package));
            }
        }

        public IWorkingMemory CreateWorkingMemory()
        {
            return new WorkingMemory(this, _configuration);
        }

        public IEnumerable<IRule> GetAllRules()
        {
            return _packages.Values.SelectMany(p => p.Rules);
        }

        public IPackage? GetPackage(string name)
        {
            _packages.TryGetValue(name, out var package);
            return package;
        }

        public override string ToString()
        {
            var ruleCount = _packages.Values.Sum(p => p.Rules.Count);
            return $"RuleBase - {_packages.Count} packages, {ruleCount} rules";
        }
    }
}