using System;
using System.IO;
using System.Threading;

namespace Flettu.IO
{
    public abstract class ConcurrentStreamBase : Stream
    {
        private readonly bool _disposeLock;
        private readonly bool _disposeStream;

        protected long _position;
        protected readonly Stream _stream;
        protected readonly SemaphoreSlim _syncLock;

        public override bool CanSeek => true;

        public override long Length => _stream.Length;

        public override long Position { get => _position; set => _position = value; }

        private ConcurrentStreamBase(Stream stream, SemaphoreSlim syncLock, bool disposeStream, bool disposeLock)
            => (_stream, _position, _syncLock, _disposeStream, _disposeLock) = (stream, stream.Position, syncLock, disposeStream, disposeLock);

        protected ConcurrentStreamBase()
            : this(new MemoryStream(), new SemaphoreSlim(1, 1), true, true) { }

        protected ConcurrentStreamBase(Stream stream)
            : this(stream, new SemaphoreSlim(1, 1), false, true) { }

        protected ConcurrentStreamBase(Stream stream, SemaphoreSlim syncLock)
            : this(stream, syncLock, false, false) { }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!this.CanSeek)
                throw new NotSupportedException("Seek operation not supported");

            long tempPosition;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    tempPosition = offset;
                    break;
                case SeekOrigin.Current:
                    tempPosition = _position + offset;
                    break;
                case SeekOrigin.End:
                    tempPosition = this.Length + offset;
                    break;
                default:
                    throw new ArgumentException($"Invalid seek origin: {origin}");
            }

            if (tempPosition < 0)
                throw new ArgumentOutOfRangeException("offset", $"Invalid offset: {offset} for seek origin: {origin}");

            if (tempPosition > int.MaxValue)
                throw new ArgumentOutOfRangeException("offset", $"Offset: {offset} exceeded maximum stream length");

            _position = tempPosition;
            return _position;
        }

        private bool _isDisposed;
        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_disposeStream)
                        _stream.Dispose();

                    if (_disposeLock)
                        _syncLock.Dispose();
                }

                _isDisposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
