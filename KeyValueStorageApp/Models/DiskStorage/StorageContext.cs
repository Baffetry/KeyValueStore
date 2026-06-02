using KeyValueStorageApp.Models.Common;

namespace KeyValueStorageApp.Models.DiskStorage
{
    public class StorageContext : IDisposable
    {
        public FileStream FileStream { get; private set; }
        public BinaryWriter Writer { get; private set; }
        public BinaryReader Reader { get; private set; }
        public object SyncRoot { get; } = new object();
        private bool _disposed = false;

        public StorageContext(string filePath)
        {
            Init(filePath);
        }

        private void Init(string filePath)
        {
            Guard.FilePathVerification(filePath);

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            FileStream = new FileStream(filePath, FileMode.OpenOrCreate, 
                FileAccess.ReadWrite, FileShare.Read);
            Writer = new BinaryWriter(FileStream);
            Reader = new BinaryReader(FileStream);
        }

        public void EnsureNotDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(StorageContext));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Writer?.Dispose();
                Reader?.Dispose();
                FileStream?.Dispose();
                _disposed = true;
            }
        }
    }
}
