using KeyValueStorageApp.Models.Common;

namespace KeyValueStorageApp.Tests
{
    public class GuardTests
    {
        [Fact]
        public void KeyVerification_NullOrEmptyKey_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => Guard.KeyVerification(null));
            Assert.Throws<ArgumentException>(() => Guard.KeyVerification("   "));
        }

        [Fact]
        public void OffsetVerification_NegativeOffset_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Guard.OffsetVerification(-5));
        }

        [Fact]
        public void FilePathVerification_InvalidChars_ThrowsException()
        {
            var invalidPath = "invalid" + Path.GetInvalidPathChars()[0] + "path.db";
            Assert.Throws<ArgumentException>(() => Guard.FilePathVerification(invalidPath));
        }
    }
}