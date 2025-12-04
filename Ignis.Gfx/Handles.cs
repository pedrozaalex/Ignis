using System;

namespace Ignis.Gfx;

/// <summary>Handle to an uploaded mesh resource on the GPU.</summary>
public readonly struct MeshHandle : IEquatable<MeshHandle>
{
    public readonly int Id;
    public static MeshHandle Invalid => new(0);
    public bool IsValid => Id != 0;
    
    public MeshHandle(int id) => Id = id;
    
    public bool Equals(MeshHandle other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is MeshHandle other && Equals(other);
    public override int GetHashCode() => Id;
    public static bool operator ==(MeshHandle left, MeshHandle right) => left.Equals(right);
    public static bool operator !=(MeshHandle left, MeshHandle right) => !left.Equals(right);
}

/// <summary>Handle to an uploaded texture resource on the GPU.</summary>
public readonly struct TextureHandle : IEquatable<TextureHandle>
{
    public readonly int Id;
    public static TextureHandle Invalid => new(0);
    public bool IsValid => Id != 0;
    
    public TextureHandle(int id) => Id = id;
    
    public bool Equals(TextureHandle other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is TextureHandle other && Equals(other);
    public override int GetHashCode() => Id;
    public static bool operator ==(TextureHandle left, TextureHandle right) => left.Equals(right);
    public static bool operator !=(TextureHandle left, TextureHandle right) => !left.Equals(right);
}

/// <summary>Handle to a compiled shader/pipeline on the GPU.</summary>
public readonly struct ShaderHandle : IEquatable<ShaderHandle>
{
    public readonly int Id;
    public static ShaderHandle Invalid => new(0);
    public bool IsValid => Id != 0;
    
    public ShaderHandle(int id) => Id = id;
    
    public bool Equals(ShaderHandle other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is ShaderHandle other && Equals(other);
    public override int GetHashCode() => Id;
    public static bool operator ==(ShaderHandle left, ShaderHandle right) => left.Equals(right);
    public static bool operator !=(ShaderHandle left, ShaderHandle right) => !left.Equals(right);
}

/// <summary>Handle to a loaded font resource (atlas + metrics).</summary>
public readonly struct FontHandle : IEquatable<FontHandle>
{
    public readonly int Id;
    public static FontHandle Invalid => new(0);
    public bool IsValid => Id != 0;
    
    public FontHandle(int id) => Id = id;
    
    public bool Equals(FontHandle other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is FontHandle other && Equals(other);
    public override int GetHashCode() => Id;
    public static bool operator ==(FontHandle left, FontHandle right) => left.Equals(right);
    public static bool operator !=(FontHandle left, FontHandle right) => !left.Equals(right);
}

/// <summary>Handle to an off-screen render target. Id 0 represents the screen backbuffer.</summary>
public readonly struct RenderTargetHandle : IEquatable<RenderTargetHandle>
{
    public readonly int Id;
    
    /// <summary>The default backbuffer (screen).</summary>
    public static RenderTargetHandle Screen => new(0);
    public static RenderTargetHandle Invalid => new(-1);
    
    public bool IsScreen => Id == 0;
    public bool IsValid => Id >= 0;
    
    public RenderTargetHandle(int id) => Id = id;
    
    public bool Equals(RenderTargetHandle other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is RenderTargetHandle other && Equals(other);
    public override int GetHashCode() => Id;
    public static bool operator ==(RenderTargetHandle left, RenderTargetHandle right) => left.Equals(right);
    public static bool operator !=(RenderTargetHandle left, RenderTargetHandle right) => !left.Equals(right);
}

