namespace KeyValueStorageApp.Models.KeyValueStorageModule
{
    public interface IKeyValueStore
    {
        /// <summary>
        /// Сохраняет или обновляет значение по указанному ключу.
        /// </summary>
        void Set(string key, string value);
        /// <summary>
        /// Извлекает значение по ключу. Выбрасывает KeyNotFoundException, если ключ не найден.
        /// </summary>
        string Get(string key);
        /// <summary>
        /// Помечает запись как удаленную (tombstone).
        /// </summary>
        void Delete(string key);
        /// <summary>
        /// Запускает фоновое уплотнение логов на диске, обновляя устаревшие данные.
        /// </summary>
        void Compact();
    }
}
