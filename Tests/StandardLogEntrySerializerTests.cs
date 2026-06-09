using KeyValueStorageApp.Models.Core;
using KeyValueStorageApp.Models.DiskStorage;

namespace KeyValueStorageApp.Tests
{
    public class StandardLogEntrySerializerTests
    {
        [Fact]
        public void SerializeAndDeserialize_ValidEntry_ReturnsEquivalentObject()
        {
            // Arrange
            var serializer = new StandardLogEntrySerializer();
            var originalEntry = new LogEntry("test_key", "test_value", isDeleted: false);

            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);
            using var reader = new BinaryReader(memoryStream);

            // Act
            serializer.Serialize(writer, originalEntry);
            memoryStream.Position = 0; // Сбрасываем курсор для чтения
            var deserializedEntry = serializer.Deserialize(reader);

            // Assert
            Assert.Equal(originalEntry.Key, deserializedEntry.Key);
            Assert.Equal(originalEntry.Value, deserializedEntry.Value);
            Assert.Equal(originalEntry.IsDeleted, deserializedEntry.IsDeleted);
        }

        [Fact]
        public void SerializeAndDeserialize_Tombstone_ReturnsNullValue()
        {
            // Arrange
            var serializer = new StandardLogEntrySerializer();
            var tombstone = new LogEntry("deleted_key", null, isDeleted: true);

            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);
            using var reader = new BinaryReader(memoryStream);

            // Act
            serializer.Serialize(writer, tombstone);
            memoryStream.Position = 0;
            var deserializedEntry = serializer.Deserialize(reader);

            // Assert
            Assert.Equal(tombstone.Key, deserializedEntry.Key);
            Assert.Null(deserializedEntry.Value);
            Assert.True(deserializedEntry.IsDeleted);
        }
    }
}