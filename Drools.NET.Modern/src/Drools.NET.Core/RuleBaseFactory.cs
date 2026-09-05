using System;

namespace Drools.NET.Core
{
    /// <summary>
    /// Factory class for creating rule base instances
    /// </summary>
    public static class RuleBaseFactory
    {
        /// <summary>
        /// Creates a new rule base with default configuration
        /// </summary>
        /// <returns>A new rule base instance</returns>
        public static IRuleBase CreateRuleBase()
        {
            return CreateRuleBase(RuleBaseConfiguration.Default);
        }

        /// <summary>
        /// Creates a new rule base with the specified configuration
        /// </summary>
        /// <param name="configuration">The configuration to use</param>
        /// <returns>A new rule base instance</returns>
        public static IRuleBase CreateRuleBase(IRuleBaseConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            return new Implementation.RuleBase(configuration);
        }

        /// <summary>
        /// Creates a new rule base with RETE network optimization
        /// </summary>
        /// <returns>A new optimized rule base instance</returns>
        public static IRuleBase CreateReteRuleBase()
        {
            var config = new RuleBaseConfiguration
            {
                AlgorithmType = RuleBaseAlgorithmType.Rete,
                ConflictResolver = ConflictResolverType.Default,
                EnabledStatistics = true
            };
            return CreateRuleBase(config);
        }

        /// <summary>
        /// Creates a new rule base optimized for sequential execution
        /// </summary>
        /// <returns>A new sequential rule base instance</returns>
        public static IRuleBase CreateSequentialRuleBase()
        {
            var config = new RuleBaseConfiguration
            {
                AlgorithmType = RuleBaseAlgorithmType.Sequential,
                ConflictResolver = ConflictResolverType.Simplicity,
                EnabledStatistics = false
            };
            return CreateRuleBase(config);
        }
    }

    /// <summary>
    /// Configuration for rule base behavior
    /// </summary>
    public interface IRuleBaseConfiguration
    {
        /// <summary>
        /// The algorithm type to use for rule execution
        /// </summary>
        RuleBaseAlgorithmType AlgorithmType { get; set; }

        /// <summary>
        /// The conflict resolution strategy
        /// </summary>
        ConflictResolverType ConflictResolver { get; set; }

        /// <summary>
        /// Whether to maintain execution statistics
        /// </summary>
        bool EnabledStatistics { get; set; }

        /// <summary>
        /// Whether to allow duplicate rule names across packages
        /// </summary>
        bool AllowDuplicateRuleNames { get; set; }

        /// <summary>
        /// Maximum number of rule executions per session (prevents infinite loops)
        /// </summary>
        int MaxRuleExecutions { get; set; }

        /// <summary>
        /// Whether to enable event support
        /// </summary>
        bool EventProcessingMode { get; set; }

        /// <summary>
        /// Creates a copy of this configuration
        /// </summary>
        /// <returns>A new configuration instance with the same settings</returns>
        IRuleBaseConfiguration Clone();
    }

    /// <summary>
    /// Default implementation of rule base configuration
    /// </summary>
    public class RuleBaseConfiguration : IRuleBaseConfiguration
    {
        public RuleBaseAlgorithmType AlgorithmType { get; set; } = RuleBaseAlgorithmType.Rete;
        public ConflictResolverType ConflictResolver { get; set; } = ConflictResolverType.Default;
        public bool EnabledStatistics { get; set; } = true;
        public bool AllowDuplicateRuleNames { get; set; } = false;
        public int MaxRuleExecutions { get; set; } = 10000;
        public bool EventProcessingMode { get; set; } = false;

        /// <summary>
        /// Default configuration instance
        /// </summary>
        public static IRuleBaseConfiguration Default => new RuleBaseConfiguration();

        public IRuleBaseConfiguration Clone()
        {
            return new RuleBaseConfiguration
            {
                AlgorithmType = AlgorithmType,
                ConflictResolver = ConflictResolver,
                EnabledStatistics = EnabledStatistics,
                AllowDuplicateRuleNames = AllowDuplicateRuleNames,
                MaxRuleExecutions = MaxRuleExecutions,
                EventProcessingMode = EventProcessingMode
            };
        }
    }

    /// <summary>
    /// Rule base algorithm types
    /// </summary>
    public enum RuleBaseAlgorithmType
    {
        /// <summary>
        /// RETE algorithm (default) - best for complex rule sets with many facts
        /// </summary>
        Rete,

        /// <summary>
        /// Sequential algorithm - best for simple rule sets or batch processing
        /// </summary>
        Sequential,

        /// <summary>
        /// Phreak algorithm - optimized RETE variant
        /// </summary>
        Phreak
    }

    /// <summary>
    /// Conflict resolution strategies
    /// </summary>
    public enum ConflictResolverType
    {
        /// <summary>
        /// Default strategy: Salience, then LIFO, then complexity
        /// </summary>
        Default,

        /// <summary>
        /// Salience only
        /// </summary>
        Salience,

        /// <summary>
        /// Simplicity (fewer conditions first)
        /// </summary>
        Simplicity,

        /// <summary>
        /// Complexity (more conditions first)
        /// </summary>
        Complexity,

        /// <summary>
        /// First In, First Out
        /// </summary>
        FIFO,

        /// <summary>
        /// Last In, First Out
        /// </summary>
        LIFO
    }
}