using KeyValueStorageApp.Models.Core;

namespace KeyValueStorageApp.Models.DiskStorage
{
    public interface ILogEntrySerializer
    {
        /// <summary>
        /// Сериализует объект LogEntry в бинарный поток.
        /// </summary>
        void Serialize(BinaryWriter writer, LogEntry entry);
        /// <summary>
        /// Читает бинарные данные из потока и восстанавливает объект LogEntry.
        /// </summary>
        LogEntry Deserialize(BinaryReader reader);
    }
}
