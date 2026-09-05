using System.Threading;

namespace Drools.NET.Core.Implementation
{
    /// <summary>
    /// Provides unique activation numbers for rules
    /// </summary>
    internal static class ActivationCounter
    {
        private static long _nextActivationNumber = 1;

        /// <summary>
        /// Gets the next unique activation number
        /// </summary>
        public static long GetNext()
        {
            return Interlocked.Increment(ref _nextActivationNumber);
        }
    }
}