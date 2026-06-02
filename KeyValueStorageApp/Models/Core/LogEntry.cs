namespace KeyValueStorageApp.Models.Core
{
    public class LogEntry
    {
        public string Key { get; }
        public string Value { get; }
        public bool  IsDeleted { get; }

        public LogEntry(string key, string value, bool isDeleted)
        {
            Key = key;
            Value = value;
            IsDeleted = isDeleted;
        }
    }
}