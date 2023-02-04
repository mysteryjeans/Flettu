using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flettu.Util;

namespace Flettu.Lock
{
    /// <summary>
    /// Synchronizer for value base locks
    /// </summary>
    /// <typeparam name="T">Type of value</typeparam>
    public class AsyncValueLock<T>
    { 
        private readonly ConcurrentDictionary<T, (SemaphoreSlim Lock, int Count)> _waits = new ConcurrentDictionary<T, (SemaphoreSlim Lock, int Count)>();

        /// <summary>
        /// Acquires an exclusive lock on the specified value, must be called in 'using' statement
        /// </summary>
        /// <example>
        /// var token = await asyncLock.AcquireAsync(value))
        /// try
        /// {
        ///     ....
        /// }
        /// finally
        /// {
        ///     asyncLock.Release(value));
        /// }
        /// </example>
        /// <param name="value">The value on which to acquire the lock.</param>
        /// <returns>Token for lock object for T value</returns>
        public async Task<int> AcquireAsync(T value, CancellationToken cancellationToken = default)
        {
            Guard.CheckNull(value, nameof(value));

            var (wait, token) = _waits.AddOrUpdate(value, _ => (new SemaphoreSlim(1), 1), (_, x) => (x.Lock, x.Count + 1));
            await wait.WaitAsync(cancellationToken);

            return token;
        }

        public int this[T value] 
            => _waits.TryGetValue(value, out var wait) ? wait.Count : 0;

        public void Release(T value, int token)
        {
            // Cast to IDictionary to do Atomic remove if the "Value" hasn't changed from the update statement
            // gist taken from https://devblogs.microsoft.com/pfxteam/little-known-gems-atomic-conditional-removals-from-concurrentdictionary/ 
            if (_waits.TryGetValue(value, out var wait))
            {
                wait.Lock.Release();

                var dict = (IDictionary<T, (SemaphoreSlim, int)>)_waits;
                if (dict.Remove(new KeyValuePair<T, (SemaphoreSlim, int)>(value, (wait.Lock, token))))
                    wait.Lock.Dispose();
            }
        }

        public override string ToString() => $"{nameof(AsyncValueLock<T>)}[Locks:{this._waits.Count}]";
    }
}
