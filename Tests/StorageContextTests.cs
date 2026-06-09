using KeyValueStorageApp.Models.DiskStorage;

namespace KeyValueStorageApp.Tests
{
    public class StorageContextTests : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly string _dbFilePath;

        public StorageContextTests()
        {
            // Генерируем уникальный путь во временной папке ОС
            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _dbFilePath = Path.Combine(_tempDirectory, "test_context.db");
        }

        [Fact]
        public void Init_File_And_Directory_Do_Not_Exist_Creates_Them()
        {
            // Act
            using (var context = new StorageContext(_dbFilePath))
            {
                // Assert
                Assert.True(Directory.Exists(_tempDirectory));
                Assert.True(File.Exists(_dbFilePath));
                Assert.NotNull(context.FileStream);
                Assert.NotNull(context.Reader);
                Assert.NotNull(context.Writer);
            }
        }

        [Fact]
        public void Ensure_Not_Disposed_After_Dispose_Throws_ObjectDisposedException()
        {
            // Arrange
            var context = new StorageContext(_dbFilePath);
            context.Dispose();

            // Act & Assert
            var exception = Assert.Throws<ObjectDisposedException>(() => context.EnsureNotDisposed());
            Assert.Contains(nameof(StorageContext), exception.ObjectName);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
        }
    }
}
