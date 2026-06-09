using KeyValueStorageApp.Models.Core;
using KeyValueStorageApp.Models.DiskStorage;

namespace KeyValueStorageApp.Tests
{
    public class LogAppenderTests : IDisposable
    {
        private readonly string _tempFilePath;
        private readonly StorageContext _context;
        private readonly ILogEntrySerializer _serializer;

        public LogAppenderTests()
        {
            _tempFilePath = Path.GetTempFileName();
            _context = new StorageContext(_tempFilePath);
            _serializer = new StandardLogEntrySerializer();
        }

        [Fact]
        public void Append_NullEntry_ThrowsArgumentNullException()
        {
            var appender = new LogAppender(_context, _serializer);

            Assert.Throws<ArgumentNullException>(() => appender.Append(null));
        }

        [Fact]
        public void Append_MultipleEntries_ReturnsSequentialOffsets()
        {
            // Arrange
            var appender = new LogAppender(_context, _serializer);
            var entry1 = new LogEntry("key1", "value1", false);
            var entry2 = new LogEntry("key2", "value2", false);

            // Act
            long offset1 = appender.Append(entry1);
            long offset2 = appender.Append(entry2);

            // Assert
            Assert.Equal(0, offset1); // Первая запись всегда начинается с 0
            Assert.True(offset2 > offset1); // Вторая запись должна быть дальше в файле

            // Физически проверяем, что размер файла вырос и равен позиции конца второй записи
            Assert.Equal(_context.FileStream.Length, _context.FileStream.Position);
        }

        public void Dispose()
        {
            _context.Dispose();
            if (File.Exists(_tempFilePath))
                File.Delete(_tempFilePath);
        }
    }
}