using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.PlatformAbstractions;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Models.DatabaseModels;
using Solria.SAFT.Desktop.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Solria.SAFT.Desktop.Services
{
    public class DatabaseService : IDatabaseService
    {
        readonly string database_filename;
        readonly string app_version;
        public DatabaseService()
        {
            database_filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SolRIA SAFT", "validator.db");
            app_version = PlatformServices.Default.Application.ApplicationVersion;
        }

        private DbConnection InitConnection()
        {
            return new SqliteConnection($"Data Source={database_filename}");
        }

        public void InitDatabase()
        {
            try
            {
                using var connection = InitConnection();
                if (File.Exists(database_filename) == false)
                {
                    var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SolRIA SAFT");
                    if (Directory.Exists(folder) == false)
                        Directory.CreateDirectory(folder);

                    FileStream fs = File.Create(database_filename);
                    fs.Close();
                }

                var versionTable = connection.QueryFirstOrDefault<int>(
                    "select COUNT(1) from sqlite_master where type = 'table' and name = 'DatabaseVersion'");

                if (versionTable <= 0)
                {
                    //no table available do the full create
                    string sqlContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "SQL", "v001.txt"));
                    string[] sqlCommands = sqlContent.Split(';');

                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    //create the tables
                    using var transaction = connection.BeginTransaction();
                    foreach (var sql in sqlCommands)
                    {
                        if (string.IsNullOrEmpty(sql))
                            continue;

                        System.Diagnostics.Debug.WriteLine($"sql update command: {sql}");
                        connection.Execute(sql, transaction: transaction);
                    }

                    transaction.Commit();
                }

                //check for updates on the database
                var versionDB = connection.QueryFirstOrDefault<int>(
                    "SELECT Version FROM DatabaseVersion ORDER BY Version DESC LIMIT 1;");

                if (versionDB < UpdateScripts.Version)
                {
                    //the database needs to update
                    var updateScripts = UpdateScripts.GetUpdateScripts(versionDB);

                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    var transaction = connection.BeginTransaction();
                    //update the database schema and data
                    if (updateScripts != null && updateScripts.Length > 0)
                    {
                        foreach (var script in updateScripts.OrderBy(s => s.Version).ThenBy(s => s.Order))
                        {
                            connection.Execute(sql: script.Sql, transaction: transaction);
                        }
                    }

                    //update the database version
                    connection.Execute(
                        "INSERT INTO DatabaseVersion (UpgradeDate,Version,AppVersion) VALUES (@UpgradeDate,@Version,@AppVersion);",
                        new { UpdateScripts.Version, UpgradeDate = DateTime.Now, AppVersion = app_version },
                        transaction);

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                LogException(ex, "InitDatabase");
            }
        }

        public string GetAppVersion()
        {
            return app_version;
        }

        public void LogException(Exception ex, string controller)
        {
            using var connection = InitConnection();
            string message = ex.Message;

            Exception innerEx = ex.InnerException;
            while (innerEx != null)
            {
                message += Environment.NewLine + innerEx;

                innerEx = innerEx.InnerException;
                connection.Execute("INSERT INTO Logs (Message,Controller) VALUES (@message, @controller);", new { message, controller });
            }
        }

        public void LogInfo(string message, string controller)
        {
            using var connection = InitConnection();
            connection.Execute("INSERT INTO Logs (Message,Controller) VALUES (@message, @controller);", new { message, controller });
        }

        public void ClearLogs()
        {
            using var connection = InitConnection();
            connection.Execute("DELETE FROM Logs;");
        }

        public void AddRecentFile(string file)
        {
            using var connection = InitConnection();
            var existing_file = connection.QueryFirstOrDefault<int>("SELECT Id FROM RecentFiles WHERE FileName=@file;", new { file });

            if (existing_file <= 0)
                connection.Execute("INSERT INTO RecentFiles (FileName) VALUES (@file);", new { file });
            else
                connection.Execute("UPDATE RecentFiles SET LastUsed = CURRENT_TIMESTAMP WHERE Id=@existing_file;", new { existing_file });
        }

        public IEnumerable<string> GetRecentFiles()
        {
            using var connection = InitConnection();
            return connection.Query<string>("SELECT FileName FROM RecentFiles ORDER BY LastUsed DESC;");
        }

        public void ClearRecentFiles()
        {
            using var connection = InitConnection();
            connection.Execute("DELETE FROM RecentFiles;");
        }

        public IEnumerable<PemFile> GetPemFiles()
        {
            using var connection = InitConnection();
            return connection.Query<PemFile>("SELECT * FROM PemFiles ORDER BY Name;");
        }
        public void UpdatePemFiles(IEnumerable<PemFile> pemFiles)
        {
            if (pemFiles == null || pemFiles.Count() == 0)
                return;

            using var connection = InitConnection();
            var update = pemFiles.Where(p => p.Id > 0).ToArray();
            var insert = pemFiles.Where(p => p.Id == 0).ToArray();

            if (update.Length > 0)
                connection.Execute(
                    "UPDATE PemFiles SET Name=@Name,PemText=@PemText,RsaSettings=@RsaSettings " +
                    "WHERE Id=@Id;", 
                    update);

            if (insert.Length > 0)
                connection.Execute(
                    "INSERT INTO PemFiles (Name,PemText,RsaSettings) VALUES (@Name,@PemText,@RsaSettings);", 
                    insert);
        }

        public T GetPreferences<T>(string key, T defaultValue)
        {
            using var connection = InitConnection();
            var preferenceValue = connection.QueryFirstOrDefault<string>(
                "SELECT `Value` FROM Preferences WHERE `Key`=@key;", new { key });

            return Preferences.Cast(preferenceValue, defaultValue);
        }

        public void UpdatePreferences(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            using var connection = InitConnection();

            if (value == null)
                connection.Execute("DELETE FROM Preferences WHERE Key=@key;", new { key, value });
            else
                connection.Execute("REPLACE INTO Preferences(Key,Value) VALUES(@key, @value);", new { key, value });
        }
        public T GetJsonPreferences<T>(string key, T defaultValue)
        {
            try
            {
                using var connection = InitConnection();
                var preferenceValue = connection.QueryFirstOrDefault<string>(
                    "SELECT `Value` FROM Preferences WHERE `Key`=@key;", new { key });

                if (string.IsNullOrWhiteSpace(preferenceValue))
                    return defaultValue;

                return JsonSerializer.Deserialize<T>(preferenceValue);
            }
            catch (Exception ex)
            {
                LogException(ex, "GetJsonPreferences");
            }

            return defaultValue;
        }

        public void UpdateJsonPreferences(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            using var connection = InitConnection();

            if (value == null)
                connection.Execute("DELETE FROM Preferences WHERE Key=@key;", new { key, value });
            else
                connection.Execute("REPLACE INTO Preferences(Key,Value) VALUES(@key, @value);", new { key, value = JsonSerializer.Serialize(value) });
        }
        public void DeletePreferences(string key)
        {
            using var connection = InitConnection();
            connection.Execute("DELETE * FROM Preferences WHERE Key = @key;", new { key });
        }
    }
}
