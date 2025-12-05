namespace Ignis.Audio;

/// <summary>
/// High-level audio service for games.
/// Wraps AudioEngine with convenient methods for SFX and music.
/// </summary>
public sealed class GameAudioService : IDisposable
{
    private readonly AudioEngine _engine;
    private readonly Dictionary<string, string> _sfxMappings = new();
    private readonly Dictionary<string, string> _musicMappings = new();
    private bool _disposed;

    /// <summary>Master volume (0.0 to 1.0).</summary>
    public float MasterVolume
    {
        get => _engine.MasterVolume;
        set
        {
            _engine.MasterVolume = Math.Clamp(value, 0f, 1f);
            _engine.UpdateMusicVolume();
        }
    }

    /// <summary>Sound effects volume (0.0 to 1.0).</summary>
    public float SfxVolume
    {
        get => _engine.SfxVolume;
        set => _engine.SfxVolume = Math.Clamp(value, 0f, 1f);
    }

    /// <summary>Music volume (0.0 to 1.0).</summary>
    public float MusicVolume
    {
        get => _engine.MusicVolume;
        set
        {
            _engine.MusicVolume = Math.Clamp(value, 0f, 1f);
            _engine.UpdateMusicVolume();
        }
    }

    /// <summary>
    /// Creates a new game audio service.
    /// </summary>
    /// <param name="contentPath">Path to audio content folder relative to executable.</param>
    public GameAudioService(string contentPath = "Content/Audio")
    {
        _engine = new AudioEngine { ContentPath = contentPath };
        _engine.Initialize();
    }

    /// <summary>
    /// Register a sound effect with an ID and filename.
    /// </summary>
    public void RegisterSfx(string soundId, string filename)
    {
        _sfxMappings[soundId] = filename;
        _engine.PreloadSound(soundId, filename);
    }

    /// <summary>
    /// Register a music track with an ID and filename.
    /// </summary>
    public void RegisterMusic(string musicId, string filename)
    {
        _musicMappings[musicId] = filename;
    }

    /// <summary>
    /// Play a registered sound effect.
    /// </summary>
    public void PlaySfx(string soundId, float volumeScale = 1.0f)
    {
        _engine.PlaySound(soundId, volumeScale);
    }

    /// <summary>
    /// Play a registered music track.
    /// </summary>
    public void PlayMusic(string musicId, bool loop = true)
    {
        if (_musicMappings.TryGetValue(musicId, out var filename))
        {
            _engine.PlayMusic(musicId, filename, loop);
        }
    }

    /// <summary>
    /// Stop the current music.
    /// </summary>
    public void StopMusic()
    {
        _engine.StopMusic();
    }

    /// <summary>
    /// Pause or resume music.
    /// </summary>
    public void SetMusicPaused(bool paused)
    {
        _engine.SetMusicPaused(paused);
    }

    /// <summary>
    /// Update the audio system. Call once per frame.
    /// </summary>
    public void Update()
    {
        _engine.Update();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _engine.Dispose();
    }
}
