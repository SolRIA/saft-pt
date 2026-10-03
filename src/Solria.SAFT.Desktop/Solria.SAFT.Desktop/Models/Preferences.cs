using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SolRIA.SAFT.Desktop.Models;

public class Preferences
{
    public IList<RecentFileEntry> RecentFiles { get; set; } = new List<RecentFileEntry>();
    public string Theme { get; set; } = "System";
    public bool UseNewParser { get; set; } = true;

    private static string GetFileName()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SolRIA SAFT", "preferences.json");
    }

    public void AddRecentFile(string filePath, RecentFileType fileType)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return;

        RecentFiles ??= new List<RecentFileEntry>();

        for (int i = RecentFiles.Count - 1; i >= 0; i--)
        {
            if (string.Equals(RecentFiles[i]?.FullPath, filePath, StringComparison.OrdinalIgnoreCase))
            {
                RecentFiles.RemoveAt(i);
            }
        }

        RecentFiles.Insert(0, new RecentFileEntry { FullPath = filePath, FileType = fileType });

        while (RecentFiles.Count > 15)
        {
            RecentFiles.RemoveAt(RecentFiles.Count - 1);
        }
    }

    public void RemoveRecentFile(string filePath)
    {
        for (int i = RecentFiles.Count - 1; i >= 0; i--)
        {
            if (string.Equals(RecentFiles[i]?.FullPath, filePath, StringComparison.OrdinalIgnoreCase))
                RecentFiles.RemoveAt(i);
        }
    }

    public static void Save(Preferences preferences)
    {
        var dir = Path.GetDirectoryName(GetFileName());
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var json = System.Text.Json.JsonSerializer.Serialize(preferences, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(GetFileName(), json, Encoding.UTF8);
    }

    public static Preferences Load()
    {
        var filename = GetFileName();
        if (File.Exists(filename))
        {
            try
            {
                var json = File.ReadAllText(filename, Encoding.UTF8);
                var prefs = System.Text.Json.JsonSerializer.Deserialize<Preferences>(json);
                if (prefs != null)
                {
                    prefs.RecentFiles ??= new List<RecentFileEntry>();
                    if (string.IsNullOrWhiteSpace(prefs.Theme))
                    {
                        prefs.Theme = "System";
                    }

                    // Deduplicate existing entries and limit to 15
                    var unique = new List<RecentFileEntry>();
                    foreach (var file in prefs.RecentFiles)
                    {
                        if (!string.IsNullOrWhiteSpace(file?.FullPath) && !unique.Exists(u => string.Equals(u.FullPath, file.FullPath, StringComparison.OrdinalIgnoreCase)))
                        {
                            unique.Add(file);
                            if (unique.Count >= 15) break;
                        }
                    }
                    prefs.RecentFiles = unique;
                    return prefs;
                }
            }
            catch
            {
                // Fallback on corrupt file
            }
        }

        Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SolRIA SAFT"));

        return new Preferences { RecentFiles = new List<RecentFileEntry>(), Theme = "System", UseNewParser = true };
    }
}
