using System;
using System.Collections.Generic;

namespace Drools.NET.Core
{
    /// <summary>
    /// Represents a package of compiled rules and related components
    /// </summary>
    public interface IPackage
    {
        /// <summary>
        /// The name of this package
        /// </summary>
        string Name { get; }

        /// <summary>
        /// All rules in this package
        /// </summary>
        IReadOnlyList<IRule> Rules { get; }

        /// <summary>
        /// All functions defined in this package
        /// </summary>
        IReadOnlyDictionary<string, IFunction> Functions { get; }

        /// <summary>
        /// All global variables defined in this package
        /// </summary>
        IReadOnlyDictionary<string, IGlobal> Globals { get; }

        /// <summary>
        /// All imports defined in this package
        /// </summary>
        IReadOnlyList<string> Imports { get; }

        /// <summary>
        /// Type declarations in this package
        /// </summary>
        IReadOnlyDictionary<string, ITypeDeclaration> TypeDeclarations { get; }

        /// <summary>
        /// Adds a rule to this package
        /// </summary>
        /// <param name="rule">The rule to add</param>
        void AddRule(IRule rule);

        /// <summary>
        /// Removes a rule from this package
        /// </summary>
        /// <param name="ruleName">The name of the rule to remove</param>
        /// <returns>True if the rule was found and removed</returns>
        bool RemoveRule(string ruleName);

        /// <summary>
        /// Gets a rule by name
        /// </summary>
        /// <param name="name">The rule name</param>
        /// <returns>The rule, or null if not found</returns>
        IRule? GetRule(string name);

        /// <summary>
        /// Adds a function to this package
        /// </summary>
        /// <param name="function">The function to add</param>
        void AddFunction(IFunction function);

        /// <summary>
        /// Adds a global variable definition to this package
        /// </summary>
        /// <param name="global">The global variable definition</param>
        void AddGlobal(IGlobal global);

        /// <summary>
        /// Adds an import to this package
        /// </summary>
        /// <param name="import">The import statement</param>
        void AddImport(string import);

        /// <summary>
        /// Whether this package is valid and ready for execution
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Any errors that occurred during package compilation
        /// </summary>
        IReadOnlyList<string> Errors { get; }

        /// <summary>
        /// Event fired when a rule is added to this package
        /// </summary>
        event EventHandler<RuleEventArgs>? RuleAdded;

        /// <summary>
        /// Event fired when a rule is removed from this package
        /// </summary>
        event EventHandler<RuleEventArgs>? RuleRemoved;
    }

    /// <summary>
    /// Event arguments for rule-related events
    /// </summary>
    public class RuleEventArgs : EventArgs
    {
        public RuleEventArgs(IRule rule)
        {
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
        }

        public IRule Rule { get; }
    }
}