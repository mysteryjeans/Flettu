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
    public class ConcurrentPipeReader : Stream
    {
        private bool _disposed;
        private readonly AsyncAutoResetEvent _wait = new AsyncAutoResetEvent(false);
        private readonly ConcurrentPipeWriter _pipe;

        public bool IsEndOfStream => _pipe.IsEndOfStream && Position == _pipe.Position;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => _pipe.Length;

        public override long Position { get; set; }

        /// <summary>
        /// Create a new reader from zero postion of the provided stream
        /// </summary>
        internal ConcurrentPipeReader(ConcurrentPipeWriter pipe)
        {
            Guard.CheckNull(pipe, nameof(pipe));
            _pipe = pipe;
            Position = pipe.AdvanceTo;
        }

        internal void Set()
        {
            _wait.Set();
        }

        /// <summary>
        /// Reads into the buffer from available bytes or stream end
        /// </summary>
        public async Task<int> ReadAvailableAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            var readSize = await ReadBufferAsync(buffer, offset, count, cancellationToken);
            while (readSize == 0 && !IsEndOfStream)
            {
                await _wait.WaitOneAsync(cancellationToken);
                readSize = await ReadBufferAsync(buffer, offset, count, cancellationToken);
            }

            return readSize;
        }

        /// <summary>
        /// Reads into the buffer to the desired count or stream end
        /// </summary>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var readSize = await ReadAvailableAsync(buffer, offset, count, cancellationToken);
            while (readSize < count && !IsEndOfStream)
                readSize += await ReadAvailableAsync(buffer, offset + readSize, count - readSize, cancellationToken);

            return readSize;
        }

        private async Task<int> ReadBufferAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            await _pipe.AcquireReadLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (Position > _pipe.Length)
                    throw new ArgumentOutOfRangeException($"Pipe buffer total length is: {_pipe.Length}, cannot read buffer at: {Position}");

                var srcOffset = Position - _pipe.AdvanceTo;
                if (srcOffset < 0)
                    throw new ArgumentOutOfRangeException($"Pipe buffer is already advance to: {_pipe.AdvanceTo}, cannot read buffer at position: {Position}");

                var readSize = _pipe.BufferLength - srcOffset < count ? _pipe.BufferLength - srcOffset : count;
                if (readSize < 0)
                    throw new InvalidOperationException($"Unable to properly calculate read count: {count} for position: {Position}, total buffer length: {_pipe.Length}, advance to: {_pipe.AdvanceTo}");

                if (readSize != 0)
                    Array.Copy(_pipe.Buffer, srcOffset, buffer, offset, readSize);

                Position += readSize;

                return (int)readSize;
            }
            finally
            {
                _pipe.ReleaseReadLock();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _pipe.CloseStreamReader(this);
                    _wait.Dispose();
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        public override void Flush() => throw new NotImplementedException();

        public override int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer, offset, count).Result;

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        public override void SetLength(long value) => throw new NotImplementedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
    }
}
