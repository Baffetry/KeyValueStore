using KeyValueStorageApp.Models.Common;
using KeyValueStorageApp.Models.Core;
using KeyValueStorageApp.Models.DiskStorage;
using KeyValueStorageApp.Models.MemoryIndexModule;
using KeyValueStorageApp.Models.StorageCompactorModule;

namespace KeyValueStorageApp.Models.KeyValueStorageModule
{
    public class KeyValueStore : IKeyValueStore, IDisposable
    {
        private readonly string _dbFilePath;
        private readonly IMemoryIndex _index;
        private readonly IStorageCompactor _compactor;
        private readonly ILogEntrySerializer _serializer;

        private StorageContext _storageContext;
        private ILogAppender _appender;
        private ILogReader _reader;

        public KeyValueStore(string dbFilePath)
        {
            // Валидация пути к файлу
            Guard.FilePathVerification(dbFilePath);

            _dbFilePath = dbFilePath;

            // Инициализация сервисов и стратегий
            _serializer = new StandardLogEntrySerializer();
            _compactor = new StorageCompactor();
            _index = new MemoryIndex();

            // Переподключаем потоки чтения и записи
            InitDiskComponents();

            // Восстановление состояний из файла при запуске
            RecoverState();
        }

        #region Private methods
        /// <summary>
        ///  Переподключает потоки чтения и записи для работы с файлом на диске.
        /// </summary>
        private void InitDiskComponents()
        {
            _storageContext = new StorageContext(_dbFilePath);
            _appender = new LogAppender(_storageContext, _serializer);
            _reader = new LogReader(_storageContext, _serializer);
        }

        private void RecoverState()
        {
            foreach (var record in _reader.ReadAllSequentially())
            {
                if (record.Entry.IsDeleted)
                    _index.Remove(record.Entry.Key);
                else
                    _index.Set(record.Entry.Key, record.Offset);
            }
        }
        #endregion

        #region IKeyValueStore
        public void Set(string key, string value)
        {
            Guard.KeyVerification(key);

            var entry = new LogEntry(key, value, isDeleted: false);

            long offset = _appender.Append(entry);
            _index.Set(key, offset);
        }

        public string Get(string key)
        {
            Guard.KeyVerification(key);

            if (_index.TryGetOffset(key, out long offset))
            {
                var entry = _reader.Read(offset);

                if (!entry.IsDeleted)
                    return entry.Value;
            }
            throw new KeyNotFoundException($"Ключ '{key}' не найден.");
        }

        public void Delete(string key)
        {
            Guard.KeyVerification(key);

            if (_index.TryGetOffset(key, out long offset))
            {
                var tombstone = new LogEntry(key, null, isDeleted: true);

                _appender.Append(tombstone);
                _index.Remove(key);
            }
        }

        public void Compact()
        {
            string tempFilePath = _dbFilePath + ".tmp";
            string backupFilePath = _dbFilePath + ".bak";

            // Получаем текущее состояние данных
            var currentSnapshot = _index.GetSnapshot();
            // Передаём работу по перезаписи данных сервису StorageCompactor
            var newOffsets = _compactor.Compact(_dbFilePath, currentSnapshot, _reader, _serializer);

            // Процесс физической подмены файлов
            lock (_storageContext.SyncRoot)
            {
                // Освобождаем поток основного файла
                _storageContext.Dispose();
                // Подменяем файлы на диске
                RotatePhysicalFiles(tempFilePath, backupFilePath);
                // Переподключаем потоки чтения и записи к уплотнённому файлу
                InitDiskComponents();
                // Обновляем смещения на диске
                UpdateMemoryIndex(newOffsets);
            }
        }

        /// <summary>
        /// Безопасная ротация файлов на диске.
        /// </summary>
        private void RotatePhysicalFiles(string tempFilePath, string  backupFilePath)
        {
            // Удаляем предыдущий бэкап, если она есть
            if (File.Exists(backupFilePath)) File.Delete(backupFilePath);
            // Переносим данные в резервную копию
            if (File.Exists(_dbFilePath)) File.Move(_dbFilePath, backupFilePath);
            // Переносим уплотнённый файл в основной
            File.Move(tempFilePath, _dbFilePath);
            // Удаляем бэкап после успешного переподключения
            if (File.Exists(backupFilePath)) File.Delete(backupFilePath);
        }

        /// <summary>
        /// Синхронизирует состояние оперативной памяти с новой структурой диска.
        /// </summary>
        private void UpdateMemoryIndex(Dictionary<string, long> newOffset)
        {
            _index.Clear();
            foreach (var kvp in newOffset)
                _index.Set(kvp.Key, kvp.Value);
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            _storageContext?.Dispose();
        }
        #endregion
    }
}