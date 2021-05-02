using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Flettu.IO
{
    /// <summary>
    /// Threadsafe writer for any stream
    /// </summary>
    public class ConcurrentStreamWriter : ConcurrentStreamBase
    {
        public override bool CanRead => false;

        public override bool CanWrite => _stream.CanWrite;

        public ConcurrentStreamWriter()
            : base() { }

        /// <summary>
        /// Create a new writer from current postion of the provided stream
        /// </summary>
        public ConcurrentStreamWriter(Stream stream)
            : base(stream) { }

        internal ConcurrentStreamWriter(Stream stream, SemaphoreSlim syncLock)
            : base(stream, syncLock) { }

        public override int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException("Read operation on stream not supported");

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => throw new NotSupportedException("Read operation on stream not supported");

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _syncLock.Wait();
            try
            {
                if (_position != _stream.Position)
                    _stream.Position = _position;

                _stream.Write(buffer, offset, count);
                if(_position + count != _stream.Position)
                    throw new InvalidOperationException($"Invalid underlying stream position: {_stream.Position}, it's expected to be at: {_position + count}");

                _position += count;
            }
            finally
            {
                _syncLock.Release();
            }
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _syncLock.WaitAsync(cancellationToken);
            try
            {
                if (_position != _stream.Position)
                    _stream.Position = _position;

                await _stream.WriteAsync(buffer, offset, count, cancellationToken);
                if(_position + count != _stream.Position)
                    throw new InvalidOperationException($"Invalid underlying stream position: {_stream.Position}, it's expected to be at: {_position + count}");

                _position += count;
            }
            finally
            {
                _syncLock.Release();
            }
        }

        public override void Flush()
        {
            _syncLock.Wait();
            try
            {
                _stream.Flush();
            }
            finally
            {
                _syncLock.Release();
            }
        }

        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            await _syncLock.WaitAsync(cancellationToken);
            try
            {
                await _stream.FlushAsync(cancellationToken);
            }
            finally
            {
                _syncLock.Release();
            }
        }

    }
}
