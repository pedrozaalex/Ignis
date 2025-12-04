namespace Ignis.Graphics;

/// <summary>Blend mode for draw operations.</summary>
public enum BlendMode
{
    /// <summary>No blending, fully opaque. Standard for 3D objects.</summary>
    Opaque,
    
    /// <summary>Standard alpha blending for transparent UI/sprites.</summary>
    AlphaBlend,
    
    /// <summary>Additive blending for fire, magic, glow effects.</summary>
    Additive,
    
    /// <summary>Pre-multiplied alpha, often used for text rendering.</summary>
    Premultiplied
}

/// <summary>Pixel format for textures.</summary>
public enum TextureFormat
{
    /// <summary>8-bit per channel RGBA.</summary>
    RGBA8,
    
    /// <summary>8-bit per channel RGB (no alpha).</summary>
    RGB8,
    
    /// <summary>Single 8-bit channel (grayscale/alpha).</summary>
    R8,
    
    /// <summary>BC1/DXT1 compressed format.</summary>
    BC1,
    
    /// <summary>BC3/DXT5 compressed format with alpha.</summary>
    BC3,
    
    /// <summary>16-bit float per channel RGBA (HDR).</summary>
    RGBA16F,
    
    /// <summary>32-bit float per channel RGBA (HDR).</summary>
    RGBA32F,
    
    /// <summary>24-bit depth + 8-bit stencil.</summary>
    Depth24Stencil8
}

/// <summary>Filter mode for texture sampling.</summary>
public enum TextureFilter
{
    /// <summary>Nearest-neighbor (pixelated).</summary>
    Point,
    
    /// <summary>Bilinear filtering.</summary>
    Linear,
    
    /// <summary>Trilinear with mipmaps.</summary>
    Trilinear,
    
    /// <summary>Anisotropic filtering.</summary>
    Anisotropic
}

/// <summary>Wrap mode for texture coordinates.</summary>
public enum TextureWrap
{
    /// <summary>Repeat the texture.</summary>
    Repeat,
    
    /// <summary>Clamp to edge pixels.</summary>
    Clamp,
    
    /// <summary>Mirror the texture on repeat.</summary>
    Mirror
}

/// <summary>Depth comparison function.</summary>
public enum DepthFunc
{
    Never,
    Less,
    Equal,
    LessEqual,
    Greater,
    NotEqual,
    GreaterEqual,
    Always
}

/// <summary>Face culling mode.</summary>
public enum CullMode
{
    None,
    Front,
    Back
}

