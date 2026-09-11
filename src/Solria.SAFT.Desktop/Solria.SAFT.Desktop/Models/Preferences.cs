using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SolRIA.SAFT.Desktop.Models;

public class Preferences
{
    public IList<string> RecentFiles { get; set; } = new List<string>();
    public string Theme { get; set; } = "System";
    public bool UseNewParser { get; set; } = true;

    private static string GetFileName()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SolRIA SAFT", "preferences.json");
    }

    public void AddRecentFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return;

        RecentFiles ??= new List<string>();

        for (int i = RecentFiles.Count - 1; i >= 0; i--)
        {
            if (string.Equals(RecentFiles[i], filePath, StringComparison.OrdinalIgnoreCase))
            {
                RecentFiles.RemoveAt(i);
            }
        }

        RecentFiles.Insert(0, filePath);

        while (RecentFiles.Count > 15)
        {
            RecentFiles.RemoveAt(RecentFiles.Count - 1);
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
                    prefs.RecentFiles ??= new List<string>();
                    if (string.IsNullOrWhiteSpace(prefs.Theme))
                    {
                        prefs.Theme = "System";
                    }

                    // Deduplicate existing entries and limit to 15
                    var unique = new List<string>();
                    foreach (var file in prefs.RecentFiles)
                    {
                        if (!string.IsNullOrWhiteSpace(file) && !unique.Exists(u => string.Equals(u, file, StringComparison.OrdinalIgnoreCase)))
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

        return new Preferences { RecentFiles = new List<string>(), Theme = "System", UseNewParser = true };
    }
}
