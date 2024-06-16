using System;
using System.IO;

namespace Apache
{
    class Config
    {
        public string FilesDir { get; private set; }
        public string Ext { get; private set; }
        public string Format { get; private set; }

        private Config() { }

        public static Config LoadFromFile(string configPath)
        {
            try
            {
                if (!File.Exists(configPath))
                {
                    Console.WriteLine("Файл конфигурации не найден!");
                    return null;
                }

                var config = new Config();
                var lines = File.ReadAllLines(configPath);

                foreach (var line in lines)
                {
                    var parts = line.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2) continue;

                    var key = parts[0].Trim().ToLower();
                    var value = parts[1].Trim();

                    switch (key)
                    {
                        case "files_dir":
                            config.FilesDir = value;
                            break;
                        case "ext":
                            config.Ext = value;
                            break;
                        case "format":
                            config.Format = value;
                            break;
                    }
                }

                if (string.IsNullOrEmpty(config.FilesDir) || string.IsNullOrEmpty(config.Ext) || string.IsNullOrEmpty(config.Format))
                {
                    Console.WriteLine("Ошибка: не все необходимые параметры заданы в конфигурационном файле.");
                    return null;
                }

                return config;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке конфигурации: {ex.Message}");
                return null;
            }
        }
    }
}
