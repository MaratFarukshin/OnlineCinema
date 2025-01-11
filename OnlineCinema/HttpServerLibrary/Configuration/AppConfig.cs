using System.Text.Json;

namespace HttpServerLibrary.Configuration
{
    public sealed class AppConfig
    {
        public static string Domain { get; set; } = "localhost";
        public static uint Port { get; set; } = 6529;
        public static string StaticDirectoryPath { get; set; } = "public\\"; 

        private AppConfig()
        {
            var config = ReadJsonFile();
        }

        public async Task ReadJsonFile(AppConfig config = null)
        {
            if (File.Exists("config.json"))
            {
                var fileConfig = await File.ReadAllTextAsync("config.json");
                config = JsonSerializer.Deserialize<AppConfig>(fileConfig);
            }
            else
            {
                Console.WriteLine("файл конфигурации сервера 'config.json' не найден");
                config = new AppConfig();
            }
        }
    }
}