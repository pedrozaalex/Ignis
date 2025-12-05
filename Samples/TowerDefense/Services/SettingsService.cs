namespace Samples.TowerDefense.Services;

/// <summary>
/// Persists and loads game settings.
/// </summary>
public sealed class SettingsService
{
    private const string SettingsFile = "towerdefense_settings.txt";

    public float MasterVolume { get; set; } = 1.0f;
    public float SfxVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 0.7f;
    public int HighestLevelUnlocked { get; set; } = 1;

    public void Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, SettingsFile);
        if (!File.Exists(path)) return;

        try
        {
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                switch (key)
                {
                    case "MasterVolume":
                        MasterVolume = float.TryParse(value, out var mv) ? mv : 1.0f;
                        break;
                    case "SfxVolume":
                        SfxVolume = float.TryParse(value, out var sv) ? sv : 1.0f;
                        break;
                    case "MusicVolume":
                        MusicVolume = float.TryParse(value, out var muv) ? muv : 0.7f;
                        break;
                    case "HighestLevel":
                        HighestLevelUnlocked = int.TryParse(value, out var hl) ? hl : 1;
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Settings] Failed to load: {ex.Message}");
        }
    }

    public void Save()
    {
        var path = Path.Combine(AppContext.BaseDirectory, SettingsFile);
        try
        {
            var lines = new[]
            {
                $"MasterVolume={MasterVolume:F2}",
                $"SfxVolume={SfxVolume:F2}",
                $"MusicVolume={MusicVolume:F2}",
                $"HighestLevel={HighestLevelUnlocked}"
            };
            File.WriteAllLines(path, lines);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Settings] Failed to save: {ex.Message}");
        }
    }

    public void UnlockLevel(int level)
    {
        if (level > HighestLevelUnlocked)
        {
            HighestLevelUnlocked = level;
            Save();
        }
    }
}
