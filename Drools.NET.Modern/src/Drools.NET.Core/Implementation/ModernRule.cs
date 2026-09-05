using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Drools.NET.Core.Implementation
{
    /// <summary>
    /// Modern .NET 8 implementation of IRule
    /// </summary>
    internal class ModernRule : IRule
    {
        private readonly Dictionary<string, object> _attributes = new();

        public ModernRule(
            string name,
            string packageName,
            int salience,
            ICondition leftHandSide,
            IConsequence rightHandSide,
            bool enabled = true,
            string? agendaGroup = null,
            string? activationGroup = null,
            bool noLoop = false,
            bool lockOnActive = false,
            bool autoFocus = false,
            DateTime? dateEffective = null,
            DateTime? dateExpires = null,
            TimeSpan? duration = null,
            string? source = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PackageName = packageName ?? throw new ArgumentNullException(nameof(packageName));
            Salience = salience;
            LeftHandSide = leftHandSide ?? throw new ArgumentNullException(nameof(leftHandSide));
            RightHandSide = rightHandSide ?? throw new ArgumentNullException(nameof(rightHandSide));
            Enabled = enabled;
            AgendaGroup = agendaGroup;
            ActivationGroup = activationGroup;
            NoLoop = noLoop;
            LockOnActive = lockOnActive;
            AutoFocus = autoFocus;
            DateEffective = dateEffective;
            DateExpires = dateExpires;
            Duration = duration;
            Source = source;
        }

        public string Name { get; }
        public string PackageName { get; }
        public int Salience { get; }
        public bool Enabled { get; set; }
        public bool AutoFocus { get; }
        public string? AgendaGroup { get; }
        public string? ActivationGroup { get; }
        public string? RuleFlowGroup { get; }
        public DateTime? DateEffective { get; }
        public DateTime? DateExpires { get; }
        public bool NoLoop { get; }
        public TimeSpan? Duration { get; }
        public bool LockOnActive { get; }
        public ICondition LeftHandSide { get; }
        public IConsequence RightHandSide { get; }
        public IReadOnlyDictionary<string, object> Attributes => _attributes;
        public bool IsValid => Enabled && LeftHandSide.IsValid && RightHandSide.IsValid && IsDateValid();
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
            if (!IsValid)
                return Enumerable.Empty<IActivation>();

            try
            {
                var tuples = LeftHandSide.Evaluate(workingMemory);
                return tuples.Select(tuple => new ModernActivation(this, tuple, workingMemory));
            }
            catch (Exception)
            {
                // Log exception in a real implementation
                return Enumerable.Empty<IActivation>();
            }
        }

        public void Execute(IActivation activation)
        {
            if (!IsValid)
                throw new InvalidOperationException($"Rule '{Name}' is not valid and cannot be executed");

            try
            {
                RightHandSide.Execute(activation);
            }
            catch (Exception ex)
            {
                throw new RuleExecutionException($"Error executing rule '{Name}': {ex.Message}", ex);
            }
        }

        private bool IsDateValid()
        {
            var now = DateTime.UtcNow;
            
            if (DateEffective.HasValue && now < DateEffective.Value)
                return false;
                
            if (DateExpires.HasValue && now > DateExpires.Value)
                return false;
                
            return true;
        }

        public override string ToString()
        {
            return $"Rule[{Name}] (salience: {Salience}, enabled: {Enabled})";
        }
    }

    /// <summary>
    /// Modern .NET 8 implementation of ICondition
    /// </summary>
    internal class ModernCondition : ICondition
    {
        private readonly LeftHandSideDescriptor _descriptor;
        private readonly List<IDeclaration> _declarations = new();

        public ModernCondition(LeftHandSideDescriptor descriptor)
        {
            _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            // In a real implementation, this would parse the LHS and extract declarations
        }

        public IReadOnlyList<IDeclaration> RequiredDeclarations => _declarations;
        public bool IsValid => !string.IsNullOrWhiteSpace(_descriptor.Content);

        public IEnumerable<ITuple> Evaluate(IWorkingMemory workingMemory)
        {
            // This is a simplified implementation
            // A real implementation would:
            // 1. Parse the condition patterns
            // 2. Match against facts in working memory using RETE algorithm
            // 3. Return matching tuples

            // For demonstration, match all facts (equivalent to "Object()")
            var allFacts = workingMemory.FactHandles.ToList();
            if (allFacts.Count == 0)
                return Enumerable.Empty<ITuple>();

            // Create a simple tuple for each fact
            return allFacts.Select(fh => new ModernTuple(new[] { fh }));
        }
    }

    /// <summary>
    /// Modern .NET 8 implementation of IConsequence
    /// </summary>
    internal class ModernConsequence : IConsequence
    {
        private readonly RightHandSideDescriptor _descriptor;

        public ModernConsequence(RightHandSideDescriptor descriptor)
        {
            _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        }

        public IReadOnlyList<IDeclaration> RequiredDeclarations { get; } = new List<IDeclaration>();
        public bool IsValid => !string.IsNullOrWhiteSpace(_descriptor.Content);
        public string? Source => _descriptor.Content;

        public void Execute(IActivation activation)
        {
            // This is a simplified implementation
            // A real implementation would:
            // 1. Compile the consequence code to executable form
            // 2. Execute with proper context (facts, globals, etc.)
            // 3. Handle modify/retract/insert operations

            // For demonstration, just log the execution
            Console.WriteLine($"Executing rule '{activation.Rule.Name}' with {activation.Tuple.Size} facts");
            
            // Example of accessing facts in the tuple
            foreach (var fact in activation.Tuple.Facts)
            {
                Console.WriteLine($"  Fact: {fact}");
            }
        }
    }

    /// <summary>
    /// Modern .NET 8 implementation of IActivation
    /// </summary>
    internal class ModernActivation : IActivation
    {
        public ModernActivation(IRule rule, ITuple tuple, IWorkingMemory workingMemory)
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

        public override bool Equals(object? obj)
        {
            return obj is ModernActivation other && ActivationNumber == other.ActivationNumber;
        }

        public override int GetHashCode()
        {
            return ActivationNumber.GetHashCode();
        }

        public override string ToString()
        {
            return $"Activation[{ActivationNumber}] Rule: {Rule.Name}";
        }
    }

    /// <summary>
    /// Modern .NET 8 implementation of ITuple
    /// </summary>
    internal class ModernTuple : ITuple
    {
        private readonly Dictionary<string, int> _declarationIndexes = new();

        public ModernTuple(IReadOnlyList<IFactHandle> factHandles)
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

        public void BindDeclaration(string name, int index)
        {
            if (index >= 0 && index < FactHandles.Count)
            {
                _declarationIndexes[name] = index;
            }
        }

        public override string ToString()
        {
            return $"Tuple[{Size}] Facts: {string.Join(", ", Facts.Select(f => f?.GetType().Name ?? "null"))}";
        }
    }

    /// <summary>
    /// Exception thrown when rule execution fails
    /// </summary>
    public class RuleExecutionException : Exception
    {
        public RuleExecutionException(string message) : base(message) { }
        public RuleExecutionException(string message, Exception innerException) : base(message, innerException) { }
    }
}