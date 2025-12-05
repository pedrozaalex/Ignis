using NAudio.Wave;

namespace Ignis.Audio;

/// <summary>
/// Interface for cached sound data that can be played.
/// </summary>
public interface ISoundData : IDisposable
{
    byte[] Data { get; }
    WaveFormat Format { get; }
}

/// <summary>
/// Core audio engine managing device initialization and audio playback.
/// Uses NAudio with WasapiOut for broad compatibility.
/// </summary>
public sealed class AudioEngine : IDisposable
{
    private readonly Dictionary<string, ISoundData> _soundCache = new();
    private readonly List<SoundInstance> _activeInstances = [];
    private readonly object _lock = new();

    private IWavePlayer? _musicOut;
    private WaveStream? _musicSource;
    private LoopStream? _musicLoop;
    private string? _currentMusicId;

    private bool _initialized;
    private bool _disposed;

    /// <summary>Master volume multiplier (0.0 to 1.0).</summary>
    public float MasterVolume { get; set; } = 1.0f;

    /// <summary>Sound effects volume multiplier (0.0 to 1.0).</summary>
    public float SfxVolume { get; set; } = 1.0f;

    /// <summary>Music volume multiplier (0.0 to 1.0).</summary>
    public float MusicVolume { get; set; } = 0.7f;

    /// <summary>Base path for audio assets.</summary>
    public string ContentPath { get; set; } = "Content/Audio";

    /// <summary>
    /// Initialize the audio engine.
    /// </summary>
    public void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
    }

    /// <summary>
    /// Preload a sound effect from a file into memory for fast playback.
    /// </summary>
    public void PreloadSound(string soundId, string filename)
    {
        if (_soundCache.ContainsKey(soundId)) return;

        var path = Path.Combine(AppContext.BaseDirectory, ContentPath, filename);
        if (!File.Exists(path))
        {
            Console.WriteLine($"[Audio] Sound file not found: {path}");
            return;
        }

        try
        {
            var data = new SoundEffectData(path);
            _soundCache[soundId] = data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Audio] Failed to load {filename}: {ex.Message}");
        }
    }

    /// <summary>
    /// Register a procedurally generated sound effect.
    /// </summary>
    public void RegisterProceduralSound(string soundId, ProceduralSoundData data)
    {
        if (_soundCache.ContainsKey(soundId)) return;
        _soundCache[soundId] = data;
    }

    /// <summary>
    /// Play a preloaded sound effect.
    /// </summary>
    public void PlaySound(string soundId, float volumeScale = 1.0f)
    {
        if (!_soundCache.TryGetValue(soundId, out var data)) return;

        var volume = MasterVolume * SfxVolume * volumeScale;
        if (volume <= 0) return;

        try
        {
            var instance = new SoundInstance(data, volume);
            instance.Play();

            lock (_lock)
            {
                _activeInstances.Add(instance);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Audio] Failed to play {soundId}: {ex.Message}");
        }
    }

    /// <summary>
    /// Start playing background music.
    /// </summary>
    public void PlayMusic(string musicId, string filename, bool loop = true)
    {
        if (_currentMusicId == musicId && _musicOut?.PlaybackState == PlaybackState.Playing)
            return;

        StopMusic();

        var path = Path.Combine(AppContext.BaseDirectory, ContentPath, filename);
        if (!File.Exists(path))
        {
            Console.WriteLine($"[Audio] Music file not found: {path}");
            return;
        }

        try
        {
            _musicSource = new AudioFileReader(path);

            if (loop)
            {
                _musicLoop = new LoopStream(_musicSource);
                _musicOut = new WasapiOut();
                _musicOut.Init(_musicLoop);
            }
            else
            {
                _musicOut = new WasapiOut();
                _musicOut.Init(_musicSource);
            }

            UpdateMusicVolume();
            _musicOut.Play();
            _currentMusicId = musicId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Audio] Failed to play music {filename}: {ex.Message}");
            _musicSource?.Dispose();
            _musicSource = null;
        }
    }

    /// <summary>
    /// Stop the current music.
    /// </summary>
    public void StopMusic()
    {
        _musicOut?.Stop();
        _musicOut?.Dispose();
        _musicLoop?.Dispose();
        _musicSource?.Dispose();

        _musicOut = null;
        _musicLoop = null;
        _musicSource = null;
        _currentMusicId = null;
    }

    /// <summary>
    /// Pause or resume the current music.
    /// </summary>
    public void SetMusicPaused(bool paused)
    {
        if (_musicOut == null) return;

        if (paused && _musicOut.PlaybackState == PlaybackState.Playing)
            _musicOut.Pause();
        else if (!paused && _musicOut.PlaybackState == PlaybackState.Paused)
            _musicOut.Play();
    }

    /// <summary>
    /// Update music volume (call after changing MasterVolume or MusicVolume).
    /// </summary>
    public void UpdateMusicVolume()
    {
        if (_musicSource is AudioFileReader reader)
        {
            reader.Volume = Math.Clamp(MasterVolume * MusicVolume, 0f, 1f);
        }
    }

    /// <summary>
    /// Update the engine - cleans up finished sound instances.
    /// Call this periodically (e.g., once per frame).
    /// </summary>
    public void Update()
    {
        lock (_lock)
        {
            for (var i = _activeInstances.Count - 1; i >= 0; i--)
            {
                if (_activeInstances[i].IsFinished)
                {
                    _activeInstances[i].Dispose();
                    _activeInstances.RemoveAt(i);
                }
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        StopMusic();

        lock (_lock)
        {
            foreach (var instance in _activeInstances)
                instance.Dispose();
            _activeInstances.Clear();
        }

        foreach (var data in _soundCache.Values)
            data.Dispose();
        _soundCache.Clear();
    }
}

/// <summary>
/// Cached sound effect data loaded from a file.
/// </summary>
public sealed class SoundEffectData : ISoundData
{
    public byte[] Data { get; }
    public WaveFormat Format { get; }

    public SoundEffectData(string filepath)
    {
        using var reader = new AudioFileReader(filepath);
        Format = reader.WaveFormat;

        using var stream = new MemoryStream();
        var buffer = new byte[reader.WaveFormat.AverageBytesPerSecond];
        int read;
        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            stream.Write(buffer, 0, read);
        }
        Data = stream.ToArray();
    }

    public void Dispose()
    {
        // Data is managed, no explicit cleanup needed
    }
}

/// <summary>
/// Procedurally generated sound effect data.
/// </summary>
public sealed class ProceduralSoundData : ISoundData
{
    public byte[] Data { get; }
    public WaveFormat Format { get; }

    public ProceduralSoundData(byte[] pcmData, WaveFormat format)
    {
        Data = pcmData;
        Format = format;
    }

    public void Dispose()
    {
        // Data is managed, no explicit cleanup needed
    }
}

/// <summary>
/// A single playing instance of a sound effect.
/// </summary>
internal sealed class SoundInstance : IDisposable
{
    private readonly WasapiOut _soundOut;
    private readonly RawSourceWaveStream _source;
    private bool _disposed;

    public bool IsFinished => _disposed || _soundOut.PlaybackState == PlaybackState.Stopped;

    public SoundInstance(ISoundData data, float volume)
    {
        var memStream = new MemoryStream(data.Data);
        _source = new RawSourceWaveStream(memStream, data.Format);
        var volumeProvider = new VolumeWaveProvider16(_source) { Volume = Math.Clamp(volume, 0f, 1f) };

        _soundOut = new WasapiOut();
        _soundOut.Init(volumeProvider);
    }

    public void Play()
    {
        _soundOut.Play();
    }

    public void Stop()
    {
        _soundOut.Stop();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _soundOut.Stop();
        _soundOut.Dispose();
        _source.Dispose();
    }
}

/// <summary>
/// Wraps a wave stream to enable looping.
/// </summary>
internal sealed class LoopStream : WaveStream
{
    private readonly WaveStream _sourceStream;
    public bool EnableLoop { get; set; } = true;

    public LoopStream(WaveStream sourceStream)
    {
        _sourceStream = sourceStream;
    }

    public override WaveFormat WaveFormat => _sourceStream.WaveFormat;
    public override long Length => _sourceStream.Length;
    public override long Position
    {
        get => _sourceStream.Position;
        set => _sourceStream.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var totalRead = 0;

        while (totalRead < count)
        {
            var read = _sourceStream.Read(buffer, offset + totalRead, count - totalRead);
            if (read == 0)
            {
                if (!EnableLoop) break;
                _sourceStream.Position = 0;
            }
            totalRead += read;
        }

        return totalRead;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sourceStream.Dispose();
        }
        base.Dispose(disposing);
    }
}
