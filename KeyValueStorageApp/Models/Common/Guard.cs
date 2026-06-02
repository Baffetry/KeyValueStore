namespace KeyValueStorageApp.Models.Common
{
    /// <summary>
    /// Статический класс для проверки входных параметров (паттерн Guard).
    /// </summary>
    public static class Guard
    {

        /// <summary>
        /// Проверка ключа на валидность.
        /// </summary>
        public static void KeyVerification(string key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key), "Ключ не может быть равен null. . .");
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Ключ не может быть пустым или состоять только из пробелов.", nameof(key));
        }

        /// <summary>
        /// Проверка адреса на валидность.
        /// </summary>
        public static void OffsetVerification(long offset)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), offset,
                    "Байтовое смещение в файле не может быть отрицательным значением.");
        }
        
        /// <summary>
        /// Проверка пути к файлу на валидность.
        /// </summary>
        public static void FilePathVerification(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Путь к файлу не может быть пустым или состоять только из пробелов.",
                    nameof(filePath));
            // Проверка на наличие запрещенных операционной системой символов
            if (filePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                throw new ArgumentException("Путь к файлу содержит недопустимые символы.",
                    nameof(filePath));
        }
    }
}