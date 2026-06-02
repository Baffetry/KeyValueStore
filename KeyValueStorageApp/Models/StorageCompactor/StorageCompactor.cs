using KeyValueStorageApp.Models.DiskStorage;

namespace KeyValueStorageApp.Models.StorageCompactorModule
{
    public class StorageCompactor : IStorageCompactor
    {
        public Dictionary<string, long> Compact(
            string sourceFilePath, 
            IEnumerable<KeyValuePair<string, long>> currentSnapshot, 
            ILogReader sourceReader, 
            ILogEntrySerializer serializer)
        {
            string tempFilePath = sourceFilePath + ".tmp";
            var newOffsets = new Dictionary<string, long>();

            using (var tempContext = new StorageContext(tempFilePath))
            {
                var tempAppender = new LogAppender(tempContext, serializer);

                foreach (var kvp in currentSnapshot)
                {
                    var entry = sourceReader.Read(kvp.Value);

                    if (!entry.IsDeleted)
                    {
                        long newOffset = tempAppender.Append(entry);
                        newOffsets[entry.Key] = newOffset;
                    }
                }
            }

            return newOffsets;
        }
    }
}
