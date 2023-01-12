using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Flettu.IO
{
    /// <summary>
    /// Threadsafe reader for any stream
    /// </summary>
    public class ConcurrentStreamReader : ConcurrentStreamBase
    {
        public override bool CanRead => _stream.CanRead;

        public override bool CanWrite => false;

        /// <summary>
        /// Create a new reader from zero postion of the provided stream
        /// </summary>
        public ConcurrentStreamReader(Stream stream)
            : base(stream) { }

        internal ConcurrentStreamReader(Stream stream, SemaphoreSlim syncLock)
            : base(stream, syncLock) { }

        public override void SetLength(long value)
            => throw new NotSupportedException("Write operation on stream not supported");

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException("Write operation on stream not supported");

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => throw new NotSupportedException("Write operation on stream not supported");

        public override void Flush()
            => throw new NotSupportedException("Write operation on stream not supported");

        public override Task FlushAsync(CancellationToken cancellationToken)
            => throw new NotSupportedException("Write operation on stream not supported");

        public override int Read(byte[] buffer, int offset, int count)
            => ReadAsync(buffer, offset, count).Result;

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _syncLock.WaitAsync(cancellationToken);
            try
            {
                if (_position != _stream.Position)
                    _stream.Position = _position;

                var readSize = await _stream.ReadAsync(buffer, offset, count, cancellationToken);
                if(_position + readSize != _stream.Position)
                    throw new InvalidOperationException($"Invalid underlying stream position: {_stream.Position}, it's expected to be at: {_position + readSize}");

                _position += readSize;
                return readSize;
            }
            finally
            {
                _syncLock.Release();
            }
        }
    }
}
