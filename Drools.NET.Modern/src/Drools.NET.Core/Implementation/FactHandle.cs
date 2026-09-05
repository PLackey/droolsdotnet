using System;
using System.Threading;

namespace Drools.NET.Core.Implementation
{
    /// <summary>
    /// Default implementation of IFactHandle
    /// </summary>
    internal class FactHandle : IFactHandle
    {
        private static long _nextId = 1;
        private static long _nextRecency = 1;

        public FactHandle(object obj)
        {
            Object = obj ?? throw new ArgumentNullException(nameof(obj));
            Id = Interlocked.Increment(ref _nextId);
            Recency = Interlocked.Increment(ref _nextRecency);
        }

        public long Id { get; }
        public object Object { get; internal set; }
        public long Recency { get; }

        public override bool Equals(object? obj)
        {
            return obj is FactHandle other && Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"FactHandle[{Id}:{Object?.GetType().Name}]";
        }
    }
}