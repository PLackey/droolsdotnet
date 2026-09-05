using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Drools.NET.Core
{
    /// <summary>
    /// Represents a working memory session where facts are asserted and rules are executed
    /// </summary>
    public interface IWorkingMemory : IDisposable
    {
        /// <summary>
        /// Asserts a fact into working memory
        /// </summary>
        /// <param name="fact">The fact to assert</param>
        /// <returns>A handle to the asserted fact</returns>
        IFactHandle AssertObject(object fact);

        /// <summary>
        /// Asserts a fact into working memory with dynamic tracking
        /// </summary>
        /// <param name="fact">The fact to assert</param>
        /// <param name="dynamic">Whether to enable dynamic property change tracking</param>
        /// <returns>A handle to the asserted fact</returns>
        IFactHandle AssertObject(object fact, bool dynamic);

        /// <summary>
        /// Retracts a fact from working memory
        /// </summary>
        /// <param name="handle">The handle of the fact to retract</param>
        void RetractObject(IFactHandle handle);

        /// <summary>
        /// Modifies a fact in working memory
        /// </summary>
        /// <param name="handle">The handle of the fact to modify</param>
        /// <param name="fact">The modified fact</param>
        void ModifyObject(IFactHandle handle, object fact);

        /// <summary>
        /// Gets all facts currently in working memory
        /// </summary>
        IReadOnlyCollection<object> Objects { get; }

        /// <summary>
        /// Gets all fact handles currently in working memory
        /// </summary>
        IReadOnlyCollection<IFactHandle> FactHandles { get; }

        /// <summary>
        /// Fires all activated rules
        /// </summary>
        /// <returns>The number of rules that fired</returns>
        int FireAllRules();

        /// <summary>
        /// Fires rules with a maximum limit
        /// </summary>
        /// <param name="limit">Maximum number of rules to fire</param>
        /// <returns>The number of rules that fired</returns>
        int FireAllRules(int limit);

        /// <summary>
        /// Fires all activated rules asynchronously
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation, with the number of rules that fired</returns>
        Task<int> FireAllRulesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Clears all facts from working memory
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the rule base associated with this working memory
        /// </summary>
        IRuleBase RuleBase { get; }

        /// <summary>
        /// Gets global variables available to rules
        /// </summary>
        IDictionary<string, object> Globals { get; }

        /// <summary>
        /// Sets a global variable
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="value">Variable value</param>
        void SetGlobal(string name, object value);

        /// <summary>
        /// Gets a global variable
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <returns>Variable value</returns>
        object? GetGlobal(string name);

        /// <summary>
        /// Event fired when an object is asserted
        /// </summary>
        event EventHandler<ObjectAssertedEventArgs>? ObjectAsserted;

        /// <summary>
        /// Event fired when an object is retracted
        /// </summary>
        event EventHandler<ObjectRetractedEventArgs>? ObjectRetracted;

        /// <summary>
        /// Event fired when an object is modified
        /// </summary>
        event EventHandler<ObjectModifiedEventArgs>? ObjectModified;

        /// <summary>
        /// Event fired before a rule is fired
        /// </summary>
        event EventHandler<BeforeRuleFiredEventArgs>? BeforeRuleFired;

        /// <summary>
        /// Event fired after a rule is fired
        /// </summary>
        event EventHandler<AfterRuleFiredEventArgs>? AfterRuleFired;
    }

    /// <summary>
    /// Represents a handle to a fact in working memory
    /// </summary>
    public interface IFactHandle
    {
        /// <summary>
        /// Unique identifier for this fact handle
        /// </summary>
        long Id { get; }

        /// <summary>
        /// The fact object associated with this handle
        /// </summary>
        object Object { get; }

        /// <summary>
        /// Recency counter for conflict resolution
        /// </summary>
        long Recency { get; }
    }
}