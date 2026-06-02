namespace KeyValueStorageApp.Models.MemoryIndexModule
{
    public interface IMemoryIndex
    {
        /// <summary>
        /// Сохраняет или перезаписывает байтовое смещение для конкретного ключа.
        /// </summary>
        void Set(string key, long offset);
        /// <summary>
        /// Пытается извлечь смещение для указанного ключа.
        /// </summary>
        /// <param name="key">Искомый ключ.</param>
        /// <param name="offset">Выходной параметр: байтовое смещение в файле.</param>
        /// <returns>true, если ключ найден в таблице индексов, иначе false.</returns>
        bool TryGetOffset(string key, out long offset);
        /// <summary>
        /// Физически удаляет упоминание ключа из карты индексов.
        /// </summary>
        void Remove(string key);
        /// <summary>
        /// Поддерживает процесс уплотнения 
        /// </summary>
        IEnumerable<KeyValuePair<string, long>> GetSnapshot();
        /// <summary>
        /// Отчищает хеш-таблицу
        /// </summary>
        void Clear();
    }
}
