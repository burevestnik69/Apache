using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Apache
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Введите команду ('parse' для парсинга логов, 'get ...' для запроса данных):");
                string command = Console.ReadLine();

                if (command == "parse")
                {
                    ParseLogsAndInsertIntoDatabase("config.txt");
                }
                else if (command.StartsWith("get"))
                {
                    GetData(command);
                }
                else
                {
                    Console.WriteLine("Неизвестная команда!");
                }

                Console.WriteLine();
            }
        }

        private static void ParseLogsAndInsertIntoDatabase(string configFile)
        {
            try
            {
                var logs = ParseLogs(configFile);

                if (logs == null)
                {
                    Console.WriteLine("Не удалось считать данные.");
                    return;
                }

                DataBase.CreateDatabase();
                bool isSuccess = DataBase.InsertLogs(logs);

                if (isSuccess)
                {
                    Console.WriteLine("Данные успешно получены и записаны в базу данных!");
                }
                else
                {
                    Console.WriteLine("Произошли ошибки при записи данных в базу.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private static List<Log> ParseLogs(string configFile)
        {
            var config = Config.LoadFromFile(configFile);

            if (config == null)
            {
                Console.WriteLine("Неправильно задан файл конфигурации!");
                return null;
            }

            try
            {
                var logFiles = Directory.GetFiles(config.FilesDir, $"*.{config.Ext}");

                if (logFiles.Length == 0)
                {
                    throw new Exception("В выбранной папке нет файлов с данным расширением!");
                }

                var result = new List<Log>();

                foreach (var logFile in logFiles)
                {
                    var logLines = File.ReadAllLines(logFile);

                    foreach (var logLine in logLines)
                    {
                        try
                        {
                            var logEntry = Log.Parse(logLine, config.Format);
                            result.Add(logEntry);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Ошибка при обработке строки: {e.Message}");
                        }
                    }
                }

                return result.Count > 0 ? result : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return null;
            }
        }

        private static void GetData(string commandLine)
        {
            string[] commands = commandLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);


            DateTime? dateFrom = null;
            DateTime? dateTo = null;
            string ip = null;
            int? status = null;

            for (int i = 1; i < commands.Length; i++)
            {
                string command = commands[i];

                if (int.TryParse(command, out int st) && command.Length == 3)
                {
                    status = st;
                }
                else if (command.Count(c => c == '.') == 3)
                {
                    ip = command;
                }
                else if (DateTime.TryParseExact(command, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    if (dateFrom == null)
                    {
                        dateFrom = date;
                    }
                    else
                    {
                        dateTo = date;
                    }
                }
                else
                {
                    Console.WriteLine("Неверный формат данных!");
                    return;
                }
            }

            DataBase.GetLogsByFilter(dateFrom, dateTo, ip, status);
        }
    }
}
