using System;
using System.Collections.Generic;

namespace Drools.NET.Core
{
    /// <summary>
    /// Represents a compiled rule with conditions (LHS) and consequences (RHS)
    /// </summary>
    public interface IRule
    {
        /// <summary>
        /// The name of this rule
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The package this rule belongs to
        /// </summary>
        string PackageName { get; }

        /// <summary>
        /// The salience (priority) of this rule - higher values have higher priority
        /// </summary>
        int Salience { get; }

        /// <summary>
        /// Whether this rule is enabled
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Whether this rule should automatically focus on its agenda group when activated
        /// </summary>
        bool AutoFocus { get; }

        /// <summary>
        /// The agenda group this rule belongs to
        /// </summary>
        string? AgendaGroup { get; }

        /// <summary>
        /// The activation group this rule belongs to (mutual exclusion)
        /// </summary>
        string? ActivationGroup { get; }

        /// <summary>
        /// The rule flow group this rule belongs to
        /// </summary>
        string? RuleFlowGroup { get; }

        /// <summary>
        /// Date after which this rule becomes effective
        /// </summary>
        DateTime? DateEffective { get; }

        /// <summary>
        /// Date after which this rule expires
        /// </summary>
        DateTime? DateExpires { get; }

        /// <summary>
        /// Whether no-loop is enabled (prevents infinite recursion)
        /// </summary>
        bool NoLoop { get; }

        /// <summary>
        /// Duration for temporal rules (timer-based activation)
        /// </summary>
        TimeSpan? Duration { get; }

        /// <summary>
        /// Lock-on-active setting for rule activation
        /// </summary>
        bool LockOnActive { get; }

        /// <summary>
        /// The left-hand side (conditions) of this rule
        /// </summary>
        ICondition LeftHandSide { get; }

        /// <summary>
        /// The right-hand side (consequences) of this rule
        /// </summary>
        IConsequence RightHandSide { get; }

        /// <summary>
        /// Metadata attributes associated with this rule
        /// </summary>
        IReadOnlyDictionary<string, object> Attributes { get; }

        /// <summary>
        /// Gets an attribute value
        /// </summary>
        /// <param name="name">Attribute name</param>
        /// <returns>Attribute value, or null if not found</returns>
        object? GetAttribute(string name);

        /// <summary>
        /// Sets an attribute value
        /// </summary>
        /// <param name="name">Attribute name</param>
        /// <param name="value">Attribute value</param>
        void SetAttribute(string name, object value);

        /// <summary>
        /// Evaluates whether this rule matches the given facts
        /// </summary>
        /// <param name="workingMemory">The working memory containing facts</param>
        /// <returns>All matching activations</returns>
        IEnumerable<IActivation> Match(IWorkingMemory workingMemory);

        /// <summary>
        /// Executes the consequence (RHS) of this rule
        /// </summary>
        /// <param name="activation">The activation that triggered this rule</param>
        void Execute(IActivation activation);

        /// <summary>
        /// Whether this rule is currently valid and can be executed
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Original DRL source code for this rule (if available)
        /// </summary>
        string? Source { get; }
    }

    /// <summary>
    /// Represents the left-hand side (condition) of a rule
    /// </summary>
    public interface ICondition
    {
        /// <summary>
        /// Evaluates this condition against working memory
        /// </summary>
        /// <param name="workingMemory">The working memory to evaluate against</param>
        /// <returns>All matching tuples</returns>
        IEnumerable<ITuple> Evaluate(IWorkingMemory workingMemory);

        /// <summary>
        /// The required declarations (variables) for this condition
        /// </summary>
        IReadOnlyList<IDeclaration> RequiredDeclarations { get; }

        /// <summary>
        /// Whether this condition can be evaluated
        /// </summary>
        bool IsValid { get; }
    }

    /// <summary>
    /// Represents the right-hand side (consequence) of a rule
    /// </summary>
    public interface IConsequence
    {
        /// <summary>
        /// Executes this consequence
        /// </summary>
        /// <param name="activation">The activation context</param>
        void Execute(IActivation activation);

        /// <summary>
        /// The required declarations (variables) for this consequence
        /// </summary>
        IReadOnlyList<IDeclaration> RequiredDeclarations { get; }

        /// <summary>
        /// Whether this consequence can be executed
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// The original consequence code
        /// </summary>
        string? Source { get; }
    }

    /// <summary>
    /// Represents a rule activation (matched condition with specific facts)
    /// </summary>
    public interface IActivation
    {
        /// <summary>
        /// The rule that was activated
        /// </summary>
        IRule Rule { get; }

        /// <summary>
        /// The tuple of facts that matched the rule conditions
        /// </summary>
        ITuple Tuple { get; }

        /// <summary>
        /// The working memory where this activation exists
        /// </summary>
        IWorkingMemory WorkingMemory { get; }

        /// <summary>
        /// The salience of this activation (from the rule)
        /// </summary>
        int Salience { get; }

        /// <summary>
        /// Whether this activation is still valid
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// When this activation was created
        /// </summary>
        DateTime CreatedTime { get; }

        /// <summary>
        /// The activation number (for conflict resolution)
        /// </summary>
        long ActivationNumber { get; }
    }

    /// <summary>
    /// Represents a tuple of facts that matched rule conditions
    /// </summary>
    public interface ITuple
    {
        /// <summary>
        /// The fact handles in this tuple
        /// </summary>
        IReadOnlyList<IFactHandle> FactHandles { get; }

        /// <summary>
        /// The facts in this tuple
        /// </summary>
        IReadOnlyList<object> Facts { get; }

        /// <summary>
        /// Gets a fact by declaration name
        /// </summary>
        /// <param name="declaration">The declaration name</param>
        /// <returns>The fact bound to that declaration</returns>
        object? GetFact(string declaration);

        /// <summary>
        /// Gets a fact handle by declaration name
        /// </summary>
        /// <param name="declaration">The declaration name</param>
        /// <returns>The fact handle bound to that declaration</returns>
        IFactHandle? GetFactHandle(string declaration);

        /// <summary>
        /// The size of this tuple
        /// </summary>
        int Size { get; }
    }

    /// <summary>
    /// Represents a variable declaration in a rule
    /// </summary>
    public interface IDeclaration
    {
        /// <summary>
        /// The name of the declared variable
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The type of the declared variable
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// The pattern this declaration is bound to
        /// </summary>
        IPattern Pattern { get; }

        /// <summary>
        /// Gets the value from a tuple
        /// </summary>
        /// <param name="tuple">The tuple to extract from</param>
        /// <returns>The value</returns>
        object? GetValue(ITuple tuple);
    }

    /// <summary>
    /// Represents a pattern in rule conditions
    /// </summary>
    public interface IPattern
    {
        /// <summary>
        /// The type this pattern matches
        /// </summary>
        Type ObjectType { get; }

        /// <summary>
        /// Constraints that must be satisfied
        /// </summary>
        IReadOnlyList<IConstraint> Constraints { get; }

        /// <summary>
        /// Tests whether an object matches this pattern
        /// </summary>
        /// <param name="obj">The object to test</param>
        /// <param name="workingMemory">The working memory context</param>
        /// <returns>True if the object matches</returns>
        bool Matches(object obj, IWorkingMemory workingMemory);
    }

    /// <summary>
    /// Represents a constraint within a pattern
    /// </summary>
    public interface IConstraint
    {
        /// <summary>
        /// Evaluates this constraint against an object
        /// </summary>
        /// <param name="obj">The object to evaluate</param>
        /// <param name="workingMemory">The working memory context</param>
        /// <returns>True if the constraint is satisfied</returns>
        bool IsSatisfied(object obj, IWorkingMemory workingMemory);

        /// <summary>
        /// The required declarations for this constraint
        /// </summary>
        IReadOnlyList<IDeclaration> RequiredDeclarations { get; }
    }
}