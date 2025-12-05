using CSCore;
using Ignis.Audio;

namespace Samples.TowerDefense.Services;

/// <summary>
/// Audio service for Tower Defense using Ignis.Audio engine.
/// Generates procedural sounds for turrets, enemies, and effects.
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
    public const string SfxBlasterFire = "blaster_fire";
    public const string SfxCannonFire = "cannon_fire";
    public const string SfxFreezerFire = "freezer_fire";
    public const string SfxExplosion = "explosion";
    public const string SfxEnemyHit = "enemy_hit";
    public const string SfxEnemyDeath = "enemy_death";
    public const string SfxEnemyReachEnd = "enemy_reach_end";
    public const string SfxWaveStart = "wave_start";
    public const string SfxWaveComplete = "wave_complete";
    public const string SfxTurretPlace = "turret_place";
    public const string SfxTurretSell = "turret_sell";
    public const string SfxMenuSelect = "menu_select";
    public const string SfxVictory = "victory";
    public const string SfxGameOver = "game_over";
    public const string SfxNotEnoughGold = "not_enough_gold";

    public AudioService()
    {
        _engine = new AudioEngine { ContentPath = "Content/Audio" };
    }

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
        // Turret firing sounds
        RegisterProceduralSound(SfxBlasterFire, SoundGenerator.GenerateSquareWave(600f, 50f, 0.25f));
        RegisterProceduralSound(SfxCannonFire, GenerateCannonSound());
        RegisterProceduralSound(SfxFreezerFire, SoundGenerator.GenerateSweep(1000f, 400f, 150f, 0.3f));

        // Impact/explosion sounds
        RegisterProceduralSound(SfxExplosion, GenerateExplosionSound());
        RegisterProceduralSound(SfxEnemyHit, SoundGenerator.GenerateSineWave(200f, 30f, 0.2f));
        RegisterProceduralSound(SfxEnemyDeath, SoundGenerator.GenerateSweep(400f, 100f, 200f, 0.35f));
        RegisterProceduralSound(SfxEnemyReachEnd, SoundGenerator.GenerateSweep(600f, 200f, 300f, 0.4f));

        // Wave sounds
        RegisterProceduralSound(SfxWaveStart, GenerateWaveStartSound());
        RegisterProceduralSound(SfxWaveComplete, GenerateWaveCompleteSound());

        // Building sounds
        RegisterProceduralSound(SfxTurretPlace, SoundGenerator.GenerateSweep(200f, 600f, 100f, 0.3f));
        RegisterProceduralSound(SfxTurretSell, SoundGenerator.GenerateSweep(600f, 200f, 100f, 0.25f));

        // UI sounds
        RegisterProceduralSound(SfxMenuSelect, SoundGenerator.GenerateBlip(800f, 0.35f));
        RegisterProceduralSound(SfxNotEnoughGold, SoundGenerator.GenerateSquareWave(150f, 200f, 0.3f));

        // Game end sounds
        RegisterProceduralSound(SfxVictory, GenerateVictorySound());
        RegisterProceduralSound(SfxGameOver, SoundGenerator.GenerateGameOver(0.4f));
    }

    private byte[] GenerateCannonSound()
    {
        // Low boom with noise tail
        var boom = SoundGenerator.GenerateSineWave(80f, 100f, 0.5f);
        var noise = SoundGenerator.GenerateNoise(150f, 0.2f);
        return MixSounds(boom, noise);
    }

    private byte[] GenerateExplosionSound()
    {
        var noise = SoundGenerator.GenerateNoise(200f, 0.4f);
        var rumble = SoundGenerator.GenerateSineWave(60f, 200f, 0.3f);
        return MixSounds(noise, rumble);
    }

    private byte[] GenerateWaveStartSound()
    {
        var samples = new List<byte>();
        samples.AddRange(SoundGenerator.GenerateSineWave(400f, 100f, 0.3f));
        samples.AddRange(SoundGenerator.GenerateSineWave(500f, 100f, 0.3f));
        samples.AddRange(SoundGenerator.GenerateSineWave(600f, 150f, 0.35f));
        return samples.ToArray();
    }

    private byte[] GenerateWaveCompleteSound()
    {
        var samples = new List<byte>();
        samples.AddRange(SoundGenerator.GenerateSineWave(523f, 100f, 0.35f)); // C5
        samples.AddRange(SoundGenerator.GenerateSineWave(659f, 100f, 0.35f)); // E5
        samples.AddRange(SoundGenerator.GenerateSineWave(784f, 200f, 0.4f));  // G5
        return samples.ToArray();
    }

    private byte[] GenerateVictorySound()
    {
        var samples = new List<byte>();
        samples.AddRange(SoundGenerator.GenerateSineWave(523f, 150f, 0.4f));  // C5
        samples.AddRange(SoundGenerator.GenerateSineWave(659f, 150f, 0.4f));  // E5
        samples.AddRange(SoundGenerator.GenerateSineWave(784f, 150f, 0.4f));  // G5
        samples.AddRange(SoundGenerator.GenerateSineWave(1047f, 400f, 0.45f)); // C6
        return samples.ToArray();
    }

    private byte[] MixSounds(byte[] a, byte[] b)
    {
        var length = Math.Max(a.Length, b.Length);
        var result = new byte[length];

        for (var i = 0; i < length; i += 2)
        {
            short sampleA = 0, sampleB = 0;

            if (i + 1 < a.Length)
                sampleA = (short)(a[i] | (a[i + 1] << 8));
            if (i + 1 < b.Length)
                sampleB = (short)(b[i] | (b[i + 1] << 8));

            var mixed = (short)Math.Clamp(sampleA + sampleB, short.MinValue, short.MaxValue);

            result[i] = (byte)(mixed & 0xFF);
            result[i + 1] = (byte)((mixed >> 8) & 0xFF);
        }

        return result;
    }

    private void RegisterProceduralSound(string soundId, byte[] pcmData)
    {
        try
        {
            var format = new WaveFormat(44100, 16, 1);
            var data = new ProceduralSoundData(pcmData, format);
            _engine.RegisterProceduralSound(soundId, data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Audio] Failed to register {soundId}: {ex.Message}");
        }
    }

    public void PlaySfx(string soundId)
    {
        if (!_initialized) return;
        _engine.PlaySound(soundId);
    }

    public void PlayMusic(string musicId, bool loop = true)
    {
        // Music would need actual audio files - no-op for now
    }

    public void StopMusic()
    {
        _engine.StopMusic();
    }

    public void SetMusicPaused(bool paused)
    {
        _engine.SetMusicPaused(paused);
    }

    public void Update()
    {
        _engine.Update();
    }

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
