using System;

namespace Drools.NET.Core
{
    /// <summary>
    /// Base class for working memory event arguments
    /// </summary>
    public abstract class WorkingMemoryEventArgs : EventArgs
    {
        protected WorkingMemoryEventArgs(IWorkingMemory workingMemory, IFactHandle factHandle)
        {
            WorkingMemory = workingMemory ?? throw new ArgumentNullException(nameof(workingMemory));
            FactHandle = factHandle ?? throw new ArgumentNullException(nameof(factHandle));
        }

        /// <summary>
        /// The working memory where the event occurred
        /// </summary>
        public IWorkingMemory WorkingMemory { get; }

        /// <summary>
        /// The fact handle involved in the event
        /// </summary>
        public IFactHandle FactHandle { get; }

        /// <summary>
        /// The object involved in the event
        /// </summary>
        public object Object => FactHandle.Object;
    }

    /// <summary>
    /// Event arguments for object assertion events
    /// </summary>
    public class ObjectAssertedEventArgs : WorkingMemoryEventArgs
    {
        public ObjectAssertedEventArgs(IWorkingMemory workingMemory, IFactHandle factHandle, bool dynamic = false)
            : base(workingMemory, factHandle)
        {
            Dynamic = dynamic;
        }

        /// <summary>
        /// Whether dynamic property tracking was enabled for this assertion
        /// </summary>
        public bool Dynamic { get; }
    }

    /// <summary>
    /// Event arguments for object retraction events
    /// </summary>
    public class ObjectRetractedEventArgs : WorkingMemoryEventArgs
    {
        public ObjectRetractedEventArgs(IWorkingMemory workingMemory, IFactHandle factHandle, object? oldObject = null)
            : base(workingMemory, factHandle)
        {
            OldObject = oldObject;
        }

        /// <summary>
        /// The object that was retracted (may be different from current FactHandle.Object if modified)
        /// </summary>
        public object? OldObject { get; }
    }

    /// <summary>
    /// Event arguments for object modification events
    /// </summary>
    public class ObjectModifiedEventArgs : WorkingMemoryEventArgs
    {
        public ObjectModifiedEventArgs(IWorkingMemory workingMemory, IFactHandle factHandle, object? oldObject = null)
            : base(workingMemory, factHandle)
        {
            OldObject = oldObject;
        }

        /// <summary>
        /// The object before modification
        /// </summary>
        public object? OldObject { get; }

        /// <summary>
        /// The object after modification
        /// </summary>
        public object NewObject => FactHandle.Object;
    }

    /// <summary>
    /// Event arguments for before rule fired events
    /// </summary>
    public class BeforeRuleFiredEventArgs : EventArgs
    {
        public BeforeRuleFiredEventArgs(IWorkingMemory workingMemory, IRule rule, IActivation activation)
        {
            WorkingMemory = workingMemory ?? throw new ArgumentNullException(nameof(workingMemory));
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
            Activation = activation ?? throw new ArgumentNullException(nameof(activation));
        }

        /// <summary>
        /// The working memory where the rule is firing
        /// </summary>
        public IWorkingMemory WorkingMemory { get; }

        /// <summary>
        /// The rule that is about to fire
        /// </summary>
        public IRule Rule { get; }

        /// <summary>
        /// The activation that triggered the rule
        /// </summary>
        public IActivation Activation { get; }
    }

    /// <summary>
    /// Event arguments for after rule fired events
    /// </summary>
    public class AfterRuleFiredEventArgs : EventArgs
    {
        public AfterRuleFiredEventArgs(IWorkingMemory workingMemory, IRule rule, IActivation activation, Exception? exception = null)
        {
            WorkingMemory = workingMemory ?? throw new ArgumentNullException(nameof(workingMemory));
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
            Activation = activation ?? throw new ArgumentNullException(nameof(activation));
            Exception = exception;
        }

        /// <summary>
        /// The working memory where the rule fired
        /// </summary>
        public IWorkingMemory WorkingMemory { get; }

        /// <summary>
        /// The rule that fired
        /// </summary>
        public IRule Rule { get; }

        /// <summary>
        /// The activation that triggered the rule
        /// </summary>
        public IActivation Activation { get; }

        /// <summary>
        /// Exception that occurred during rule execution, if any
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Whether the rule execution succeeded
        /// </summary>
        public bool Success => Exception == null;
    }
}