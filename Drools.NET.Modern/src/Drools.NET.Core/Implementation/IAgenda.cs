using System.Collections.Generic;

namespace Drools.NET.Core.Implementation
{
    /// <summary>
    /// Manages rule activations and conflict resolution
    /// </summary>
    internal interface IAgenda
    {
        /// <summary>
        /// Adds an activation to the agenda
        /// </summary>
        /// <param name="activation">The activation to add</param>
        void AddActivation(IActivation activation);

        /// <summary>
        /// Removes an activation from the agenda
        /// </summary>
        /// <param name="activation">The activation to remove</param>
        void RemoveActivation(IActivation activation);

        /// <summary>
        /// Removes all activations that depend on the given fact
        /// </summary>
        /// <param name="factHandle">The fact handle</param>
        void RemoveActivationsForFact(IFactHandle factHandle);

        /// <summary>
        /// Gets the next activation to fire based on conflict resolution
        /// </summary>
        /// <returns>The next activation, or null if agenda is empty</returns>
        IActivation? GetNextActivation();

        /// <summary>
        /// Gets all current activations
        /// </summary>
        IReadOnlyList<IActivation> Activations { get; }

        /// <summary>
        /// Whether the agenda has any activations
        /// </summary>
        bool HasActivations { get; }

        /// <summary>
        /// Clears all activations from the agenda
        /// </summary>
        void Clear();
    }
}