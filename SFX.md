This guide provides a comprehensive architectural breakdown for implementing a game audio engine using **CSCore**.

Based on the samples provided (specifically `XAudio2Playback`, `X3DAudioSample`, and `NVorbisIntegration`), the most robust approach for a game engine is to use **XAudio2** rather than `WasapiOut`. XAudio2 is designed specifically for games: it handles mixing, polyphony (playing many sounds at once), and 3D spatial calculations natively.

### Prerequisites
1.  **CSCore** (Main library)
2.  **CSCore.Ffmpeg** (Optional, for broad format support)
3.  **NVorbis** (Recommended for OGG support, standard in games)

---

### Phase 1: The Core Audio Engine
You need a central manager to initialize the XAudio2 device and the Mastering Voice (the final output mix).

```csharp
using CSCore.XAudio2;
using CSCore.XAudio2.X3DAudio;
using CSCore;

public class AudioEngine : IDisposable
{
    private XAudio2 _xaudio;
    private XAudio2MasteringVoice _masteringVoice;
    private X3DAudioCore _x3dAudio;
    
    // 3D Calculation Helpers
    public int OutputChannelCount { get; private set; }
    public ChannelMask OutputChannelMask { get; private set; }

    public void Initialize()
    {
        // 1. Initialize the XAudio2 Interface
        _xaudio = XAudio2.CreateXAudio2();

        // 2. Create the Mastering Voice (The speakers)
        _masteringVoice = _xaudio.CreateMasteringVoice();

        // 3. Get Device Details for 3D Math
        // We need to know how many speakers the user has (Stereo, 5.1, etc.)
        // Note: Implementation varies slightly by XAudio2 version, this is for 2.7 (Windows 7/8/10 legacy)
        // For XAudio 2.8/2.9 (Win 10+), channel details are grabbed differently, 
        // but often creating the mastering voice defaults correctly.
        
        // This is a safe fallback for getting output attributes
        var deviceDetails = ((XAudio2_7)_xaudio).GetDeviceDetails(0);
        OutputChannelCount = deviceDetails.OutputFormat.Channels;
        OutputChannelMask = deviceDetails.OutputFormat.ChannelMask;

        // 4. Initialize X3DAudio for 3D calculations
        _x3dAudio = new X3DAudioCore(OutputChannelMask);
    }

    public XAudio2 Device => _xaudio;
    public X3DAudioCore X3D => _x3dAudio;
    public XAudio2MasteringVoice MasterVoice => _masteringVoice;

    public void Dispose()
    {
        _masteringVoice?.Dispose();
        _xaudio?.Dispose();
    }
}
```

---

### Phase 2: Asset Management
In games, you typically have two types of audio:
1.  **Sound Effects (SFX):** Short, played frequently. Load these fully into RAM.
2.  **Music/Ambience:** Long, large files. Stream these from disk.

#### 2.1 Integrating OGG Vorbis (Recommended)
Based on `Samples/NVorbisIntegration`, register the codec once at startup:
```csharp
using CSCore.Codecs;
// ... inside your Game Initialization
CodecFactory.Instance.Register("ogg-vorbis", new CodecFactoryEntry(s => new NVorbisSource(s).ToWaveSource(), ".ogg"));
```

#### 2.2 Cached Sound (SFX)
This class loads audio data into a byte array for instant, repeated playback.

```csharp
public class SoundEffectData : IDisposable
{
    public byte[] Data { get; private set; }
    public WaveFormat Format { get; private set; }

    public SoundEffectData(string filepath)
    {
        // Use CodecFactory to handle Wav, Mp3, or Ogg automatically
        using (var source = CodecFactory.Instance.GetCodec(filepath))
        {
            Format = source.WaveFormat;
            
            // Read the whole file into memory
            using (var stream = new MemoryStream())
            {
                source.WriteTo(stream);
                Data = stream.ToArray();
            }
        }
    }

    public void Dispose() { /* Clear data if needed */ }
}
```

---

### Phase 3: The 2D Audio Source (UI, HUD, Music)
This handles playing sounds that do not have a position in the world.

```csharp
public class AudioSource : IDisposable
{
    protected XAudio2SourceVoice _sourceVoice;
    protected AudioEngine _engine;
    protected bool _isLooping;

    public AudioSource(AudioEngine engine, SoundEffectData data)
    {
        _engine = engine;
        
        // Create a SourceVoice for this specific sound data
        _sourceVoice = engine.Device.CreateSourceVoice(data.Format);

        // Submit the data to the voice
        var buffer = new XAudio2Buffer(data.Data.Length);
        
        // Copy data to buffer (simplified for brevity, in production use Stream/Unsafe copy)
        using(var stream = buffer.GetStream())
        {
            stream.Write(data.Data, 0, data.Data.Length);
        }
        
        buffer.Flags = XAudio2BufferFlags.EndOfStream;
        
        _sourceVoice.SubmitSourceBuffer(buffer);
    }

    public void Play()
    {
        _sourceVoice.Start();
    }

    public void Stop()
    {
        _sourceVoice.Stop();
        _sourceVoice.FlushSourceBuffers(); // Reset position
    }

    public void SetVolume(float volume) // 0.0 to 1.0
    {
        _sourceVoice.SetVolume(volume);
    }

    public void SetLooping(bool loop)
    {
        // Note: To update looping on an active buffer, you often modify the XAudio2Buffer 
        // before submission. For dynamic looping, you might need to re-submit.
        // Simple way: Set LoopCount on the buffer during initialization.
    }

    public virtual void Dispose()
    {
        _sourceVoice?.Dispose();
    }
}
```

---

### Phase 4: The 3D Audio Source (Spatial)
This is the most complex part, involving `X3DAudio`. It utilizes the `Listener` and `Emitter` concepts found in `Samples/X3DAudioSample`.

```csharp
using CSCore.Utils; // For Vector3

public class AudioSource3D : AudioSource
{
    private Emitter _emitter;
    private Listener _listener; // Usually passed in from the Camera/Player
    private DspSettings _dspSettings;
    private int _inputChannels;

    public Vector3 Position 
    {
        get => _emitter.Position;
        set => _emitter.Position = value; 
    }

    public Vector3 Velocity
    {
        get => _emitter.Velocity;
        set => _emitter.Velocity = value;
    }

    public AudioSource3D(AudioEngine engine, SoundEffectData data) : base(engine, data)
    {
        _inputChannels = data.Format.Channels;

        // Initialize Emitter settings
        _emitter = new Emitter
        {
            ChannelCount = _inputChannels,
            CurveDistanceScaler = 1.0f,
            Position = new Vector3(0, 0, 0),
            Velocity = new Vector3(0, 0, 0),
            // Default orientation (needed for calculation)
            OrientFront = new Vector3(0, 0, 1),
            OrientTop = new Vector3(0, 1, 0)
        };

        // Prepare DSP Settings (stores the calculated matrix results)
        _dspSettings = new DspSettings(_inputChannels, engine.OutputChannelCount);
    }

    public void Update3D(Listener activeListener)
    {
        // 1. Calculate the 3D physics (Volume matrix, Doppler, etc.)
        // CalculateFlags.Matrix calculates volume per speaker based on position
        // CalculateFlags.Doppler calculates pitch shift based on velocity
        _engine.X3D.X3DAudioCalculate(activeListener, _emitter, 
            CalculateFlags.Matrix | CalculateFlags.Doppler, 
            _dspSettings);

        // 2. Apply Volume/Pan (Matrix)
        _sourceVoice.SetOutputMatrix(
            _engine.MasterVoice, 
            _inputChannels, 
            _engine.OutputChannelCount, 
            _dspSettings.MatrixCoefficients
        );

        // 3. Apply Doppler (Pitch)
        _sourceVoice.SetFrequencyRatio(_dspSettings.DopplerFactor);
    }
}
```

---

### Phase 5: The Game Loop Integration
You need to update the `Listener` every frame based on your camera position.

```csharp
public class GameAudioSystem
{
    private AudioEngine _audioEngine;
    private Listener _activeListener;
    private List<AudioSource3D> _activeSounds = new List<AudioSource3D>();

    public GameAudioSystem()
    {
        _audioEngine = new AudioEngine();
        _audioEngine.Initialize();

        // Initialize Listener (usually represents the Camera or Player Head)
        _activeListener = new Listener
        {
            Position = new Vector3(0, 0, 0),
            OrientFront = new Vector3(0, 0, 1),
            OrientTop = new Vector3(0, 1, 0),
            Velocity = new Vector3(0, 0, 0)
        };
    }

    public void Update(float deltaTime, Vector3 cameraPos, Vector3 cameraFwd, Vector3 cameraUp)
    {
        // Update Listener Position
        _activeListener.Position = cameraPos;
        _activeListener.OrientFront = cameraFwd;
        _activeListener.OrientTop = cameraUp;
        // _activeListener.Velocity = ... (calculate based on pos - prevPos)

        // Update all 3D sounds
        foreach (var sound in _activeSounds)
        {
            sound.Update3D(_activeListener);
        }
    }
}
```

---

### Phase 6: Advanced Features (Effects)

#### 6.1 Pitch Shifting & Time Stretching
CSCore includes a port of the SoundTouch library (`Samples/SoundTouchPitchAndTempo`). To use this in a game engine (e.g., for slow-motion effects), you cannot use the raw `XAudio2Buffer`. You must use a `StreamingSourceVoice` that pulls data through the `SoundTouchSource`.

1.  Wrap your `IWaveSource` in a `SoundTouchSource`.
2.  Use `SoundTouchSource.SetPitch(float)` or `SetTempo(float)`.
3.  Feed this source into a `StreamingSourceVoice`.

#### 6.2 Filters (Muffling/Underwater Effect)
XAudio2 supports native effects (Reverb, EQ), but CSCore also allows DSP effects on the source data.
Using `BiQuadFilterSample` as a reference:

```csharp
// To apply a LowPass filter (underwater effect)
public void ApplyUnderwaterEffect(IWaveSource source)
{
    // Wrap source with BiQuad filter
    var filterSource = new BiQuadFilterSource(source.ToSampleSource());
    filterSource.Filter = new LowpassFilter(source.WaveFormat.SampleRate, 800); // 800Hz cutoff
    
    // Play filterSource...
}
```
*Note: Doing DSP in C# (Managed code) is slower than XAudio2's native XAPO effects (Native C++), but easier to implement.*

---

### Summary of Workflow
1.  **Initialize** `XAudio2` and `X3DAudioCore`.
2.  **Load** audio files using `CodecFactory` into byte arrays (`SoundEffectData`).
3.  **Spawn** `SourceVoice` objects for playback.
4.  **Loop**:
    *   Update `Listener` (Camera) position.
    *   Update `Emitter` (Source) position.
    *   Call `X3DAudioCalculate`.
    *   Apply `dspSettings.MatrixCoefficients` to the `SourceVoice`.
5.  **Dispose** resources when the level unloads.