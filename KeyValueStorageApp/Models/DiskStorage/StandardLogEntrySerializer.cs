using KeyValueStorageApp.Models.Core;

namespace KeyValueStorageApp.Models.DiskStorage
{
    /// <summary>
    /// Стандартная реализация сериализатора.
    /// Жёсткий порядок байтов: [Флаг удаления] -> [Ключ] -> [Значение]
    /// </summary>
    public class StandardLogEntrySerializer : ILogEntrySerializer
    {
        public void Serialize(BinaryWriter writer, LogEntry entry)
        {
            writer.Write(entry.IsDeleted);
            writer.Write(entry.Key);

            if (!entry.IsDeleted)
                writer.Write(entry.Value);
        }

        public LogEntry Deserialize(BinaryReader reader)
        {
            bool isDeleted = reader.ReadBoolean();
            string key = reader.ReadString();
            string value = isDeleted ? null : reader.ReadString();

            return new LogEntry(key, value, isDeleted);
        }
    }
}
