namespace Samples.Breakout.Services;

/// <summary>
/// Audio service stub - actual implementation requires audio backend.
/// Provides interface for SFX and music with volume controls.
/// </summary>
public sealed class AudioService
{
    public float MasterVolume { get; set; } = 1.0f;
    public float SfxVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 0.7f;
    
    // Sound effect identifiers
    public const string SfxPaddleHit = "paddle_hit";
    public const string SfxBrickBreak = "brick_break";
    public const string SfxPowerUp = "power_up";
    public const string SfxWallBounce = "wall_bounce";
    public const string SfxLoseLife = "lose_life";
    public const string SfxLevelComplete = "level_complete";
    public const string SfxGameOver = "game_over";
    public const string SfxMenuSelect = "menu_select";
    
    /// <summary>
    /// Play a sound effect.
    /// </summary>
    public void PlaySfx(string soundId)
    {
        // Stub - would hook into actual audio backend (OpenAL, etc.)
        // For now, just log in debug mode
        #if DEBUG
        // Console.WriteLine($"[Audio] SFX: {soundId} (vol: {SfxVolume * MasterVolume:F2})");
        #endif
    }
    
    /// <summary>
    /// Start playing background music.
    /// </summary>
    public void PlayMusic(string musicId, bool loop = true)
    {
        // Stub - would start music playback
        #if DEBUG
        // Console.WriteLine($"[Audio] Music: {musicId} (loop: {loop}, vol: {MusicVolume * MasterVolume:F2})");
        #endif
    }
    
    /// <summary>
    /// Stop current music.
    /// </summary>
    public void StopMusic()
    {
        // Stub
    }
    
    /// <summary>
    /// Pause/resume music.
    /// </summary>
    public void SetMusicPaused(bool paused)
    {
        // Stub
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
}
