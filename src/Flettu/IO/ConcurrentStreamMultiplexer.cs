using System.IO;

namespace Flettu.IO
{
    /// <summary>
    /// Threadsafe multiple writer and multiple readers for any stream
    /// </summary>
    internal class ConcurrentStreamMultiplexer : ConcurrentStreamWriter
    {
        public ConcurrentStreamMultiplexer()
            : base() { }

        public ConcurrentStreamMultiplexer(Stream stream)
            : base(stream) { }

        /// <summary>
        /// Opens a new reader from current postion of underlying stream
        /// </summary>
        public Stream OpenStreamReader(int? position = null)
        {
            var stream = new ConcurrentStreamReader(_stream, _syncLock);
            if (position != null)
                try
                {
                    stream.Seek(position.Value, SeekOrigin.Begin);
                }
                catch
                {
                    stream.Dispose();
                    throw;
                }

            return stream;
        }
    }
}
