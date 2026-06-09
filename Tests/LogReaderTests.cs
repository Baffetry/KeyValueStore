using KeyValueStorageApp.Models.Core;
using KeyValueStorageApp.Models.DiskStorage;

namespace KeyValueStorageApp.Tests
{
    public class LogReaderTests : IDisposable
    {
        private readonly string _tempFilePath;
        private readonly StorageContext _context;
        private readonly ILogEntrySerializer _serializer;
        private readonly LogAppender _appender;
        private readonly LogReader _reader;

        public LogReaderTests()
        {
            _tempFilePath = Path.GetTempFileName();
            _context = new StorageContext(_tempFilePath);
            _serializer = new StandardLogEntrySerializer();

            _appender = new LogAppender(_context, _serializer);
            _reader = new LogReader(_context, _serializer);
        }

        [Fact]
        public void Read_Valid_Offset_Returns_Correct_Entry()
        {
            // Arrange: подготавливаем базу
            var expectedEntry = new LogEntry("target_key", "target_value", false);

            _appender.Append(new LogEntry("noise_1", "noise_val", false));
            long targetOffset = _appender.Append(expectedEntry);
            _appender.Append(new LogEntry("noise_2", "noise_val", false));

            // Act
            var actualEntry = _reader.Read(targetOffset);

            // Assert
            Assert.Equal(expectedEntry.Key, actualEntry.Key);
            Assert.Equal(expectedEntry.Value, actualEntry.Value);
        }

        [Fact]
        public void Read_Offset_Beyond_File_Length_Throws_ArgumentOutOfRangeException()
        {
            // Arrange
            _appender.Append(new LogEntry("key1", "val1", false));
            long invalidOffset = _context.FileStream.Length + 100; // Явно за пределами файла

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _reader.Read(invalidOffset));
            Assert.Contains("Смещение выходит за пределы", exception.Message);
        }

        [Fact]
        public void Read_All_Sequentially_File_With_Multiple_Entries_Returns_All_In_Correct_Order()
        {
            // Arrange
            _appender.Append(new LogEntry("k1", "v1", false));
            _appender.Append(new LogEntry("k2", null, true)); // Tombstone
            _appender.Append(new LogEntry("k3", "v3", false));

            // Act
            var records = _reader.ReadAllSequentially().ToList();

            // Assert
            Assert.Equal(3, records.Count);

            Assert.Equal("k1", records[0].Entry.Key);

            Assert.Equal("k2", records[1].Entry.Key);
            Assert.True(records[1].Entry.IsDeleted);

            Assert.Equal("k3", records[2].Entry.Key);
            Assert.Equal("v3", records[2].Entry.Value);
        }

        public void Dispose()
        {
            _context.Dispose();
            if (File.Exists(_tempFilePath))
                File.Delete(_tempFilePath);
        }
    }
}
