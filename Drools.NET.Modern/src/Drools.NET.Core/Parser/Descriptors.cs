using System;
using System.Collections.Generic;

namespace Drools.NET.Core.Implementation
{
    /// <summary>
    /// Describes a package during compilation
    /// </summary>
    internal class PackageDescriptor
    {
        public string Name { get; set; } = "";
        public List<string> Imports { get; } = new();
        public List<GlobalDescriptor> Globals { get; } = new();
        public List<FunctionDescriptor> Functions { get; } = new();
        public List<RuleDescriptor> Rules { get; } = new();
        public List<TypeDescriptor> Types { get; } = new();
    }

    /// <summary>
    /// Describes a rule during compilation
    /// </summary>
    internal class RuleDescriptor
    {
        public string Name { get; set; } = "";
        public LeftHandSideDescriptor Lhs { get; set; } = new(string.Empty);
        public RightHandSideDescriptor Rhs { get; set; } = new(string.Empty);
        public int Salience { get; set; }
        public bool Enabled { get; set; } = true;
        public string? AgendaGroup { get; set; }
        public string? ActivationGroup { get; set; }
        public bool NoLoop { get; set; }
        public bool LockOnActive { get; set; }
        public bool AutoFocus { get; set; }
        public DateTime? DateEffective { get; set; }
        public DateTime? DateExpires { get; set; }
        public TimeSpan? Duration { get; set; }
        public string? Source { get; set; }
        public Dictionary<string, object> Attributes { get; } = new();
    }

    /// <summary>
    /// Describes the left-hand side (conditions) of a rule
    /// </summary>
    internal class LeftHandSideDescriptor
    {
        public LeftHandSideDescriptor(string content)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public string Content { get; }
        public List<PatternDescriptor> Patterns { get; } = new();
        public List<DeclarationDescriptor> Declarations { get; } = new();
    }

    /// <summary>
    /// Describes the right-hand side (consequences) of a rule
    /// </summary>
    internal class RightHandSideDescriptor
    {
        public RightHandSideDescriptor(string content)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public string Content { get; }
        public string Dialect { get; set; } = "csharp";
    }

    /// <summary>
    /// Describes a pattern in the LHS
    /// </summary>
    internal class PatternDescriptor
    {
        public PatternDescriptor(string objectType)
        {
            ObjectType = objectType ?? throw new ArgumentNullException(nameof(objectType));
        }

        public string ObjectType { get; }
        public string? Identifier { get; set; }
        public List<ConstraintDescriptor> Constraints { get; } = new();
    }

    /// <summary>
    /// Describes a constraint within a pattern
    /// </summary>
    internal class ConstraintDescriptor
    {
        public ConstraintDescriptor(string expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public string Expression { get; }
        public ConstraintType Type { get; set; } = ConstraintType.Predicate;
    }

    /// <summary>
    /// Types of constraints
    /// </summary>
    internal enum ConstraintType
    {
        Predicate,
        Literal,
        ReturnValue,
        Variable
    }

    /// <summary>
    /// Describes a variable declaration
    /// </summary>
    internal class DeclarationDescriptor
    {
        public DeclarationDescriptor(string name, string type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            TypeName = type ?? throw new ArgumentNullException(nameof(type));
        }

        public string Name { get; }
        public string TypeName { get; }
        public Type? ResolvedType { get; set; }
    }

    /// <summary>
    /// Describes a global variable
    /// </summary>
    internal class GlobalDescriptor
    {
        public GlobalDescriptor(string name, Type type, object? defaultValue = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            DefaultValue = defaultValue;
        }

        public string Name { get; }
        public Type Type { get; }
        public object? DefaultValue { get; }
    }

    /// <summary>
    /// Describes a function
    /// </summary>
    internal class FunctionDescriptor
    {
        public FunctionDescriptor(string name, string returnType, string parameters)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public string Name { get; }
        public string ReturnType { get; }
        public string Parameters { get; }
        public string? Body { get; set; }
    }

    /// <summary>
    /// Describes a type declaration
    /// </summary>
    internal class TypeDescriptor
    {
        public TypeDescriptor(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }
        public List<FieldDescriptor> Fields { get; } = new();
        public Dictionary<string, object> Metadata { get; } = new();
    }

    /// <summary>
    /// Describes a field in a type declaration
    /// </summary>
    internal class FieldDescriptor
    {
        public FieldDescriptor(string name, string type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            TypeName = type ?? throw new ArgumentNullException(nameof(type));
        }

        public string Name { get; }
        public string TypeName { get; }
        public object? InitialValue { get; set; }
        public bool IsKey { get; set; }
    }
}