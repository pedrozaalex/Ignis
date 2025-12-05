using System.Text.Json;

namespace Samples.Breakout.Services;

/// <summary>
/// User settings with persistence.
/// </summary>
public sealed class SettingsService
{
    private const string FileName = "settings.json";
    
    public float MasterVolume { get; set; } = 1.0f;
    public float SfxVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 0.7f;
    public bool FullScreen { get; set; }
    public int SelectedLevel { get; set; } = 1;
    public bool DebugMode { get; set; }
    
    public void Load()
    {
        try
        {
            var path = GetSavePath();
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var settings = JsonSerializer.Deserialize<SettingsData>(json);
                if (settings != null)
                {
                    MasterVolume = settings.MasterVolume;
                    SfxVolume = settings.SfxVolume;
                    MusicVolume = settings.MusicVolume;
                    FullScreen = settings.FullScreen;
                    SelectedLevel = settings.SelectedLevel;
                    DebugMode = settings.DebugMode;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load settings: {ex.Message}");
        }
    }
    
    public void Save()
    {
        try
        {
            var path = GetSavePath();
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            
            var data = new SettingsData
            {
                MasterVolume = MasterVolume,
                SfxVolume = SfxVolume,
                MusicVolume = MusicVolume,
                FullScreen = FullScreen,
                SelectedLevel = SelectedLevel,
                DebugMode = DebugMode
            };
            
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }
    
    private static string GetSavePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "Ignis", "Breakout", FileName);
    }
    
    private class SettingsData
    {
        public float MasterVolume { get; set; }
        public float SfxVolume { get; set; }
        public float MusicVolume { get; set; }
        public bool FullScreen { get; set; }
        public int SelectedLevel { get; set; }
        public bool DebugMode { get; set; }
    }
}
