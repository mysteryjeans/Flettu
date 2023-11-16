using System;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using Flettu.Collections;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Flettu.Lock;
using Flettu.Extensions;

namespace Flettu.IO
{
    /// <summary>
    /// Threadsafe multiple writer and multiple readers for any stream
    /// </summary>
    public class ConcurrentPipeWriter : Stream
    {
        private long _advanceTo;

        private bool _disposed;

        private byte[] _buffer;

        private readonly MemoryStream _stream = new MemoryStream();

        private readonly ConcurrentList<ConcurrentPipeReader> _readers = new ConcurrentList<ConcurrentPipeReader>();

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public ReadOnlyCollection<ConcurrentPipeReader> Readers { get; private set; }

        public bool IsEndOfStream { get; private set; }

        public int ReaderCount => _readers.Count;

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => _advanceTo + BufferLength;

        public override long Position { get => _advanceTo + BufferLength; set => throw new NotImplementedException(); }

        public long AdvanceTo => _advanceTo;

        internal byte[] Buffer => _buffer ?? _stream.GetBuffer();

        internal long BufferLength => _buffer != null ? _buffer.Length : _stream.Length;

        public ConcurrentPipeWriter()
            : base()
        {
            Readers = new ReadOnlyCollection<ConcurrentPipeReader>(_readers);
        }

        /// <summary>
        /// Opens a new reader from current postion of underlying stream
        /// </summary>
        public async Task<ConcurrentPipeReader> OpenStreamReaderAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ConcurrentPipeWriter));

            await _lock.WaitAsync(cancellationToken);
            try
            {
                var stream = new ConcurrentPipeReader(this);
                _readers.Add(stream);
                return stream;
            }
            finally
            {
                _lock.Release();
            }
        }

        internal async Task AcquireReadLockAsync(CancellationToken cancellationToken)
        {
            if (!_disposed)
                try
                {
                    while (!_disposed)
                        using (var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                            try
                            {
                                timeoutCts.CancelAfter(1000);
                                await _lock.WaitAsync(timeoutCts.Token);
                                return;
                            }
                            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                            { }
                }
                catch (ObjectDisposedException)
                { }
        }

        internal void ReleaseReadLock()
        {
            if (!_disposed)
                try
                {
                    _lock.Release();
                }
                catch (ObjectDisposedException)
                { }
        }

        internal void CloseStreamReader(ConcurrentPipeReader reader)
        {
            var removed = _readers.Remove(reader);
            Debug.Assert(removed);
        }

        public override void Write(byte[] buffer, int offset, int count) => WriteAsync(buffer, offset, count).Wait();

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                await _stream.WriteAsync(buffer, offset, count);
            }
            finally
            {
                _lock.Release();
            }

            foreach (var reader in _readers)
                reader.Set();
        }

        public override void Flush() => _stream.Flush();

        public async Task AdvanceToAsync(int? count = null, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                var advanceTo = Position;

                if (count != null)
                    advanceTo = _advanceTo + count.Value;
                else
                {
                    foreach (var reader in _readers)
                        if (reader.Position < advanceTo)
                            advanceTo = reader.Position;
                }

                var offset = advanceTo - _advanceTo;
                if (offset < 0 || offset > _stream.Length)
                    throw new ArgumentNullException($"Cannot advance to: {advanceTo}, since already advance to: {_advanceTo} and buffer size is: {_stream.Length}");

                if (offset > 0)
                {
                    var length = (int)(_stream.Length - offset);

                    _stream.Seek(0, SeekOrigin.Begin);
                    _stream.Write(Buffer, (int)offset, length);
                    _stream.SetLength(length);

                    _advanceTo = advanceTo;
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Marked write stream completed
        /// </summary>
        public void EndOfStream() => EndOfStreamAsync().Wait();

        public async Task EndOfStreamAsync(CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                _buffer = _stream.ToArray();
                this.IsEndOfStream = true;
            }
            finally
            {
                _lock.Release();
            }

            foreach (var reader in _readers)
                reader.Set();
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        public override void SetLength(long value) => throw new NotImplementedException();

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (!IsEndOfStream)
                        this.EndOfStream();

                    _lock.Dispose();

                    _stream.Dispose();
                }
                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
