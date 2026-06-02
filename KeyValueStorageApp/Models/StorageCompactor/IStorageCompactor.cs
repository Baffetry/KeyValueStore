using KeyValueStorageApp.Models.DiskStorage;

namespace KeyValueStorageApp.Models.StorageCompactorModule
{
    public interface IStorageCompactor
    {
        Dictionary<string, long> Compact(
            string sourceFilePath,
            IEnumerable<KeyValuePair<string, long>> currentSnapshot,
            ILogReader sourceReader,
            ILogEntrySerializer serializer);
    }
}
