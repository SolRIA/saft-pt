using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Solria.SAFT.Desktop.Models
{
    public class Preferences
    {
        public IList<string> RecentFiles { get; set; }

        private static string GetFileName()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SolRIA SAFT", "preferences.json");
        }
        public static void Save(Preferences preferences)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(preferences);

            File.WriteAllText(GetFileName(), json, Encoding.UTF8);
        }

        public static Preferences Load()
        {
            var filename = GetFileName();
            if (File.Exists(filename))
            {
                var json = File.ReadAllText(filename, Encoding.UTF8);

                return System.Text.Json.JsonSerializer.Deserialize<Preferences>(json);
            }

            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SolRIA SAFT"));

            return new Preferences { RecentFiles = new List<string>() };
        }
    }
}
