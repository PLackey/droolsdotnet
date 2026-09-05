using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Drools.NET.Core.Implementation
{
    /// <summary>
    /// Default implementation of IWorkingMemory
    /// </summary>
    internal class WorkingMemory : IWorkingMemory
    {
        private readonly ConcurrentDictionary<long, IFactHandle> _facts = new();
        private readonly ConcurrentDictionary<string, object> _globals = new();
        private readonly IRuleBase _ruleBase;
        private readonly IRuleBaseConfiguration _configuration;
        private readonly IAgenda _agenda;
        private bool _disposed;

        public WorkingMemory(IRuleBase ruleBase, IRuleBaseConfiguration configuration)
        {
            _ruleBase = ruleBase ?? throw new ArgumentNullException(nameof(ruleBase));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _agenda = new Agenda(this, configuration);

            // Initialize globals from packages
            foreach (var package in ruleBase.Packages)
            {
                foreach (var global in package.Globals)
                {
                    if (global.Value.DefaultValue != null)
                    {
                        _globals[global.Key] = global.Value.DefaultValue;
                    }
                }
            }
        }

        public IRuleBase RuleBase => _ruleBase;
        
        public IReadOnlyCollection<object> Objects => _facts.Values.Select(fh => fh.Object).ToList();
        
        public IReadOnlyCollection<IFactHandle> FactHandles => _facts.Values.ToList();
        
        public IDictionary<string, object> Globals => _globals.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        public event EventHandler<ObjectAssertedEventArgs>? ObjectAsserted;
        public event EventHandler<ObjectRetractedEventArgs>? ObjectRetracted;
        public event EventHandler<ObjectModifiedEventArgs>? ObjectModified;
        public event EventHandler<BeforeRuleFiredEventArgs>? BeforeRuleFired;
        public event EventHandler<AfterRuleFiredEventArgs>? AfterRuleFired;

        public IFactHandle AssertObject(object fact)
        {
            return AssertObject(fact, false);
        }

        public IFactHandle AssertObject(object fact, bool dynamic)
        {
            if (fact == null) throw new ArgumentNullException(nameof(fact));
            CheckDisposed();

            var factHandle = new FactHandle(fact);
            _facts[factHandle.Id] = factHandle;

            // If dynamic, set up property change notifications
            if (dynamic && fact is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged += (sender, e) =>
                {
                    if (sender == fact && _facts.ContainsKey(factHandle.Id))
                    {
                        PropagateModification(factHandle);
                    }
                };
            }

            PropagateAssertion(factHandle, dynamic);
            ObjectAsserted?.Invoke(this, new ObjectAssertedEventArgs(this, factHandle, dynamic));

            return factHandle;
        }

        public void RetractObject(IFactHandle handle)
        {
            if (handle == null) throw new ArgumentNullException(nameof(handle));
            CheckDisposed();

            if (_facts.TryRemove(handle.Id, out var factHandle))
            {
                PropagateRetraction(factHandle);
                ObjectRetracted?.Invoke(this, new ObjectRetractedEventArgs(this, factHandle));
            }
        }

        public void ModifyObject(IFactHandle handle, object fact)
        {
            if (handle == null) throw new ArgumentNullException(nameof(handle));
            if (fact == null) throw new ArgumentNullException(nameof(fact));
            CheckDisposed();

            if (_facts.TryGetValue(handle.Id, out var factHandle) && factHandle is FactHandle mutableHandle)
            {
                var oldObject = mutableHandle.Object;
                mutableHandle.Object = fact;
                
                PropagateModification(factHandle);
                ObjectModified?.Invoke(this, new ObjectModifiedEventArgs(this, factHandle, oldObject));
            }
        }

        public int FireAllRules()
        {
            return FireAllRules(_configuration.MaxRuleExecutions);
        }

        public int FireAllRules(int limit)
        {
            CheckDisposed();
            
            int firedCount = 0;
            int executionCount = 0;

            while (firedCount < limit && executionCount < _configuration.MaxRuleExecutions)
            {
                var activation = _agenda.GetNextActivation();
                if (activation == null)
                    break;

                try
                {
                    BeforeRuleFired?.Invoke(this, new BeforeRuleFiredEventArgs(this, activation.Rule, activation));
                    
                    activation.Rule.Execute(activation);
                    firedCount++;
                    
                    AfterRuleFired?.Invoke(this, new AfterRuleFiredEventArgs(this, activation.Rule, activation));
                }
                catch (Exception ex)
                {
                    AfterRuleFired?.Invoke(this, new AfterRuleFiredEventArgs(this, activation.Rule, activation, ex));
                    throw; // Re-throw to maintain exception behavior
                }
                finally
                {
                    executionCount++;
                }
            }

            return firedCount;
        }

        public async Task<int> FireAllRulesAsync(CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => FireAllRules(), cancellationToken);
        }

        public void Clear()
        {
            CheckDisposed();
            
            var handles = _facts.Keys.ToList();
            foreach (var id in handles)
            {
                if (_facts.TryRemove(id, out var factHandle))
                {
                    ObjectRetracted?.Invoke(this, new ObjectRetractedEventArgs(this, factHandle));
                }
            }
            
            _agenda.Clear();
        }

        public void SetGlobal(string name, object value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Global name cannot be null or empty", nameof(name));
            CheckDisposed();
            
            _globals[name] = value;
        }

        public object? GetGlobal(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            CheckDisposed();
            
            _globals.TryGetValue(name, out var value);
            return value;
        }

        private void PropagateAssertion(IFactHandle factHandle, bool dynamic)
        {
            // Find all matching patterns and create activations
            foreach (var package in _ruleBase.Packages)
            {
                foreach (var rule in package.Rules)
                {
                    if (rule.Enabled)
                    {
                        var activations = rule.Match(this);
                        foreach (var activation in activations)
                        {
                            _agenda.AddActivation(activation);
                        }
                    }
                }
            }
        }

        private void PropagateRetraction(IFactHandle factHandle)
        {
            // Remove activations that depend on this fact
            _agenda.RemoveActivationsForFact(factHandle);
        }

        private void PropagateModification(IFactHandle factHandle)
        {
            // Remove old activations and add new ones
            PropagateRetraction(factHandle);
            PropagateAssertion(factHandle, false);
        }

        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WorkingMemory));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Clear();
                _disposed = true;
            }
        }
    }
}