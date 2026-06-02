using KeyValueStorageApp.Models.Core;

namespace KeyValueStorageApp.Models.DiskStorage
{
    /// <summary>
    /// Интерфейс отвечает исключительно за чтение данных.
    /// </summary>
    public interface ILogReader
    {
        /// <summary>
        /// Выполняет произвольный доступ к файлу и считывает объект записи по точному смещению.
        /// </summary>
        /// <param name="offset">Байтовое смещение в файле.</param>
        /// <returns>Десериализованный объект LogEntry.</returns>
        LogEntry Read(long offset);
        /// <summary>
        /// Осуществляет полный последовательный перебор файла.
        /// Используется при запуске базы данных для восстановления индексов в оперативной памяти.
        /// </summary>
        /// <returns>Последовательность пар (Смещение, Объект записи).</returns>
        IEnumerable<(long Offset, LogEntry Entry)> ReadAllSequentially();
    }
}
