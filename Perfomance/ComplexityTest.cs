using KeyValueStorageApp.Models.Core;
using KeyValueStorageApp.Models.DiskStorage;
using KeyValueStorageApp.Models.MemoryIndexModule;
using System.Diagnostics;

namespace KeyValueStorageApp.Perfomance
{
    public class ComplexityTest
    {
        private const string DbPath = "complexity_benchmark.db";
        private const int BatchSize = 1000;

        public static void Run()
        {
            if (File.Exists(DbPath))
                File.Delete(DbPath);

            // Размеры БД, на которых будем делать замеры
            int[] thresholds = { 10_000, 100_000, 500_000, 1_000_000 };

            using var context = new StorageContext(DbPath);
            var serializer = new StandardLogEntrySerializer();
            var appender = new LogAppender(context, serializer);

            // Инициализация подсистемы оперативной памяти
            var memoryIndex = new MemoryIndex();

            PrintHeader();

            // Прогрев JIT-компилятора
            JitWarmUp(appender, memoryIndex);

            int currentRecordCount = 0;

            foreach (var targetSize in thresholds)
            {
                // 1. "Нагрев" базы
                WarmUpDatabase(targetSize, ref currentRecordCount, appender, memoryIndex);

                // 2. Проверка скорость записи на O(1) 
                double writeTimeMs = MeasureWritePerformance(BatchSize, ref currentRecordCount, appender);

                // 3. Провекра скорости чтения на O(1)
                double readTimeMs = MeasureReadPerformance(BatchSize, targetSize, memoryIndex);

                // Вывод результатов
                PrintResult(targetSize, writeTimeMs, readTimeMs);
            }
        }

        #region Вывод консоли
        private static void PrintHeader()
        {
            Console.WriteLine("Доказательство асимптотической сложности O(1)\n");
            Console.WriteLine($"{"Размер БД (записей)",-20} | {"Запись 1000 элементов в файл",-30} | {"Чтение 1000 адресов из RAM",-30}");
            Console.WriteLine(new string('-', 85));
        }

        private static void PrintResult(int targetSize, double writeTimeMs, double readTimeMs)
        {
            Console.WriteLine($"{targetSize,-20:N0} | {writeTimeMs,-27:F4} мс | {readTimeMs,-27:F4} мс");
        }
        #endregion

        #region Прогрев компилятора и базы
        private static void JitWarmUp(LogAppender appender, MemoryIndex memoryIndex)
        {
            var dummyEntry = new LogEntry("warmup_key", "warmup_value", false);

            appender.Append(dummyEntry);
            memoryIndex.Set("warmup_key", 0);
            memoryIndex.TryGetOffset("warmup_key", out _);
        }

        private static void WarmUpDatabase(int targetSize, ref int currentRecordCount, LogAppender appender, MemoryIndex memoryIndex)
        {
            while (currentRecordCount < targetSize)
            {
                string key = $"key_{currentRecordCount}";
                var entry = new LogEntry(key, "dummy_payload_data", false);

                long offset = appender.Append(entry);
                memoryIndex.Set(key, offset);

                currentRecordCount++;
            }
        }
        #endregion

        #region Оценка производительности
        private static double MeasureWritePerformance(int batchSize, ref int currentRecordCount, LogAppender appender)
        {
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < batchSize; i++)
            {
                string key = $"key_{currentRecordCount + i}";
                var entry = new LogEntry(key, "benchmark_data", false);
                appender.Append(entry);
            }
            stopwatch.Stop();

            // Увеличиваем счетчик на добавленную пачку
            currentRecordCount += batchSize;

            return stopwatch.Elapsed.TotalMilliseconds;
        }

        private static double MeasureReadPerformance(int batchSize, int targetSize, MemoryIndex memoryIndex)
        {
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < batchSize; i++)
            {
                // Берем ключи из середины словаря
                string keyToFind = $"key_{targetSize / 2 + i}";
                memoryIndex.TryGetOffset(keyToFind, out long _);
            }
            stopwatch.Stop();

            return stopwatch.Elapsed.TotalMilliseconds;
        }
        #endregion
    }
}