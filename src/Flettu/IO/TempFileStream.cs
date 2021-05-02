using System.IO;

namespace Flettu.IO
{
    public class TempFileStream : FileStream
    {
        public TempFileStream(FileMode mode, string path = null)
           : base(path ?? Path.GetTempFileName(), mode)
        { }

        public TempFileStream(FileMode mode, FileAccess access, string path = null)
           : base(path ?? Path.GetTempFileName(), mode, access)
        { }

        public TempFileStream(FileMode mode, FileAccess access, FileShare share, string path = null)
            : base(path ?? Path.GetTempFileName(), mode, access, share)
        { }

        private bool _isDisposed;
        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                base.Dispose();

                if (File.Exists(this.Name))
                    File.Delete(this.Name);

                _isDisposed = true;
            }
        }
    }
}
