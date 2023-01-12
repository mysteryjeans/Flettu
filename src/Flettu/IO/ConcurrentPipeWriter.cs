using System;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using Flettu.Collections;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Flettu.Lock;

namespace Flettu.IO
{
    /// <summary>
    /// Threadsafe multiple writer and multiple readers for any stream
    /// </summary>
    public class ConcurrentPipeWriter : ConcurrentStreamWriter
    {
        private readonly AsyncAutoResetEvent _wait = new AsyncAutoResetEvent(false);

        private readonly ConcurrentList<ConcurrentPipeReader> _readers = new ConcurrentList<ConcurrentPipeReader>();

        public ReadOnlyCollection<ConcurrentPipeReader> Readers { get; private set; }

        public bool IsEndOfStream { get; private set; }

        public int ReaderCount => _readers.Count;

        public ConcurrentPipeWriter()
            : base()
        {
            Readers = new ReadOnlyCollection<ConcurrentPipeReader>(_readers);
        }

        public ConcurrentPipeWriter(Stream stream)
            : base(stream)
        {
            Readers = new ReadOnlyCollection<ConcurrentPipeReader>(_readers);
        }

        /// <summary>
        /// Opens a new reader from current postion of underlying stream
        /// </summary>
        public ConcurrentPipeReader OpenStreamReader(int position = 0)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ConcurrentPipeWriter));

            var stream = new ConcurrentPipeReader(this, _stream, _syncLock);
            if (position != 0)
                try
                {
                    stream.Seek(position, SeekOrigin.Begin);
                }
                catch
                {
                    stream.Dispose();
                    throw;
                }

            _readers.Add(stream);
            return stream;
        }

        internal void CloseStreamReader(ConcurrentPipeReader reader)
        {
            var removed = _readers.Remove(reader);
            Debug.Assert(removed);
            _wait.Set();
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await base.WriteAsync(buffer, offset, count, cancellationToken);
            foreach (var reader in _readers)
                reader.Set();
        }

        /// <summary>
        /// Marked write stream completed
        /// </summary>
        public void EndOfStream()
        {
            this.IsEndOfStream = true;
            foreach (var reader in _readers)
                reader.Set();
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    this.EndOfStream();

                    while (_readers.Count != 0)
                        _wait.WaitOne();

                    _wait.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
