using System.Text.Json;

namespace Ignis.Core.IO;

/// <summary>
/// Base class for JSON-based persistence.
/// Subclass and define your data properties.
/// </summary>
/// <typeparam name="T">The concrete settings type.</typeparam>
public abstract class JsonSaveService<T> where T : JsonSaveService<T>, new()
{
    /// <summary>The filename for this save data (e.g., "settings.json").</summary>
    protected abstract string FileName { get; }

    /// <summary>The application/game name for organizing save folders.</summary>
    protected abstract string AppName { get; }

    /// <summary>JSON serializer options for save/load.</summary>
    protected virtual JsonSerializerOptions SerializerOptions => new() { WriteIndented = true };

    /// <summary>
    /// Loads data from disk into this instance.
    /// </summary>
    public virtual void Load()
    {
        try
        {
            var path = GetSavePath();
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var loaded = JsonSerializer.Deserialize<T>(json, SerializerOptions);
                if (loaded != null)
                    CopyFrom(loaded);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{GetType().Name}] Failed to load: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves this instance to disk.
    /// </summary>
    public virtual void Save()
    {
        try
        {
            var path = GetSavePath();
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize((T)this, SerializerOptions);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{GetType().Name}] Failed to save: {ex.Message}");
        }
    }

    /// <summary>
    /// Copy values from another instance into this one.
    /// Override to implement property copying.
    /// </summary>
    protected abstract void CopyFrom(T other);

    /// <summary>
    /// Gets the full path to the save file.
    /// </summary>
    protected virtual string GetSavePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "Ignis", AppName, FileName);
    }
}

/// <summary>
/// Common game settings with volume controls.
/// Extend this with game-specific settings.
/// </summary>
public class GameSettings : JsonSaveService<GameSettings>
{
    protected override string FileName => "settings.json";
    protected override string AppName => "Game";

    public float MasterVolume { get; set; } = 1.0f;
    public float SfxVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 0.7f;
    public bool Fullscreen { get; set; }

    protected override void CopyFrom(GameSettings other)
    {
        MasterVolume = other.MasterVolume;
        SfxVolume = other.SfxVolume;
        MusicVolume = other.MusicVolume;
        Fullscreen = other.Fullscreen;
    }
}
