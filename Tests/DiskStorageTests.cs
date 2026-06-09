using KeyValueStorageApp.Models.Core;
using KeyValueStorageApp.Models.DiskStorage;

namespace KeyValueStorageApp.Tests
{
    public class DiskStorageTests : IDisposable
    {
        private readonly string _tempFilePath;
        private readonly StorageContext _context;
        private readonly ILogEntrySerializer _serializer;

        public DiskStorageTests()
        {
            _tempFilePath = Path.GetTempFileName();
            _context = new StorageContext(_tempFilePath);
            _serializer = new StandardLogEntrySerializer();
        }

        [Fact]
        public void AppenderAndReader_WriteAndReadSequential_ReturnsCorrectData()
        {
            // Arrange
            var appender = new LogAppender(_context, _serializer);
            var reader = new LogReader(_context, _serializer);

            var entry1 = new LogEntry("user:1", "alice", false);
            var entry2 = new LogEntry("user:2", "bob", false);
            var entry3 = new LogEntry("user:1", null, true); // Удаление первого ключа

            // Act
            var offset1 = appender.Append(entry1);
            var offset2 = appender.Append(entry2);
            var offset3 = appender.Append(entry3);

            // Assert: Произвольный доступ по смещению
            var readEntry1 = reader.Read(offset1);
            Assert.Equal("alice", readEntry1.Value);

            var readEntry3 = reader.Read(offset3);
            Assert.True(readEntry3.IsDeleted);

            // Assert: Последовательное чтение
            var allRecords = reader.ReadAllSequentially().ToList();
            Assert.Equal(3, allRecords.Count);
            Assert.Equal(offset2, allRecords[1].Offset);
            Assert.Equal("bob", allRecords[1].Entry.Value);
        }

        public void Dispose()
        {
            _context.Dispose();
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }
    }
}
