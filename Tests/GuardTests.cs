using KeyValueStorageApp.Models.Common;

namespace KeyValueStorageApp.Tests
{
    /// <summary>
    /// Модульные тесты для проверки Guard.
    /// </summary>
    public class GuardTests
    {
        #region Key
        [Fact]
        public void Key_Verification_Null_Or_Empty_Key_Throws_Exception()
        {
            Assert.Throws<ArgumentNullException>(() => Guard.KeyVerification(null));
            Assert.Throws<ArgumentException>(() => Guard.KeyVerification("   "));
        }

        [Fact]
        public void Key_Verification_Valid_Key_Does_Not_Throw()
        {
            string validKey = "user_123_data";
            var exception = Record.Exception(() => Guard.KeyVerification(validKey));
            Assert.Null(exception);
        }

        [Fact]
        public void Key_Verification_Null_Key_Throws_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Guard.KeyVerification(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void Key_Verification_Empty_Or_White_Space_Key_Throws_ArgumentException(string invalidKey)
        {
            var exception = Assert.Throws<ArgumentException>(() => Guard.KeyVerification(invalidKey));
            Assert.Contains("Ключ не может быть пустым", exception.Message);
        }
        #endregion

        #region Offset
        [Fact]
        public void Offset_Verification_Negative_Offset_Throws_Exception()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Guard.OffsetVerification(-5));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100000)]
        [InlineData(long.MaxValue)]
        public void Offset_Verification_Valid_Offset_Does_Not_Throw(long validOffset)
        {
            var exception = Record.Exception(() => Guard.OffsetVerification(validOffset));
            Assert.Null(exception);
        }

        [Fact]
        public void Offset_Verification_Negative_Offset_Throws_ArgumentOutOfRangeException()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Guard.OffsetVerification(-1));
            Assert.Contains("не может быть отрицательным", exception.Message);
        }
        #endregion

        #region File path
        [Fact]
        public void File_Path_Verification_Invalid_Chars_Throws_Exception()
        {
            var invalidPath = "invalid" + Path.GetInvalidPathChars()[0] + "path.db";
            Assert.Throws<ArgumentException>(() => Guard.FilePathVerification(invalidPath));
        }

        [Theory]
        [InlineData("database.db")]
        [InlineData(@"C:\data\database.db")]
        [InlineData("./local_storage.dat")]
        public void File_Path_Verification_Valid_Path_Does_Not_Throw(string validPath)
        {
            var exception = Record.Exception(() => Guard.FilePathVerification(validPath));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void File_Path_Verification_Null_Or_White_Space_ThrowsArgumentException(string invalidPath)
        {
            Assert.Throws<ArgumentException>(() => Guard.FilePathVerification(invalidPath));
        }

        [Fact]
        public void File_Path_Verification_Invalid_Chars_Throws_ArgumentException()
        {
            // Получаем первый запрещенный символ для ОС
            char invalidChar = Path.GetInvalidPathChars()[0];
            string invalidPath = $"data{invalidChar}base.db";

            var exception = Assert.Throws<ArgumentException>(() => Guard.FilePathVerification(invalidPath));
            Assert.Contains("содержит недопустимые символы", exception.Message);
        }
        #endregion
    }
}