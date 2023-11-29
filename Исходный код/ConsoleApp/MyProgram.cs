using CL;

namespace ConsoleApp
{
    public class MyProgram
    {
        static async Task Main(string[] args)
        {
            const int N = 3, minNumberOfSongs = 0;
            var random = new Random();

            // Создание коллекции из 1000 объектов
            var singers = new List<Singer>();

            for (int i = 0; i < N; i++)
            {
                singers.Add(new Singer
                {
                    Id = i + 1,
                    Name = $"Singer {i + 1}",
                    NumberOfSongs = random.Next(0, 10)
                });
            }

            // Вывод информации о начале работы
            Console.WriteLine($"Главный поток выполнения: {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine("Начало работы.");

            var streamService = new StreamService<Singer>();
            var memoryStream = new MemoryStream();

            // Метод 1
            var progressReporter = new Progress<string>(message =>
            {
                Console.WriteLine($"Поток {Thread.CurrentThread.ManagedThreadId}: {message}");
            });
            var result1 = streamService.WriteToStreamAsync(memoryStream, singers, progressReporter);

            // Задержка между запусками методов для гарантирования последовательности
            await Task.Delay(200);
            result1.Wait();

            // Метод 2
            var result2 = streamService.CopyFromStreamAsync(memoryStream, "output.txt", progressReporter);

            // Вывод информации о запуске потоков
            Console.WriteLine("Потоки 1 и 2 запущены.");

            // Задержка между запусками методов для гарантирования последовательности
            await Task.Delay(200);
            result2.Wait();

            // Асинхронное получение статистических данных
            int statistics = await streamService.GetStatisticsAsync("output.txt", singer => singer.NumberOfSongs > minNumberOfSongs);

            // Вывод статистических данных
            Console.WriteLine($"Статистика: {statistics} объектов удовлетворяют условию.");

            Console.WriteLine("Ожидание нажатия клавиши...");
            Console.ReadLine();
        }
    }
}