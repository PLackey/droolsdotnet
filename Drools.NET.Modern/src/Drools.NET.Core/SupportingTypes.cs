using System;
using System.Collections.Generic;
using System.Linq;

namespace Drools.NET.Core
{
    /// <summary>
    /// Represents a function that can be called from rules
    /// </summary>
    public interface IFunction
    {
        /// <summary>
        /// The name of the function
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The namespace the function belongs to
        /// </summary>
        string? Namespace { get; }

        /// <summary>
        /// Return type of the function
        /// </summary>
        Type ReturnType { get; }

        /// <summary>
        /// Parameter types
        /// </summary>
        IReadOnlyList<Type> ParameterTypes { get; }

        /// <summary>
        /// Invokes the function with the given arguments
        /// </summary>
        /// <param name="arguments">Function arguments</param>
        /// <returns>Function result</returns>
        object? Invoke(params object?[] arguments);

        /// <summary>
        /// Whether this function is valid and can be invoked
        /// </summary>
        bool IsValid { get; }
    }

    /// <summary>
    /// Represents a global variable declaration
    /// </summary>
    public interface IGlobal
    {
        /// <summary>
        /// The name of the global variable
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The type of the global variable
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// The default value (if any)
        /// </summary>
        object? DefaultValue { get; }
    }

    /// <summary>
    /// Represents a type declaration in a package
    /// </summary>
    public interface ITypeDeclaration
    {
        /// <summary>
        /// The name of the declared type
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The .NET type this declaration represents
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Fields declared for this type
        /// </summary>
        IReadOnlyList<IFieldDeclaration> Fields { get; }

        /// <summary>
        /// Metadata attributes
        /// </summary>
        IReadOnlyDictionary<string, object> Attributes { get; }
    }

    /// <summary>
    /// Represents a field declaration within a type
    /// </summary>
    public interface IFieldDeclaration
    {
        /// <summary>
        /// The name of the field
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The type of the field
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// The default value (if any)
        /// </summary>
        object? DefaultValue { get; }

        /// <summary>
        /// Whether this field is a key field
        /// </summary>
        bool IsKey { get; }
    }

    /// <summary>
    /// Default implementation of IGlobal
    /// </summary>
    public class Global : IGlobal
    {
        public Global(string name, Type type, object? defaultValue = null)
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
    /// Default implementation of IFunction for .NET methods
    /// </summary>
    public class DotNetFunction : IFunction
    {
        private readonly System.Reflection.MethodInfo _methodInfo;
        private readonly object? _target;

        public DotNetFunction(System.Reflection.MethodInfo methodInfo, object? target = null)
        {
            _methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            _target = target;
            
            Name = methodInfo.Name;
            Namespace = methodInfo.DeclaringType?.Namespace;
            ReturnType = methodInfo.ReturnType;
            ParameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToList();
        }

        public string Name { get; }
        public string? Namespace { get; }
        public Type ReturnType { get; }
        public IReadOnlyList<Type> ParameterTypes { get; }
        public bool IsValid => _methodInfo != null;

        public object? Invoke(params object?[] arguments)
        {
            try
            {
                return _methodInfo.Invoke(_target, arguments);
            }
            catch (System.Reflection.TargetParameterCountException ex)
            {
                throw new ArgumentException($"Function {Name} expects {ParameterTypes.Count} parameters, but {arguments.Length} were provided", ex);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Invalid arguments for function {Name}: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Default implementation of IFieldDeclaration
    /// </summary>
    public class FieldDeclaration : IFieldDeclaration
    {
        public FieldDeclaration(string name, Type type, object? defaultValue = null, bool isKey = false)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            DefaultValue = defaultValue;
            IsKey = isKey;
        }

        public string Name { get; }
        public Type Type { get; }
        public object? DefaultValue { get; }
        public bool IsKey { get; }
    }

    /// <summary>
    /// Default implementation of ITypeDeclaration
    /// </summary>
    public class TypeDeclaration : ITypeDeclaration
    {
        private readonly List<IFieldDeclaration> _fields = new();
        private readonly Dictionary<string, object> _attributes = new();

        public TypeDeclaration(string name, Type type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public string Name { get; }
        public Type Type { get; }
        public IReadOnlyList<IFieldDeclaration> Fields => _fields;
        public IReadOnlyDictionary<string, object> Attributes => _attributes;

        public void AddField(IFieldDeclaration field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            _fields.Add(field);
        }

        public void SetAttribute(string name, object value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            _attributes[name] = value;
        }
    }
}