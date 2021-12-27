using Solria.SAFT.Parser.Models;
using System;
using System.Collections.Generic;

namespace Solria.SAFT.Parser.Services
{
    public interface IDatabaseService
    {
        void InitDatabase();
        string GetAppVersion();
        void LogException(Exception ex, string controller);
        void LogInfo(string message, string controller);
        void ClearLogs();
        IEnumerable<PemFile> GetPemFiles();
        void DeletePemFile(int id);
        void UpdatePemFiles(IEnumerable<PemFile> pemFiles);
    }
}
