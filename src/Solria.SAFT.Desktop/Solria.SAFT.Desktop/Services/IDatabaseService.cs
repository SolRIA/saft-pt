using System;
using System.Collections.Generic;

namespace Solria.SAFT.Desktop.Services
{
    public interface IDatabaseService
    {
        void InitDatabase();
        string GetAppVersion();
        void LogException(Exception ex, string controller);
        void LogInfo(string message, string controller);
        void ClearLogs();
        void AddRecentFile(string file);
        IEnumerable<string> GetRecentFiles();
        void ClearRecentFiles();
        T GetPreferences<T>(string key, T defaultValue);
        void UpdatePreferences(string key, object value);
        T GetJsonPreferences<T>(string key, T defaultValue);
        void UpdateJsonPreferences(string key, object value);
        void DeletePreferences(string key);
    }
}
