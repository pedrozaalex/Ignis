namespace Ignis.Audio;

/// <summary>
/// Procedural sound generator for simple game sound effects.
/// Generates basic waveforms that can be used when no audio files are available.
/// </summary>
public static class SoundGenerator
{
    /// <summary>
    /// Generate a simple sine wave beep.
    /// </summary>
    public static byte[] GenerateSineWave(float frequency, float durationMs, float volume = 0.5f, int sampleRate = 44100)
    {
        var samples = (int)(sampleRate * durationMs / 1000);
        var data = new byte[samples * 2]; // 16-bit mono

        for (var i = 0; i < samples; i++)
        {
            var t = (float)i / sampleRate;
            var envelope = GetEnvelope(i, samples);
            var sample = (short)(MathF.Sin(2 * MathF.PI * frequency * t) * 32767 * volume * envelope);

            data[i * 2] = (byte)(sample & 0xFF);
            data[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }

        return data;
    }

    /// <summary>
    /// Generate a square wave (retro game style).
    /// </summary>
    public static byte[] GenerateSquareWave(float frequency, float durationMs, float volume = 0.3f, int sampleRate = 44100)
    {
        var samples = (int)(sampleRate * durationMs / 1000);
        var data = new byte[samples * 2];
        var period = sampleRate / frequency;

        for (var i = 0; i < samples; i++)
        {
            var envelope = GetEnvelope(i, samples);
            var value = (i % period) < (period / 2) ? 1f : -1f;
            var sample = (short)(value * 32767 * volume * envelope);

            data[i * 2] = (byte)(sample & 0xFF);
            data[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }

        return data;
    }

    /// <summary>
    /// Generate white noise (useful for explosions, static).
    /// </summary>
    public static byte[] GenerateNoise(float durationMs, float volume = 0.3f, int sampleRate = 44100)
    {
        var samples = (int)(sampleRate * durationMs / 1000);
        var data = new byte[samples * 2];
        var random = new Random();

        for (var i = 0; i < samples; i++)
        {
            var envelope = GetEnvelope(i, samples);
            var sample = (short)((random.NextDouble() * 2 - 1) * 32767 * volume * envelope);

            data[i * 2] = (byte)(sample & 0xFF);
            data[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }

        return data;
    }

    /// <summary>
    /// Generate a frequency sweep (laser/zap sound).
    /// </summary>
    public static byte[] GenerateSweep(float startFreq, float endFreq, float durationMs, float volume = 0.4f, int sampleRate = 44100)
    {
        var samples = (int)(sampleRate * durationMs / 1000);
        var data = new byte[samples * 2];

        float phase = 0;
        for (var i = 0; i < samples; i++)
        {
            var t = (float)i / samples;
            var freq = startFreq + (endFreq - startFreq) * t;
            var envelope = GetEnvelope(i, samples);

            phase += 2 * MathF.PI * freq / sampleRate;
            var sample = (short)(MathF.Sin(phase) * 32767 * volume * envelope);

            data[i * 2] = (byte)(sample & 0xFF);
            data[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }

        return data;
    }

    /// <summary>
    /// Generate a "blip" sound (menu selection).
    /// </summary>
    public static byte[] GenerateBlip(float frequency = 800f, float volume = 0.4f)
    {
        return GenerateSineWave(frequency, 50, volume);
    }

    /// <summary>
    /// Generate a "bounce" sound (ball hitting paddle).
    /// </summary>
    public static byte[] GenerateBounce(float frequency = 400f, float volume = 0.4f)
    {
        return GenerateSweep(frequency, frequency * 0.5f, 80, volume);
    }

    /// <summary>
    /// Generate a "break" sound (brick breaking).
    /// </summary>
    public static byte[] GenerateBreak(float volume = 0.3f)
    {
        var noise = GenerateNoise(100, volume * 0.5f);
        var tone = GenerateSweep(600, 200, 100, volume);
        return MixSounds(noise, tone);
    }

    /// <summary>
    /// Generate a "powerup" sound.
    /// </summary>
    public static byte[] GeneratePowerUp(float volume = 0.4f)
    {
        return GenerateSweep(400, 1200, 200, volume);
    }

    /// <summary>
    /// Generate a "life lost" sound.
    /// </summary>
    public static byte[] GenerateLifeLost(float volume = 0.4f)
    {
        return GenerateSweep(600, 100, 500, volume);
    }

    /// <summary>
    /// Generate a "game over" sound.
    /// </summary>
    public static byte[] GenerateGameOver(float volume = 0.4f)
    {
        var samples = new List<byte>();
        samples.AddRange(GenerateSineWave(400, 200, volume));
        samples.AddRange(GenerateSineWave(300, 200, volume));
        samples.AddRange(GenerateSineWave(200, 400, volume));
        return samples.ToArray();
    }

    /// <summary>
    /// Generate a "level complete" fanfare.
    /// </summary>
    public static byte[] GenerateLevelComplete(float volume = 0.4f)
    {
        var samples = new List<byte>();
        samples.AddRange(GenerateSineWave(523, 150, volume)); // C5
        samples.AddRange(GenerateSineWave(659, 150, volume)); // E5
        samples.AddRange(GenerateSineWave(784, 150, volume)); // G5
        samples.AddRange(GenerateSineWave(1047, 300, volume)); // C6
        return samples.ToArray();
    }

    private static float GetEnvelope(int sample, int totalSamples)
    {
        var t = (float)sample / totalSamples;

        // Quick attack, gradual release
        const float attackTime = 0.05f;
        const float releaseStart = 0.3f;

        if (t < attackTime)
            return t / attackTime;
        if (t > releaseStart)
            return 1f - (t - releaseStart) / (1f - releaseStart);
        return 1f;
    }

    private static byte[] MixSounds(byte[] a, byte[] b)
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
}
