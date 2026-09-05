using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Drools.NET.Core
{
    /// <summary>
    /// Builds rule packages from DRL (Drools Rule Language) source
    /// </summary>
    public interface IPackageBuilder
    {
        /// <summary>
        /// Adds rules from a DRL stream to the package being built
        /// </summary>
        /// <param name="drlStream">Stream containing DRL content</param>
        void AddPackageFromDrl(Stream drlStream);

        /// <summary>
        /// Adds rules from a DRL stream with a resource name
        /// </summary>
        /// <param name="resourceName">Name of the resource (for error reporting)</param>
        /// <param name="drlStream">Stream containing DRL content</param>
        void AddPackageFromDrl(string resourceName, Stream drlStream);

        /// <summary>
        /// Adds rules from DRL content asynchronously
        /// </summary>
        /// <param name="resourceName">Name of the resource</param>
        /// <param name="drlStream">Stream containing DRL content</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task AddPackageFromDrlAsync(string resourceName, Stream drlStream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds rules from DRL string content
        /// </summary>
        /// <param name="drlContent">DRL content as string</param>
        void AddPackageFromDrl(string drlContent);

        /// <summary>
        /// Adds rules from DRL string content with a resource name
        /// </summary>
        /// <param name="resourceName">Name of the resource</param>
        /// <param name="drlContent">DRL content as string</param>
        void AddPackageFromDrl(string resourceName, string drlContent);

        /// <summary>
        /// Adds a decision table from a stream
        /// </summary>
        /// <param name="dtableStream">Stream containing decision table (Excel, CSV, etc.)</param>
        /// <param name="inputType">Type of decision table input</param>
        void AddPackageFromDecisionTable(Stream dtableStream, DecisionTableInputType inputType);

        /// <summary>
        /// Adds a decision table from a stream with resource name
        /// </summary>
        /// <param name="resourceName">Name of the resource</param>
        /// <param name="dtableStream">Stream containing decision table</param>
        /// <param name="inputType">Type of decision table input</param>
        void AddPackageFromDecisionTable(string resourceName, Stream dtableStream, DecisionTableInputType inputType);

        /// <summary>
        /// Gets the compiled package
        /// </summary>
        /// <returns>The compiled package, or null if compilation failed</returns>
        IPackage? GetPackage();

        /// <summary>
        /// Gets all packages that have been built
        /// </summary>
        IReadOnlyList<IPackage> Packages { get; }

        /// <summary>
        /// Whether there are any compilation errors
        /// </summary>
        bool HasErrors { get; }

        /// <summary>
        /// All compilation errors
        /// </summary>
        IReadOnlyList<ICompilationError> Errors { get; }

        /// <summary>
        /// All compilation warnings
        /// </summary>
        IReadOnlyList<ICompilationError> Warnings { get; }

        /// <summary>
        /// Clears all packages and errors
        /// </summary>
        void Clear();

        /// <summary>
        /// Configuration for this package builder
        /// </summary>
        IPackageBuilderConfiguration Configuration { get; }

        /// <summary>
        /// Event fired when compilation errors occur
        /// </summary>
        event EventHandler<CompilationErrorEventArgs>? ErrorOccurred;

        /// <summary>
        /// Event fired when compilation warnings occur
        /// </summary>
        event EventHandler<CompilationErrorEventArgs>? WarningOccurred;
    }

    /// <summary>
    /// Configuration for package builder behavior
    /// </summary>
    public interface IPackageBuilderConfiguration
    {
        /// <summary>
        /// Language level for DRL parsing
        /// </summary>
        LanguageLevel LanguageLevel { get; set; }

        /// <summary>
        /// Whether to include debug information in compiled rules
        /// </summary>
        bool IncludeDebugInfo { get; set; }

        /// <summary>
        /// Default dialect for consequences (e.g., "csharp", "mvel")
        /// </summary>
        string DefaultDialect { get; set; }

        /// <summary>
        /// Additional imports to include in all packages
        /// </summary>
        IList<string> DefaultImports { get; }

        /// <summary>
        /// Whether to treat warnings as errors
        /// </summary>
        bool TreatWarningsAsErrors { get; set; }

        /// <summary>
        /// Maximum allowed rule complexity
        /// </summary>
        int MaxRuleComplexity { get; set; }
    }

    /// <summary>
    /// Default package builder configuration
    /// </summary>
    public class PackageBuilderConfiguration : IPackageBuilderConfiguration
    {
        public LanguageLevel LanguageLevel { get; set; } = LanguageLevel.DRL6;
        public bool IncludeDebugInfo { get; set; } = true;
        public string DefaultDialect { get; set; } = "csharp";
        public IList<string> DefaultImports { get; } = new List<string>
        {
            "System",
            "System.Collections.Generic",
            "System.Linq"
        };
        public bool TreatWarningsAsErrors { get; set; } = false;
        public int MaxRuleComplexity { get; set; } = 1000;

        /// <summary>
        /// Default configuration instance
        /// </summary>
        public static IPackageBuilderConfiguration Default => new PackageBuilderConfiguration();
    }

    /// <summary>
    /// Represents a compilation error or warning
    /// </summary>
    public interface ICompilationError
    {
        /// <summary>
        /// The error message
        /// </summary>
        string Message { get; }

        /// <summary>
        /// The resource where the error occurred
        /// </summary>
        string? Resource { get; }

        /// <summary>
        /// Line number where the error occurred (1-based)
        /// </summary>
        int Line { get; }

        /// <summary>
        /// Column number where the error occurred (1-based)
        /// </summary>
        int Column { get; }

        /// <summary>
        /// Whether this is an error (true) or warning (false)
        /// </summary>
        bool IsError { get; }

        /// <summary>
        /// Error code or category
        /// </summary>
        string? Code { get; }

        /// <summary>
        /// The underlying exception, if any
        /// </summary>
        Exception? Exception { get; }
    }

    /// <summary>
    /// Default implementation of compilation error
    /// </summary>
    public class CompilationError : ICompilationError
    {
        public CompilationError(string message, bool isError = true, string? resource = null, int line = 0, int column = 0, string? code = null, Exception? exception = null)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            IsError = isError;
            Resource = resource;
            Line = line;
            Column = column;
            Code = code;
            Exception = exception;
        }

        public string Message { get; }
        public string? Resource { get; }
        public int Line { get; }
        public int Column { get; }
        public bool IsError { get; }
        public string? Code { get; }
        public Exception? Exception { get; }

        public override string ToString()
        {
            var prefix = IsError ? "Error" : "Warning";
            var location = Resource != null ? $" in {Resource}" : "";
            var position = Line > 0 ? $" at line {Line}" : "";
            if (Column > 0) position += $", column {Column}";
            var codeText = Code != null ? $" ({Code})" : "";

            return $"{prefix}{location}{position}{codeText}: {Message}";
        }
    }

    /// <summary>
    /// Event arguments for compilation errors
    /// </summary>
    public class CompilationErrorEventArgs : EventArgs
    {
        public CompilationErrorEventArgs(ICompilationError error)
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }

        public ICompilationError Error { get; }
    }

    /// <summary>
    /// DRL language levels
    /// </summary>
    public enum LanguageLevel
    {
        /// <summary>
        /// Drools 3.x syntax
        /// </summary>
        DRL3,

        /// <summary>
        /// Drools 4.x syntax  
        /// </summary>
        DRL4,

        /// <summary>
        /// Drools 5.x syntax
        /// </summary>
        DRL5,

        /// <summary>
        /// Drools 6.x syntax (default)
        /// </summary>
        DRL6,

        /// <summary>
        /// Drools 7.x syntax
        /// </summary>
        DRL7,

        /// <summary>
        /// Latest syntax
        /// </summary>
        Latest = DRL7
    }

    /// <summary>
    /// Decision table input types
    /// </summary>
    public enum DecisionTableInputType
    {
        /// <summary>
        /// Excel format (.xls, .xlsx)
        /// </summary>
        Excel,

        /// <summary>
        /// Comma-separated values
        /// </summary>
        CSV
    }
}