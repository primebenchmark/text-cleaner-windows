using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace TextCleaner
{
    public class AppSettings
    {
        public bool IsDarkTheme { get; set; } = true;
        public List<CharacterRule> Rules { get; set; } = GetDefaultRules();
        public int WindowWidth { get; set; } = 1000;
        public int WindowHeight { get; set; } = 600;
        public double TextFontSize { get; set; } = 13;

        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TextCleaner", "settings.json");

        public static List<CharacterRule> GetDefaultRules()
        {
            return new List<CharacterRule>
            {
                new() { Find = "&nbsp;", ReplaceWith = "" },
                new() { Find = "\u00A0", ReplaceWith = "" },
                new() { Find = "$rightarrow$", ReplaceWith = "➜" },
                new() { Find = "`", ReplaceWith = "" },
                new() { Find = "\\", ReplaceWith = "" },
                new() { Find = "*", ReplaceWith = "" },
                new() { Find = "-", ReplaceWith = "" },
                new() { Find = "#", ReplaceWith = "❱" },
                new() { Find = "|", ReplaceWith = "❱" },
            };
        }

        public void Save()
        {
            var dir = Path.GetDirectoryName(SettingsPath)!;
            Directory.CreateDirectory(dir);
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch { /* fall through to defaults */ }
            return new AppSettings();
        }
    }
}
