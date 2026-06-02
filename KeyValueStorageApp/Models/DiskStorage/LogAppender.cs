using KeyValueStorageApp.Models.Core;

namespace KeyValueStorageApp.Models.DiskStorage
{
    public class LogAppender : ILogAppender
    {
        private readonly StorageContext _context;
        private readonly ILogEntrySerializer _serializer;

        public LogAppender(StorageContext context, ILogEntrySerializer serializer)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public long Append(LogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            // Блокируем контекст только на время записи
            lock (_context.SyncRoot)
            {
                _context.EnsureNotDisposed();

                long offset = _context.FileStream.Seek(0, SeekOrigin.End);
                _serializer.Serialize(_context.Writer, entry);
                _context.Writer.Flush();

                return offset;
            }
        }
    }
}
