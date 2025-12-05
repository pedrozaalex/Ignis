using NAudio.Wave;
using Ignis.Audio;

namespace Samples.Breakout.Services;

/// <summary>
/// Audio service for Breakout using Ignis.Audio engine.
/// Generates procedural sounds when no audio files are available.
/// </summary>
public sealed class AudioService : IDisposable
{
    private readonly AudioEngine _engine;
    private bool _initialized;
    private bool _disposed;

    public float MasterVolume
    {
        get => _engine.MasterVolume;
        set
        {
            _engine.MasterVolume = Math.Clamp(value, 0f, 1f);
            _engine.UpdateMusicVolume();
        }
    }

    public float SfxVolume
    {
        get => _engine.SfxVolume;
        set => _engine.SfxVolume = Math.Clamp(value, 0f, 1f);
    }

    public float MusicVolume
    {
        get => _engine.MusicVolume;
        set
        {
            _engine.MusicVolume = Math.Clamp(value, 0f, 1f);
            _engine.UpdateMusicVolume();
        }
    }

    // Sound effect identifiers
    public const string SfxPaddleHit = "paddle_hit";
    public const string SfxBrickBreak = "brick_break";
    public const string SfxBrickHit = "brick_hit";
    public const string SfxPowerUp = "power_up";
    public const string SfxWallBounce = "wall_bounce";
    public const string SfxLifeLost = "life_lost";
    public const string SfxLevelComplete = "level_complete";
    public const string SfxGameOver = "game_over";
    public const string SfxMenuSelect = "menu_select";
    public const string SfxBallLaunch = "ball_launch";

    public AudioService()
    {
        _engine = new AudioEngine { ContentPath = "Content/Audio" };
    }

    /// <summary>
    /// Initialize audio engine and generate procedural sounds.
    /// </summary>
    public void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
            _engine.Initialize();
            GenerateProceduralSounds();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Audio] Failed to initialize: {ex.Message}");
        }
    }

    private void GenerateProceduralSounds()
    {
        // Generate and cache procedural sounds
        RegisterProceduralSound(SfxPaddleHit, SoundGenerator.GenerateBounce(500f, 0.4f));
        RegisterProceduralSound(SfxBrickBreak, SoundGenerator.GenerateBreak(0.35f));
        RegisterProceduralSound(SfxBrickHit, SoundGenerator.GenerateBounce(600f, 0.3f));
        RegisterProceduralSound(SfxPowerUp, SoundGenerator.GeneratePowerUp(0.4f));
        RegisterProceduralSound(SfxWallBounce, SoundGenerator.GenerateBounce(300f, 0.25f));
        RegisterProceduralSound(SfxLifeLost, SoundGenerator.GenerateLifeLost(0.4f));
        RegisterProceduralSound(SfxLevelComplete, SoundGenerator.GenerateLevelComplete(0.4f));
        RegisterProceduralSound(SfxGameOver, SoundGenerator.GenerateGameOver(0.4f));
        RegisterProceduralSound(SfxMenuSelect, SoundGenerator.GenerateBlip(800f, 0.35f));
        RegisterProceduralSound(SfxBallLaunch, SoundGenerator.GenerateSweep(200f, 600f, 100f, 0.3f));
    }

    private void RegisterProceduralSound(string soundId, byte[] pcmData)
    {
        try
        {
            var format = new WaveFormat(44100, 16, 1); // 44.1kHz, 16-bit, mono
            var data = new ProceduralSoundData(pcmData, format);
            _engine.RegisterProceduralSound(soundId, data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Audio] Failed to register {soundId}: {ex.Message}");
        }
    }

    /// <summary>
    /// Play a sound effect.
    /// </summary>
    public void PlaySfx(string soundId)
    {
        if (!_initialized) return;
        _engine.PlaySound(soundId);
    }

    /// <summary>
    /// Start playing background music.
    /// </summary>
    public void PlayMusic(string musicId, bool loop = true)
    {
        // Music would need actual audio files
        // For now, this is a no-op since we don't have music files
    }

    /// <summary>
    /// Stop current music.
    /// </summary>
    public void StopMusic()
    {
        _engine.StopMusic();
    }

    /// <summary>
    /// Pause/resume music.
    /// </summary>
    public void SetMusicPaused(bool paused)
    {
        _engine.SetMusicPaused(paused);
    }

    /// <summary>
    /// Update audio engine - call once per frame.
    /// </summary>
    public void Update()
    {
        _engine.Update();
    }

    /// <summary>
    /// Update volume settings from SettingsService.
    /// </summary>
    public void UpdateFromSettings(SettingsService settings)
    {
        MasterVolume = settings.MasterVolume;
        SfxVolume = settings.SfxVolume;
        MusicVolume = settings.MusicVolume;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _engine.Dispose();
    }
}
