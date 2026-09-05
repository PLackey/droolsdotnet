using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Drools.NET.Core.Implementation
{
    /// <summary>
    /// Default implementation of IPackage
    /// </summary>
    internal class Package : IPackage
    {
        private readonly ConcurrentDictionary<string, IRule> _rules = new();
        private readonly ConcurrentDictionary<string, IFunction> _functions = new();
        private readonly ConcurrentDictionary<string, IGlobal> _globals = new();
        private readonly ConcurrentDictionary<string, ITypeDeclaration> _typeDeclarations = new();
        private readonly List<string> _imports = new();
        private readonly List<string> _errors = new();

        public Package(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }
        
        public IReadOnlyList<IRule> Rules => _rules.Values.ToList();
        
        public IReadOnlyDictionary<string, IFunction> Functions => _functions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
        public IReadOnlyDictionary<string, IGlobal> Globals => _globals.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
        public IReadOnlyList<string> Imports 
        { 
            get 
            { 
                lock (_imports) 
                { 
                    return _imports.ToList(); 
                } 
            } 
        }
        
        public IReadOnlyDictionary<string, ITypeDeclaration> TypeDeclarations => _typeDeclarations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
        public bool IsValid => _errors.Count == 0 && _rules.Count > 0;
        
        public IReadOnlyList<string> Errors 
        { 
            get 
            { 
                lock (_errors) 
                { 
                    return _errors.ToList(); 
                } 
            } 
        }

        public event EventHandler<RuleEventArgs>? RuleAdded;
        public event EventHandler<RuleEventArgs>? RuleRemoved;

        public void AddRule(IRule rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            
            if (_rules.ContainsKey(rule.Name))
            {
                throw new ArgumentException($"Rule '{rule.Name}' already exists in package '{Name}'");
            }

            _rules[rule.Name] = rule;
            RuleAdded?.Invoke(this, new RuleEventArgs(rule));
        }

        public bool RemoveRule(string ruleName)
        {
            if (string.IsNullOrEmpty(ruleName))
                return false;

            if (_rules.TryRemove(ruleName, out var rule))
            {
                RuleRemoved?.Invoke(this, new RuleEventArgs(rule));
                return true;
            }

            return false;
        }

        public IRule? GetRule(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            _rules.TryGetValue(name, out var rule);
            return rule;
        }

        public void AddFunction(IFunction function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            
            _functions[function.Name] = function;
        }

        public void AddGlobal(IGlobal global)
        {
            if (global == null) throw new ArgumentNullException(nameof(global));
            
            _globals[global.Name] = global;
        }

        public void AddImport(string import)
        {
            if (string.IsNullOrWhiteSpace(import))
                throw new ArgumentException("Import cannot be null or empty", nameof(import));

            lock (_imports)
            {
                if (!_imports.Contains(import))
                {
                    _imports.Add(import);
                }
            }
        }

        public void AddTypeDeclaration(ITypeDeclaration typeDeclaration)
        {
            if (typeDeclaration == null) throw new ArgumentNullException(nameof(typeDeclaration));
            
            _typeDeclarations[typeDeclaration.Name] = typeDeclaration;
        }

        internal void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                lock (_errors)
                {
                    _errors.Add(error);
                }
            }
        }

        public override string ToString()
        {
            return $"Package[{Name}] - {_rules.Count} rules, {_functions.Count} functions";
        }
    }
}