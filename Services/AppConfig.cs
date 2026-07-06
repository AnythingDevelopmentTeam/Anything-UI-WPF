using System.IO;

namespace Anything_UI_WPF.Services;

public static class AppConfig
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".config", "anything");

    private static readonly string IndexPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".config", "anything-index.anythingindex");

    private static readonly string SkipDirsPath = Path.Combine(ConfigDir, "custom_skip_dirs.txt");
    private static readonly string SettingsPath = Path.Combine(ConfigDir, "settings.json");

    public static string GetIndexPath() => IndexPath;
    public static string GetConfigDir() => ConfigDir;

    public static List<string> LoadCustomSkipDirs()
    {
        try
        {
            if (!File.Exists(SkipDirsPath)) return new();
            return File.ReadAllLines(SkipDirsPath)
                .Select(l => l.Trim())
                .Where(l => l.Length > 0 && !l.StartsWith('#'))
                .ToList();
        }
        catch
        {
            return new();
        }
    }

    public static void SaveCustomSkipDirs(List<string> dirs)
    {
        try
        {
            Directory.CreateDirectory(ConfigDir);
            File.WriteAllLines(SkipDirsPath, dirs);
        }
        catch { }
    }

    public static string? LoadLanguage()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return null;
            var json = File.ReadAllText(SettingsPath);
            var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return settings?.GetValueOrDefault("language");
        }
        catch { return null; }
    }

    public static void SaveLanguage(string code)
    {
        SaveSetting("language", code);
    }

    public static bool? LoadTheme()
    {
        var val = LoadSetting("theme");
        if (val == null) return null;
        return bool.TryParse(val, out var result) ? result : null;
    }

    public static void SaveTheme(bool isDark)
    {
        SaveSetting("theme", isDark.ToString().ToLower());
    }

    private static string? LoadSetting(string key)
    {
        try
        {
            if (!File.Exists(SettingsPath)) return null;
            var json = File.ReadAllText(SettingsPath);
            var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return settings?.GetValueOrDefault(key);
        }
        catch { return null; }
    }

    private static void SaveSetting(string key, string value)
    {
        try
        {
            Directory.CreateDirectory(ConfigDir);
            Dictionary<string, string> settings;
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            }
            else
            {
                settings = new();
            }
            settings[key] = value;
            File.WriteAllText(SettingsPath, System.Text.Json.JsonSerializer.Serialize(settings));
        }
        catch { }
    }
}
