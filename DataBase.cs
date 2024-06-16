using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;

namespace Apache
{
    static class DataBase
    {
        private static string dbFileName = "logs.db";
        private static string connectionString = $"Data Source={dbFileName};Version=3;";

        public static void CreateDatabase()
        {
            if (File.Exists(dbFileName))
            {
                File.Delete(dbFileName);
            }

            SQLiteConnection.CreateFile(dbFileName);

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string createTableQuery = @"
                    CREATE TABLE Logs (
                        ip TEXT,
                        dateofrequest DATETIME,
                        request TEXT,
                        status INTEGER
                    )";

                ExecuteNonQuery(connection, createTableQuery);
            }
        }

        public static bool InsertLogs(List<Log> logs)
        {
            bool success = false;
            foreach (Log log in logs)
            {
                if (IsValidLogEntry(log))
                {
                    InsertLogEntry(log);
                    success = true;
                }
                else
                {
                    Console.WriteLine("Ошибка при вносе данных!");
                }
            }
            return success;
        }

        public static void GetLogsByFilter(DateTime? dateFrom, DateTime? dateTo, string ip, int? status)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string selectQuery = "SELECT ip, dateofrequest, request, status FROM Logs WHERE 1=1";

                if (dateFrom.HasValue && dateTo.HasValue)
                {
                    selectQuery += $" AND dateofrequest BETWEEN '{dateFrom.Value:yyyy-MM-dd}' AND '{dateTo.Value:yyyy-MM-dd}'";
                }
                else if (dateFrom.HasValue)
                {
                    selectQuery += $" AND date(dateofrequest) = '{dateFrom.Value:yyyy-MM-dd}'";
                }
                else if (dateTo.HasValue)
                {
                    selectQuery += $" AND date(dateofrequest) = '{dateTo.Value:yyyy-MM-dd}'";
                }

                if (!string.IsNullOrEmpty(ip))
                {
                    selectQuery += $" AND ip = '{ip}'";
                }

                if (status.HasValue)
                {
                    selectQuery += $" AND status = {status}";
                }

                using (var command = new SQLiteCommand(selectQuery, connection))
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string logIp = reader.GetString(0);
                        DateTime logDateOfRequest = reader.GetDateTime(1);
                        string logRequest = reader.GetString(2);
                        int logStatus = reader.GetInt32(3);

                        Console.WriteLine($"IP: {logIp}, Date: {logDateOfRequest}, Request: {logRequest}, Status: {logStatus}");
                    }
                }
            }
        }

        private static void InsertLogEntry(Log log)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string insertQuery = @"
                    INSERT INTO Logs (ip, dateofrequest, request, status)
                    VALUES (@ip, @dateofrequest, @request, @status)";

                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@ip", log.Data["%h"]);
                    command.Parameters.AddWithValue("@dateofrequest", DateTime.Parse(log.Data["%t"]));
                    command.Parameters.AddWithValue("@request", log.Data["%r"]);
                    command.Parameters.AddWithValue("@status", int.Parse(log.Data["%>s"]));

                    ExecuteNonQuery(command);
                }
            }
        }

        private static bool IsValidLogEntry(Log log)
        {
            return log.Data.ContainsKey("%h") && log.Data.ContainsKey("%t")
                && log.Data.ContainsKey("%r") && log.Data.ContainsKey("%>s");
        }

        private static void ExecuteNonQuery(SQLiteConnection connection, string query)
        {
            using (var command = new SQLiteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void ExecuteNonQuery(SQLiteCommand command)
        {
            using (command)
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
