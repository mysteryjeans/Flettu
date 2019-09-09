using System;
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
        #region Lock Helper Classes

        private class LockHandle : IDisposable
        {
            private bool disposed = false;
            private Task<IDisposable> acquireLockTask = null;
            private IDisposable acquiredLock = null;
            private LockObject lockObject = null;
            private AsyncValueLock<T> valueLock;

            public LockHandle(AsyncValueLock<T> sync, LockObject lockObject)
            {
                this.valueLock = sync;
                this.lockObject = lockObject;
                this.acquireLockTask = this.lockObject.AcquireAsync();
            }

            public async Task<IDisposable> AcquireAsync()
            {
                this.acquiredLock = await this.acquireLockTask;
                return this;
            }

            public void Dispose()
            {
                if(!this.disposed)
                {
                    this.disposed = true;
                    this.acquiredLock.Dispose();
                    this.valueLock.UnLock(this.lockObject);
                }
            }
        }

        private class LockObject : IDisposable
        {
            private AsyncLock retry = new AsyncLock();

            public readonly T Value;

            public int Count;

            public long? TaskId { get => this.retry.TaskId; }

            public LockObject(T value) => this.Value = value;

            public Task<IDisposable> AcquireAsync() => this.retry.AcquireAsync();

            public Task<IDisposable> AcquireAsync(CancellationToken cancellationToken) => this.retry.AcquireAsync(cancellationToken);

            public void Dispose() => this.retry.Dispose();
        }

        #endregion

        private Dictionary<T, LockObject> lockObjects = new Dictionary<T, LockObject>();

        /// <summary>
        /// Acquires an exclusive lock on the specified value, must be called in 'using' statement
        /// </summary>
        /// <example>
        /// using(await asyncLock.AcquireAsync(value))
        /// {
        ///     ....
        /// }
        /// </example>
        /// <param name="value">The value on which to acquire the lock.</param>
        /// <returns>Handle for lock object for T value</returns>
        public Task<IDisposable> AcquireAsync(T value)
        {
            Guard.CheckNull(value, nameof(value));

            LockObject lockObject;
            lock (this.lockObjects)
            {
                if (!this.lockObjects.TryGetValue(value, out lockObject))
                {
                    lockObject = new LockObject(value);
                    this.lockObjects.Add(value, lockObject);
                }

                lockObject.Count += 1;
            } 

            var lockHandle = new LockHandle(this, lockObject);
            return lockHandle.AcquireAsync();
        }

        public long? GetTaskId(T value)
        {
            lock (this.lockObjects)
                if (this.lockObjects.TryGetValue(value, out LockObject lockObject))
                    return lockObject.TaskId;

            return null;
        }

        private void UnLock(LockObject lockObject)
        {
            lock (this.lockObjects)
            {
                lockObject.Count -= 1;
                if (lockObject.Count == 0)
                {
                    this.lockObjects.Remove(lockObject.Value);
                    lockObject.Dispose();
                }
            }
        }

        public override string ToString() => $"{nameof(AsyncValueLock<T>)}[Locks:{this.lockObjects.Count}]";
    }
}
