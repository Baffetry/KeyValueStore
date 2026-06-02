using KeyValueStorageApp.Models.Common;
using KeyValueStorageApp.Models.Core;

namespace KeyValueStorageApp.Models.DiskStorage
{
    public class LogReader : ILogReader
    {
        private readonly StorageContext _context;
        private readonly ILogEntrySerializer _serializer;

        public LogReader(StorageContext context, ILogEntrySerializer serializer)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public LogEntry Read(long offset)
        {
            Guard.OffsetVerification(offset);

            lock (_context.SyncRoot)
            {
                _context.EnsureNotDisposed();

                if (offset >= _context.FileStream.Length)
                    throw new ArgumentOutOfRangeException(nameof(offset), 
                        "Смещение выходит за пределы файла.");

                _context.FileStream.Seek(offset, SeekOrigin.Begin);
                return _serializer.Deserialize(_context.Reader);
            }
        }

        public IEnumerable<(long Offset, LogEntry Entry)> ReadAllSequentially()
        {
            lock (_context.SyncRoot)
            {
                _context.EnsureNotDisposed();
                _context.FileStream.Seek(0, SeekOrigin.Begin);

                while (_context.FileStream.Position < _context.FileStream.Length)
                {
                    long currentOffset = _context.FileStream.Position;
                    LogEntry entry = _serializer.Deserialize(_context.Reader);
                    yield return (currentOffset, entry);
                }
            }
        }
    }
}
