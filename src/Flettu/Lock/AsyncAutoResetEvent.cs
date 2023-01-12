using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Flettu.Lock
{
    public class AsyncAutoResetEvent : IDisposable
    {
        private readonly SemaphoreSlim _waiters;

        public AsyncAutoResetEvent(bool initialState)
            => _waiters = new SemaphoreSlim(initialState ? 1 : 0, 1);

        public Task<bool> WaitOneAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
            => _waiters.WaitAsync(timeout, cancellationToken);

        public Task WaitOneAsync(CancellationToken cancellationToken = default)
            => _waiters.WaitAsync(cancellationToken);

        public bool WaitOne(TimeSpan timeout, CancellationToken cancellationToken = default)
            => _waiters.WaitAsync(timeout, cancellationToken).Result;

        public void WaitOne(CancellationToken cancellationToken = default)
            => _waiters.WaitAsync(cancellationToken).Wait();

        public void Set()
        {
            lock (_waiters)
            {
                if (_waiters.CurrentCount == 0)
                    _waiters.Release();
            }
        }

        public override string ToString()
            => $"Signaled: {_waiters.CurrentCount != 0}"; 

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _waiters.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}