using System;
using System.Threading;
using System.Threading.Tasks;

namespace Flettu.Lock
{
    public class AsyncLock : IDisposable
    {
        private static long _globalTaskCounter = 0;
        private static readonly AsyncLocal<long> _taskId = new AsyncLocal<long>();

        private static long GetTaskId()
        {
            if (_taskId.Value == 0)
                _taskId.Value = Interlocked.Increment(ref _globalTaskCounter);

            return _taskId.Value;
        }

        private bool disposed = false;
        private int reentrances = 0;
        private SemaphoreSlim retry = new SemaphoreSlim(1, 1);

        public long? TaskId { get; private set; }

        internal class LockObject : IDisposable
        {
            private long taskId = 0;
            private bool disposed = false;
            private AsyncLock asyncLock;

            public LockObject(AsyncLock asyncLock, long taskId)
            {
                this.taskId = taskId;
                this.asyncLock = asyncLock;
            }

            public async Task<IDisposable> AcquireAsync(CancellationToken cancellationToken)
            {
                if (!HasAcquired())
                {
                    await this.asyncLock.retry.WaitAsync(cancellationToken);

                    this.asyncLock.TaskId = this.taskId;
                    this.asyncLock.reentrances++;
                }

                return this;
            }

            private bool HasAcquired()
            {
                if (this.asyncLock.TaskId != null && this.asyncLock.TaskId == this.taskId)
                {
                    this.asyncLock.reentrances++;
                    return true;
                }

                return false;
            }

            public void Dispose()
            {
                if (!this.disposed)
                {
                    this.disposed = true;

                    this.asyncLock.reentrances--;
                    if (this.asyncLock.reentrances == 0)
                    {
                        this.asyncLock.TaskId = null;
                        this.asyncLock.retry.Release();
                    }
                }
            }
        }

        public Task<IDisposable> AcquireAsync(CancellationToken cancellationToken)
        {
            var lockObject = new LockObject(this, AsyncLock.GetTaskId());
            return lockObject.AcquireAsync(cancellationToken);
        }

        public Task<IDisposable> AcquireAsync() => this.AcquireAsync(CancellationToken.None);

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;
                if (disposing)
                {
                    this.retry.Dispose();
                }
            }
        }

        public void Dispose() => this.Dispose(true);
    }
}
