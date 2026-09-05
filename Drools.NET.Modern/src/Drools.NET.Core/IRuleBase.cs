using System;
using System.Collections.Generic;

namespace Drools.NET.Core
{
    /// <summary>
    /// Represents a rule base that contains packages of compiled rules
    /// </summary>
    public interface IRuleBase
    {
        /// <summary>
        /// Adds a package of compiled rules to this rule base
        /// </summary>
        /// <param name="package">The package to add</param>
        void AddPackage(IPackage package);

        /// <summary>
        /// Removes a package from this rule base
        /// </summary>
        /// <param name="packageName">The name of the package to remove</param>
        void RemovePackage(string packageName);

        /// <summary>
        /// Gets all packages in this rule base
        /// </summary>
        IReadOnlyList<IPackage> Packages { get; }

        /// <summary>
        /// Creates a new working memory session for executing rules
        /// </summary>
        /// <returns>A new working memory instance</returns>
        IWorkingMemory CreateWorkingMemory();

        /// <summary>
        /// Gets the configuration for this rule base
        /// </summary>
        IRuleBaseConfiguration Configuration { get; }

        /// <summary>
        /// Event fired when a package is added
        /// </summary>
        event EventHandler<PackageEventArgs>? PackageAdded;

        /// <summary>
        /// Event fired when a package is removed
        /// </summary>
        event EventHandler<PackageEventArgs>? PackageRemoved;
    }

    /// <summary>
    /// Event arguments for package-related events
    /// </summary>
    public class PackageEventArgs : EventArgs
    {
        public PackageEventArgs(IPackage package)
        {
            Package = package ?? throw new ArgumentNullException(nameof(package));
        }

        public IPackage Package { get; }
    }
}