using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Flettu.Lock;
using Flettu.Util;

namespace Flettu.IO
{
    /// <summary>
    /// Threadsafe reader for any stream
    /// </summary>
    public class ConcurrentPipeReader : ConcurrentStreamReader
    {
        private readonly AsyncAutoResetEvent _wait = new AsyncAutoResetEvent(false);
        private readonly ConcurrentPipeWriter _writer;

        public bool IsEndOfStream => _writer.IsEndOfStream && _position == _writer.Length;

        /// <summary>
        /// Create a new reader from zero postion of the provided stream
        /// </summary>
        internal ConcurrentPipeReader(ConcurrentPipeWriter writer, Stream stream)
            : base(stream)
        {
            Guard.CheckNull(writer, nameof(writer));
            _writer = writer;
        }

        /// <summary>
        /// Create a new reader from zero postion of the provided stream
        /// </summary>
        internal ConcurrentPipeReader(ConcurrentPipeWriter writer, Stream stream, SemaphoreSlim syncLock)
            : base(stream, syncLock)
        {
            Guard.CheckNull(writer, nameof(writer));
            _writer = writer;
        }

        internal void Set()
        {
            _wait.Set();
        }

        /// <summary>
        /// Reads into the buffer from available bytes or stream end
        /// </summary>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var readSize = await base.ReadAsync(buffer, offset, count, cancellationToken);
            while (readSize == 0 && !IsEndOfStream)
            {
                await _wait.WaitOneAsync(cancellationToken);
                readSize = await base.ReadAsync(buffer, offset, count, cancellationToken);
            }

            return readSize;
        }

        /// <summary>
        /// Reads into the buffer to the desired count or stream end
        /// </summary>
        public Task<int> ReadToCountAsync(byte[] buffer, int offset, int count)
            => ReadToCountAsync(buffer, offset, count, CancellationToken.None);

        /// <summary>
        /// Reads into the buffer to the desired count or stream end
        /// </summary>
        public async Task<int> ReadToCountAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var readSize = await base.ReadAsync(buffer, offset, count, cancellationToken);
            while (readSize < count && !IsEndOfStream)
            {
                await _wait.WaitOneAsync(cancellationToken);
                readSize += await base.ReadAsync(buffer, offset + readSize, count - readSize, cancellationToken);
            }

            return readSize;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    _writer.CloseStreamReader(this);
                    _wait.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
