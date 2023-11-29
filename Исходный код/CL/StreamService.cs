using System.IO;
using System.Text;
using System.Text.Json;

namespace CL
{
    public class StreamService<T>
    {
        private readonly object lockObject = new object();
        private MemoryStream memoryStream = new MemoryStream();

        /// <summary>
        /// Записывает коллекцию data в поток stream
        /// </summary>
        public async Task WriteToStreamAsync(Stream stream, IEnumerable<T> data, IProgress<string> progress)
        {
            int totalCount = data.Count();
            int currentCount = 0;

            foreach (var item in data)
            {
                // Имитация медленной записи
                await Task.Delay(1500);

                // Оповещение о начале записи
                progress.Report($"Начало записи элемента {item} в поток. Поток выполнения: {Thread.CurrentThread.ManagedThreadId}");

                // Преобразование объекта в массив байт
                byte[] itemBytes = await SerializeItem(item);

                //// Запись элемента в поток
                lock (lockObject)
                {
                    // Здесь происходит запись элемента в поток stream
                    stream.WriteAsync(itemBytes, 0, itemBytes.Length);
                }

                currentCount++;

                // Оповещение о проценте выполнения
                double progressPercentage = (double)currentCount / totalCount * 100;
                progress.Report($"Прогресс записи: {progressPercentage:F2}% в поток. Поток выполнения: {Thread.CurrentThread.ManagedThreadId}");
            }

            // Оповещение о завершении записи
            progress.Report($"Запись в поток завершена. Поток выполнения: {Thread.CurrentThread.ManagedThreadId}");
            memoryStream = (MemoryStream)stream;
        }

        /// <summary>
        /// Копирует информацию из потока stream в файл с именем fileName
        /// </summary>
        public async Task CopyFromStreamAsync(Stream stream, string filename, IProgress<string> progress)
        {
            // Оповещение о начале чтения
            progress.Report($"Начало чтения из потока в файл {filename}. Поток выполнения: {Thread.CurrentThread.ManagedThreadId}");

            // Преобразование потока в массив байт
            byte[] buffer = await ReadStreamAsync(stream, progress);

            // Запись массива байт в файл
            await WriteToFileAsync(filename, buffer, progress);

            // Оповещение о завершении чтения
            progress.Report($"Чтение из потока в файл {filename} завершено. Поток выполнения: {Thread.CurrentThread.ManagedThreadId}");
        }

        /// <summary>
        /// Считывает объекты типа Т из файла с именем filename и возвращает количество объектов, 
        /// удовлетворяющих условию filter
        /// </summary>
        public async Task<int> GetStatisticsAsync(string fileName, Func<Singer, bool> filter)
        {
            try
            {
                var singers = await ReadSingersFromFileAsync(fileName);

                // Применяем фильтр и возвращаем количество объектов, удовлетворяющих условию
                return singers.Count(filter);
            }
            catch (Exception ex)
            {
                // Обработка ошибок, например, логирование
                Console.WriteLine($"An error occurred: {ex.Message}");
                return -1;
            }
        }

        private async Task<byte[]> SerializeItem(T item)
        {
            string itemString = $"{item}\n";
            return await Task.FromResult(Encoding.UTF8.GetBytes(itemString));
        }

        private async Task<byte[]> ReadStreamAsync(Stream stream, IProgress<string> progress)
        {
            byte[] buffer = new byte[16 * 1024];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                memoryStream.Write(buffer, 0, bytesRead);

                // Оповещение о проценте выполнения
                double progressPercentage = (double)memoryStream.Length / stream.Length * 100;
                progress.Report($"Прогресс чтения: {progressPercentage:F2}% из потока. Поток выполнения: {Thread.CurrentThread.ManagedThreadId}");
            }

            return memoryStream.ToArray();
        }

        private async Task WriteToFileAsync(string filename, byte[] data, IProgress<string> progress)
        {
            // Запускаем запись в отдельном потоке
            await Task.Run(() =>
            {
                // Блокируем выполнение до завершения записи
                lock (lockObject)
                {
                    using (var fileStream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        fileStream.Write(data, 0, data.Length);

                        // Оповещение о проценте выполнения
                        double progressPercentage = (double)data.Length / data.Length * 100;
                        progress.Report($"Прогресс записи в файл: {progressPercentage:F2}%. Поток выполнения: {Thread.CurrentThread.ManagedThreadId}");
                    }
                }
            });
        }

        private async Task<List<Singer>> ReadSingersFromFileAsync(string fileName)
        {
            var singers = new List<Singer>();

            using (var reader = new StreamReader(fileName))
            {
                string line;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var singer = ParseSinger(line);

                    if (singer != null)
                    {
                        singers.Add(singer);
                    }
                }
            }

            return singers;
        }

        private Singer ParseSinger(string line)
        {
            string[] parts = line.Split(';');

            if (parts.Length == 3 &&
                int.TryParse(parts[0], out int id) &&
                int.TryParse(parts[2], out int numberOfSongs))
            {
                return new Singer
                {
                    Id = id,
                    Name = parts[1],
                    NumberOfSongs = numberOfSongs
                };
            }

            // В случае некорректного формата строки, можно вернуть null или выбросить исключение
            return null;
        }
    }
}