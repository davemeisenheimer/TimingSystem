using System;
using System.IO;
using System.Text.Json;

namespace TrailMeisterDb
{
    public static class AppSettingsService
    {
        private static readonly string SettingsDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TrailMeister");

        private static readonly string SettingsPath =
            Path.Combine(SettingsDir, "settings.json");

        public static void Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                    AppSettings.Current = JsonSerializer.Deserialize<AppSettings>(
                        File.ReadAllText(SettingsPath)) ?? new AppSettings();
            }
            catch
            {
                AppSettings.Current = new AppSettings();
            }
        }

        public static void Save()
        {
            Directory.CreateDirectory(SettingsDir);
            File.WriteAllText(SettingsPath,
                JsonSerializer.Serialize(AppSettings.Current,
                    new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
