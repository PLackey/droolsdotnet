using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Drools.NET.Core.Implementation
{
    /// <summary>
    /// Default implementation of IAgenda with conflict resolution
    /// </summary>
    internal class Agenda : IAgenda
    {
        private readonly ConcurrentDictionary<long, IActivation> _activations = new();
        private readonly IWorkingMemory _workingMemory;
        private readonly IRuleBaseConfiguration _configuration;
        private readonly object _lockObject = new();

        public Agenda(IWorkingMemory workingMemory, IRuleBaseConfiguration configuration)
        {
            _workingMemory = workingMemory ?? throw new ArgumentNullException(nameof(workingMemory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IReadOnlyList<IActivation> Activations => _activations.Values.ToList();
        
        public bool HasActivations => !_activations.IsEmpty;

        public void AddActivation(IActivation activation)
        {
            if (activation == null) throw new ArgumentNullException(nameof(activation));
            
            lock (_lockObject)
            {
                _activations[activation.ActivationNumber] = activation;
            }
        }

        public void RemoveActivation(IActivation activation)
        {
            if (activation == null) return;
            
            lock (_lockObject)
            {
                _activations.TryRemove(activation.ActivationNumber, out _);
            }
        }

        public void RemoveActivationsForFact(IFactHandle factHandle)
        {
            if (factHandle == null) return;
            
            lock (_lockObject)
            {
                var toRemove = _activations.Values
                    .Where(a => a.Tuple.FactHandles.Contains(factHandle))
                    .Select(a => a.ActivationNumber)
                    .ToList();

                foreach (var activationNumber in toRemove)
                {
                    _activations.TryRemove(activationNumber, out _);
                }
            }
        }

        public IActivation? GetNextActivation()
        {
            lock (_lockObject)
            {
                if (_activations.IsEmpty)
                    return null;

                // Apply conflict resolution strategy
                var sortedActivations = ApplyConflictResolution(_activations.Values);
                var nextActivation = sortedActivations.FirstOrDefault();

                if (nextActivation != null)
                {
                    _activations.TryRemove(nextActivation.ActivationNumber, out _);
                }

                return nextActivation;
            }
        }

        public void Clear()
        {
            lock (_lockObject)
            {
                _activations.Clear();
            }
        }

        private IEnumerable<IActivation> ApplyConflictResolution(IEnumerable<IActivation> activations)
        {
            var validActivations = activations.Where(a => a.IsValid).ToList();
            
            return _configuration.ConflictResolver switch
            {
                ConflictResolverType.Salience => validActivations.OrderByDescending(a => a.Salience)
                    .ThenByDescending(a => a.ActivationNumber),
                    
                ConflictResolverType.Simplicity => validActivations.OrderBy(a => a.Rule.LeftHandSide.RequiredDeclarations.Count)
                    .ThenByDescending(a => a.Salience)
                    .ThenByDescending(a => a.ActivationNumber),
                    
                ConflictResolverType.Complexity => validActivations.OrderByDescending(a => a.Rule.LeftHandSide.RequiredDeclarations.Count)
                    .ThenByDescending(a => a.Salience)
                    .ThenByDescending(a => a.ActivationNumber),
                    
                ConflictResolverType.FIFO => validActivations.OrderBy(a => a.ActivationNumber),
                
                ConflictResolverType.LIFO => validActivations.OrderByDescending(a => a.ActivationNumber),
                
                ConflictResolverType.Default or _ => validActivations
                    .OrderByDescending(a => a.Salience)
                    .ThenByDescending(a => a.ActivationNumber)
                    .ThenByDescending(a => a.Rule.LeftHandSide.RequiredDeclarations.Count)
            };
        }
    }
}