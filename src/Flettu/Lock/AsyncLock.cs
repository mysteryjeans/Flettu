using System;
using System.Threading;
using System.Threading.Tasks;

namespace Flettu.Lock
{
    public class AsyncLock : IDisposable
    {
        private long _taskCounter = 0;
        private readonly AsyncLocal<long> _taskId = new AsyncLocal<long>();

        private long GetTaskId()
        {
            if (_taskId.Value == 0)
                _taskId.Value = Interlocked.Increment(ref _taskCounter);

            return _taskId.Value;
        }

        private int _reentrances = 0;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public long TaskId { get; private set; } 

        public Task AcquireAsync(CancellationToken cancellationToken = default)
        {
            var currentTaskId = GetTaskId();
            if (TaskId == currentTaskId)
                _reentrances++;

            return TakeLock();
            async Task TakeLock()
            {
                await _lock.WaitAsync(cancellationToken);
                TaskId = currentTaskId;
                _reentrances++;
            }
        }

        public void Release()
        {
            if (TaskId != GetTaskId())
                throw new InvalidOperationException($"Lock may only be release from the acquired task, otherwise use {nameof(SemaphoreSlim)}!");

            _reentrances--;
            if (_reentrances == 0)
            {
                TaskId = 0;
                _lock.Release();
            }
        }

        private bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _lock.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose() => Dispose(true);
    }
}
