using KeyValueStorageApp.Models.Core;

namespace KeyValueStorageApp.Models.DiskStorage
{
    /// <summary>
    /// Интерфейс отвечает исключительно за запись логов.
    /// </summary>
    public interface ILogAppender
    {
        /// <summary>
        /// Дописывает записть строго в конце файла (append-only) и возвращает байтовое смещение
        /// начала этой записи.
        /// </summary>
        /// <param name="entry">Объект записи (ключ, значение, маркер удаления).</param>
        /// <returns>Положительное байтовое смещение.</returns>
        long Append(LogEntry entry);
    }
}
