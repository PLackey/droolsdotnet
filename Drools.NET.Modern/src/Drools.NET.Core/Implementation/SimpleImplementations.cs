using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Drools.NET.Core.Implementation
{
    /// <summary>
    /// Simple rule implementation for basic functionality
    /// </summary>
    internal class SimpleRule : IRule
    {
        private readonly Dictionary<string, object> _attributes = new();

        public SimpleRule(string name, string packageName, int salience, ICondition leftHandSide, IConsequence rightHandSide)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PackageName = packageName ?? throw new ArgumentNullException(nameof(packageName));
            Salience = salience;
            LeftHandSide = leftHandSide ?? throw new ArgumentNullException(nameof(leftHandSide));
            RightHandSide = rightHandSide ?? throw new ArgumentNullException(nameof(rightHandSide));
        }

        public string Name { get; }
        public string PackageName { get; }
        public int Salience { get; }
        public bool Enabled { get; set; } = true;
        public bool AutoFocus { get; } = false;
        public string? AgendaGroup { get; } = null;
        public string? ActivationGroup { get; } = null;
        public string? RuleFlowGroup { get; } = null;
        public DateTime? DateEffective { get; } = null;
        public DateTime? DateExpires { get; } = null;
        public bool NoLoop { get; } = false;
        public TimeSpan? Duration { get; } = null;
        public bool LockOnActive { get; } = false;
        public ICondition LeftHandSide { get; }
        public IConsequence RightHandSide { get; }
        public IReadOnlyDictionary<string, object> Attributes => _attributes;
        public bool IsValid => LeftHandSide.IsValid && RightHandSide.IsValid;
        public string? Source { get; }

        public object? GetAttribute(string name)
        {
            _attributes.TryGetValue(name, out var value);
            return value;
        }

        public void SetAttribute(string name, object value)
        {
            _attributes[name] = value;
        }

        public IEnumerable<IActivation> Match(IWorkingMemory workingMemory)
        {
            var tuples = LeftHandSide.Evaluate(workingMemory);
            return tuples.Select(tuple => new SimpleActivation(this, tuple, workingMemory));
        }

        public void Execute(IActivation activation)
        {
            RightHandSide.Execute(activation);
        }
    }

    /// <summary>
    /// Simple condition that matches all objects (for demonstration)
    /// </summary>
    internal class SimpleCondition : ICondition
    {
        public IReadOnlyList<IDeclaration> RequiredDeclarations { get; } = new List<IDeclaration>();
        public bool IsValid => true;

        public IEnumerable<ITuple> Evaluate(IWorkingMemory workingMemory)
        {
            // For demonstration, create a tuple for each fact
            // A real implementation would apply actual pattern matching
            return workingMemory.FactHandles.Select(fh => new SimpleTuple(new[] { fh }));
        }
    }

    /// <summary>
    /// Simple consequence that does nothing (for demonstration)
    /// </summary>
    internal class SimpleConsequence : IConsequence
    {
        public IReadOnlyList<IDeclaration> RequiredDeclarations { get; } = new List<IDeclaration>();
        public bool IsValid => true;
        public string? Source => "// Simple consequence - no action";

        public void Execute(IActivation activation)
        {
            // For demonstration, just increment a counter or log
            // A real implementation would execute the actual consequence code
            Console.WriteLine($"Rule {activation.Rule.Name} fired with {activation.Tuple.Size} facts");
        }
    }

    /// <summary>
    /// Simple activation implementation
    /// </summary>
    internal class SimpleActivation : IActivation
    {
        public SimpleActivation(IRule rule, ITuple tuple, IWorkingMemory workingMemory)
        {
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
            Tuple = tuple ?? throw new ArgumentNullException(nameof(tuple));
            WorkingMemory = workingMemory ?? throw new ArgumentNullException(nameof(workingMemory));
            CreatedTime = DateTime.UtcNow;
            ActivationNumber = ActivationCounter.GetNext();
        }

        public IRule Rule { get; }
        public ITuple Tuple { get; }
        public IWorkingMemory WorkingMemory { get; }
        public int Salience => Rule.Salience;
        public bool IsValid => Rule.Enabled && Rule.IsValid;
        public DateTime CreatedTime { get; }
        public long ActivationNumber { get; }
    }

    /// <summary>
    /// Simple tuple implementation
    /// </summary>
    internal class SimpleTuple : ITuple
    {
        private readonly Dictionary<string, int> _declarationIndexes = new();

        public SimpleTuple(IReadOnlyList<IFactHandle> factHandles)
        {
            FactHandles = factHandles ?? throw new ArgumentNullException(nameof(factHandles));
            Facts = factHandles.Select(fh => fh.Object).ToList();
        }

        public IReadOnlyList<IFactHandle> FactHandles { get; }
        public IReadOnlyList<object> Facts { get; }
        public int Size => FactHandles.Count;

        public object? GetFact(string declaration)
        {
            if (_declarationIndexes.TryGetValue(declaration, out var index) && index < Facts.Count)
            {
                return Facts[index];
            }
            return null;
        }

        public IFactHandle? GetFactHandle(string declaration)
        {
            if (_declarationIndexes.TryGetValue(declaration, out var index) && index < FactHandles.Count)
            {
                return FactHandles[index];
            }
            return null;
        }

        internal void BindDeclaration(string name, int index)
        {
            _declarationIndexes[name] = index;
        }
    }

    /// <summary>
    /// Simple pattern implementation
    /// </summary>
    internal class SimplePattern : IPattern
    {
        public SimplePattern(Type objectType)
        {
            ObjectType = objectType ?? throw new ArgumentNullException(nameof(objectType));
            Constraints = new List<IConstraint>();
        }

        public Type ObjectType { get; }
        public IReadOnlyList<IConstraint> Constraints { get; }

        public bool Matches(object obj, IWorkingMemory workingMemory)
        {
            if (obj == null) return false;
            
            // Check type compatibility
            if (!ObjectType.IsAssignableFrom(obj.GetType()))
                return false;

            // Check all constraints
            return Constraints.All(constraint => constraint.IsSatisfied(obj, workingMemory));
        }
    }

    /// <summary>
    /// Simple constraint implementation
    /// </summary>
    internal class SimpleConstraint : IConstraint
    {
        private readonly Func<object, IWorkingMemory, bool> _predicate;

        public SimpleConstraint(Func<object, IWorkingMemory, bool> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            RequiredDeclarations = new List<IDeclaration>();
        }

        public IReadOnlyList<IDeclaration> RequiredDeclarations { get; }

        public bool IsSatisfied(object obj, IWorkingMemory workingMemory)
        {
            return _predicate(obj, workingMemory);
        }
    }

    /// <summary>
    /// Simple declaration implementation
    /// </summary>
    internal class SimpleDeclaration : IDeclaration
    {
        public SimpleDeclaration(string name, Type type, IPattern pattern)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        }

        public string Name { get; }
        public Type Type { get; }
        public IPattern Pattern { get; }

        public object? GetValue(ITuple tuple)
        {
            return tuple.GetFact(Name);
        }
    }
}