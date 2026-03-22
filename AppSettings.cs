using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TextCleaner
{
    public class AppSettings
    {
        public bool IsDarkTheme { get; set; } = true;
        public List<CharacterRule> Rules { get; set; } = GetDefaultRules();
        public int WindowWidth { get; set; } = 1000;
        public int WindowHeight { get; set; } = 600;
        public double TextFontSize { get; set; } = 13;

        // Maximum number of substitution rules to prevent abuse via crafted settings file
        private const int MaxRuleCount = 200;

        private const string RemoteRulesUrl = "https://primebenchmark.com.np/textcleaner-default-rules-d8844e72.json";
        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(10) };

        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TextCleaner", "settings.json");

        private static readonly string DefaultRulesPath = Path.Combine(
            AppContext.BaseDirectory, "textcleaner-default-rules-d8844e72.json");

        public static List<CharacterRule> GetDefaultRules()
        {
            try
            {
                if (File.Exists(DefaultRulesPath))
                {
                    var json = File.ReadAllText(DefaultRulesPath);
                    var rules = JsonSerializer.Deserialize<List<CharacterRule>>(json);
                    if (rules != null && rules.Count > 0)
                        return rules;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load default rules: {ex.Message}");
            }

            // Hardcoded fallback if JSON file is missing or invalid
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

            // Atomic write: write to temp file then move, so a crash mid-write
            // cannot corrupt the settings file
            var tempPath = SettingsPath + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, SettingsPath, overwrite: true);
        }

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                    settings.Sanitize();
                    return settings;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }
            return new AppSettings();
        }

        /// <summary>
        /// Fetches the latest substitution rules from the remote URL and updates
        /// the local settings. Returns true if rules were updated.
        /// On failure, the existing local settings remain unchanged.
        /// </summary>
        public async Task<bool> UpdateRulesFromRemoteAsync()
        {
            try
            {
                var json = await _httpClient.GetStringAsync(RemoteRulesUrl);
                var remoteRules = JsonSerializer.Deserialize<List<CharacterRule>>(json);

                if (remoteRules != null && remoteRules.Count > 0 && remoteRules.Count <= MaxRuleCount)
                {
                    // Remove rules with null or empty Find strings
                    remoteRules.RemoveAll(r => r == null! || string.IsNullOrEmpty(r.Find));

                    if (remoteRules.Count > 0)
                    {
                        Rules = remoteRules;
                        Save();
                        Debug.WriteLine($"Updated rules from remote: {remoteRules.Count} rules loaded.");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to fetch remote rules: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Clamp all deserialized values to safe ranges so a crafted settings.json
        /// cannot inject out-of-range window sizes, font sizes, or null/oversized rule lists.
        /// </summary>
        private void Sanitize()
        {
            WindowWidth = Math.Clamp(WindowWidth, 600, 3840);
            WindowHeight = Math.Clamp(WindowHeight, 400, 2160);
            TextFontSize = Math.Clamp(TextFontSize, 8, 48);

            // Guard against null or oversized rules list from crafted JSON
            if (Rules == null!)
            {
                Rules = GetDefaultRules();
            }
            else if (Rules.Count > MaxRuleCount)
            {
                Rules = Rules.GetRange(0, MaxRuleCount);
            }

            // Remove rules with null or empty Find strings (invalid)
            Rules.RemoveAll(r => r == null! || string.IsNullOrEmpty(r.Find));
        }
    }
}
