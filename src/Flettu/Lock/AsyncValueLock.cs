using System;
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
    public class AsyncValueLock<T> : IDisposable
    {
        private class ValueLock
        {
            public SemaphoreSlim Lock { get; set; }
            public int Count { get; set; }
        }

        private readonly SemaphoreSlim _wait = new SemaphoreSlim(1);
        private readonly Dictionary<T, ValueLock> _valueWaits = new Dictionary<T, ValueLock>();

        /// <summary>
        /// Acquires an exclusive lock on the specified value, must be called in 'using' statement
        /// </summary>
        /// <example>
        /// await asyncLock.AcquireAsync(value))
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
        public async Task AcquireAsync(T value, CancellationToken cancellationToken = default)
        {
            ValueLock valueLock;
            await _wait.WaitAsync(cancellationToken);
            try
            {
                if (!_valueWaits.TryGetValue(value, out valueLock))
                {
                    valueLock = new ValueLock { Lock = new SemaphoreSlim(1), Count = 0 };
                    _valueWaits.Add(value, valueLock);
                }

                valueLock.Count++;
            }
            finally
            {
                _wait.Release();
            }

            try
            {
                await valueLock.Lock.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Release(value, false);
                throw;
            }
        }

        /// <summary>
        /// Try Acquiring an exclusive lock on the specified value, must be called in 'using' statement
        /// </summary>
        /// <example>
        /// if(await asyncLock.TryAcquireAsync(value)))
        ///     try
        ///     {
        ///         ....
        ///     }
        ///     finally
        ///     {
        ///         asyncLock.Release(value));
        ///     }
        /// </example>
        /// <param name="value">The value on which to acquire the lock.</param>
        /// <returns>Token for lock object for T value</returns>
        public async Task<bool> TryAcquireAsync(T value, CancellationToken cancellationToken = default)
        {
            ValueLock valueLock;
            await _wait.WaitAsync(cancellationToken);
            try
            {
                if (!_valueWaits.TryGetValue(value, out valueLock))
                {
                    valueLock = new ValueLock { Lock = new SemaphoreSlim(1), Count = 0 };
                    _valueWaits.Add(value, valueLock);
                }

                if (await valueLock.Lock.WaitAsync(0))
                {
                    valueLock.Count++;
                    return true;
                }
            }
            finally
            {
                _wait.Release();
            }

            return false;
        }

        public void Release(T value) => Release(value, true);

        private void Release(T value, bool releaseLock = true)
        {
            ValueLock valueLock;
            _wait.Wait();
            try
            {
                if (_valueWaits.TryGetValue(value, out valueLock))
                {
                    valueLock.Count--;
                    if (valueLock.Count == 0)
                        _valueWaits.Remove(value);
                }
                else
                    return;
            }
            finally
            {
                _wait.Release();
            }

            if (releaseLock)
                valueLock.Lock.Release();

            if (valueLock.Count == 0)
                valueLock.Lock.Dispose();
        }

        public void Dispose() => _wait.Dispose();
    }
}