using KeyValueStorageApp.Models.Core;
using KeyValueStorageApp.Models.DiskStorage;

namespace KeyValueStorageApp.Tests
{
    /// <summary>
    /// Модульные тесты для проверки корректности сериализации.
    /// </summary>
    public class StandardLogEntrySerializerTests
    {
        #region IsDeleted = false
        [Fact]
        public void Serialize_And_Deserialize_Valid_Entry_Returns_Equivalent_Object()
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

        [Theory]
        [InlineData("test_key", "test_value")]                           // Стандартный случай
        [InlineData("key_with_empty_value", "")]                         // Пустое значение
        [InlineData("complex_!@#$%^&*()", "complex_value\n\r\t")]        // Спецсимволы и переносы строк
        public void Serialize_And_Deserialize_Valid_Active_Entries_Returns_Equivalent_Object(string key, string value)
        {
            // Arrange
            var serializer = new StandardLogEntrySerializer();
            var originalEntry = new LogEntry(key, value, isDeleted: false);

            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);
            using var reader = new BinaryReader(memoryStream);

            // Act
            serializer.Serialize(writer, originalEntry);

            // Сбрасываем курсор потока в начало перед чтением
            memoryStream.Position = 0;
            var deserializedEntry = serializer.Deserialize(reader);

            // Assert
            Assert.Equal(originalEntry.Key, deserializedEntry.Key);
            Assert.Equal(originalEntry.Value, deserializedEntry.Value);
            Assert.False(deserializedEntry.IsDeleted);
        }
        #endregion

        #region IsDeleted = true [tombstones]
        [Fact]
        public void Serialize_And_Deserialize_Tombstone_Returns_Null_Value()
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

        [Theory]
        [InlineData("deleted_key")]
        [InlineData("уничтоженный_ключ_1")]
        [InlineData("")] // Сериализатор не проверяет ключи (это делает Guard), он должен просто писать данные
        public void Serialize_And_Deserialize_Tombstone_Entries_Returns_Null_Value(string key)
        {
            // Arrange
            var serializer = new StandardLogEntrySerializer();

            // Для удаленной записи Value = null
            var tombstone = new LogEntry(key, null, isDeleted: true);

            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);
            using var reader = new BinaryReader(memoryStream);

            // Act
            serializer.Serialize(writer, tombstone);
            memoryStream.Position = 0;
            var deserializedEntry = serializer.Deserialize(reader);

            // Assert
            Assert.Equal(tombstone.Key, deserializedEntry.Key);
            Assert.Null(deserializedEntry.Value); // Сериализатор должен восстановить именно null
            Assert.True(deserializedEntry.IsDeleted);
        }
        #endregion

        #region BinaryWriter
        [Fact]
        public void Serialize_Active_Entry_With_Null_Value_Throws_ArgumentNullException()
        {
            // Arrange
            var serializer = new StandardLogEntrySerializer();

            // Живая запись без значения
            // BinaryWriter.Write(string) не поддерживает null и должен выбросить исключение.
            var invalidEntry = new LogEntry("valid_key", null, isDeleted: false);

            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => serializer.Serialize(writer, invalidEntry));
            Assert.Contains("Value cannot be null", exception.Message);
        }
        #endregion
    }
}