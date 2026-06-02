using KeyValueStorageApp.Models.Common;
using System.Collections.Concurrent;

namespace KeyValueStorageApp.Models.MemoryIndexModule
{
    public class MemoryIndex : IMemoryIndex
    {
        // Ключ - позиция первого байта записи в файле данных
        private readonly ConcurrentDictionary<string, long> _index 
            = new ConcurrentDictionary<string, long>();

        public void Set(string key, long offset)
        {
            // Валидация параметров
            Guard.KeyVerification(key);
            Guard.OffsetVerification(offset);

            _index[key] = offset;
        }

        public bool TryGetOffset(string key, out long offset)
        {
            Guard.KeyVerification(key);
            return _index.TryGetValue(key, out offset);
        }

        public void Remove(string key)
        {
            Guard.KeyVerification(key);
            _index.TryRemove(key, out _);
        }

        public IEnumerable<KeyValuePair<string, long>> GetSnapshot()
        {
            // ToArray() создает копию данных в текущем состоянии.
            return _index.ToArray();
        }

        public void Clear()
        {
            _index.Clear();
        }
    }
}
