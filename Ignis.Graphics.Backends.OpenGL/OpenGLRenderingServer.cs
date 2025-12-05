using System.Numerics;
using FontStashSharp;
using Ignis.Core;
using Silk.NET.OpenGL;

namespace Ignis.Graphics.Backends.OpenGL;

/// <summary>
/// OpenGL implementation of the rendering server.
/// </summary>
public sealed class OpenGLRenderingServer : IRenderingServer
{
    private GL? _gl;
    private int _width;
    private int _height;

    // Resource storage
    private readonly Dictionary<int, GLMesh> _meshes = new();
    private readonly Dictionary<int, GLTexture> _textures = new();
    private readonly Dictionary<int, GLShader> _shaders = new();
    private readonly Dictionary<int, GLRenderTarget> _renderTargets = new();
    private readonly Dictionary<int, FontSystem> _fontSystems = new();

    // Font resolution factor (stored for scaling font requests)
    private const float FontResolutionFactor = 1.5f;

    private int _nextMeshId = 1;
    private int _nextTextureId = 1;
    private int _nextShaderId = 1;
    private int _nextRenderTargetId = 1;
    private int _nextFontId = 1;

    // Default shaders
    private ShaderHandle _defaultShader3D;
    private ShaderHandle _defaultShader3DLit;
    private ShaderHandle _defaultShader2D;
    private ShaderHandle _defaultShaderText;

    // 2D batching
    private GL2DBatch? _batch2D;

    // Font rendering
    private GLFontRenderer? _fontRenderer;

    // Current state
    private Matrix4x4 _currentProjection = Matrix4x4.Identity;
    private Matrix4x4 _currentView = Matrix4x4.Identity;
    private ShaderHandle _currentShader;
    private bool _disposed;

    public int Width => _width;
    public int Height => _height;

    /// <summary>Direct access to the underlying OpenGL API. Internal use only.</summary>
    public GL? GL => _gl;

    /// <summary>Direct access to the font renderer for text drawing.</summary>
    internal GLFontRenderer? FontRenderer => _fontRenderer;

    public ShaderHandle DefaultShader3D => _defaultShader3D;
    public ShaderHandle DefaultShader3DLit => _defaultShader3DLit;
    public ShaderHandle DefaultShader2D => _defaultShader2D;
    public ShaderHandle DefaultShaderText => _defaultShaderText;

    public RenderCapabilities Capabilities { get; private set; }

    /// <summary>
    /// Creates a new OpenGL rendering server.
    /// Call Initialize() with a Window before use.
    /// </summary>
    public OpenGLRenderingServer()
    {
    }

    /// <summary>
    /// Initialize from an Ignis.Core.Window. Call this in the OnLoad handler.
    /// </summary>
    public void Initialize(Window window)
    {
        var gl = GL.GetApi(window.NativeWindow);
        InitializeWithContext(gl, window.Width, window.Height);
    }

    public void Initialize(IntPtr windowHandle, int width, int height)
    {
        throw new NotSupportedException(
            "Direct window handle initialization is not supported. " +
            "Use Initialize(Window) instead."
        );
    }

    /// <summary>
    /// Initialize with an existing Silk.NET GL context.
    /// </summary>
    public void InitializeWithContext(GL gl, int width, int height)
    {
        _gl = gl;
        _width = width;
        _height = height;

        // Query capabilities
        gl.GetInteger(GetPName.MaxTextureSize, out var maxTexSize);
        gl.GetInteger(GetPName.MaxTextureImageUnits, out var maxTexUnits);
        var maxSamples = 8; // Default, actual query varies by extension

        Capabilities = new RenderCapabilities
        {
            BackendName = "OpenGL",
            MaxTextureSize = maxTexSize,
            MaxTextureSlots = maxTexUnits,
            SupportsCompute = true,
            SupportsGeometryShaders = true,
            SupportsTessellation = true,
            SupportsInstancing = true,
            SupportsMRT = true,
            MaxMSAASamples = maxSamples,
            SupportsAnisotropicFiltering = true,
            MaxAnisotropy = 16
        };

        // Create default shaders
        _defaultShader3D = CreateShader(DefaultShaders.Shader3DVertex, DefaultShaders.Shader3DFragment);
        _defaultShader3DLit = CreateShader(DefaultShaders.Shader3DVertex, DefaultShaders.Shader3DLitFragment);
        _defaultShader2D = CreateShader(DefaultShaders.Shader2DVertex, DefaultShaders.Shader2DFragment);
        _defaultShaderText = CreateShader(DefaultShaders.ShaderTextVertex, DefaultShaders.ShaderTextFragment);

        // Create 2D batch
        _batch2D = new GL2DBatch(gl);

        // Create font renderer
        _fontRenderer = new GLFontRenderer(gl);

        // Set default state
        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        gl.Enable(EnableCap.DepthTest);
        gl.DepthFunc(DepthFunction.Less);
    }

    public void Resize(int width, int height)
    {
        _width = width;
        _height = height;
        _gl?.Viewport(0, 0, (uint)width, (uint)height);
    }

    // --- Mesh Management ---

    public MeshHandle CreateMesh(MeshData data)
    {
        if (_gl == null) throw new InvalidOperationException("Not initialized");

        var mesh = new GLMesh(_gl, data);
        var id = _nextMeshId++;
        _meshes[id] = mesh;
        return new MeshHandle(id);
    }

    public void UpdateMesh(MeshHandle handle, MeshData data)
    {
        if (_meshes.TryGetValue(handle.Id, out var mesh))
            mesh.Update(data);
    }

    public void DestroyMesh(MeshHandle handle)
    {
        if (_meshes.Remove(handle.Id, out var mesh))
            mesh.Dispose();
    }

    // --- Texture Management ---

    public TextureHandle CreateTexture(ReadOnlySpan<byte> pixelData, TextureDesc desc)
    {
        if (_gl == null) throw new InvalidOperationException("Not initialized");

        var texture = new GLTexture(_gl, pixelData, desc.Width, desc.Height,
            desc.Format, desc.Filter, desc.Wrap, desc.GenerateMips);
        var id = _nextTextureId++;
        _textures[id] = texture;
        return new TextureHandle(id);
    }

    public TextureHandle CreateTextureFromFile(string path)
    {
        if (_gl == null) throw new InvalidOperationException("Not initialized");

        var texture = GLTexture.FromFile(_gl, path);
        var id = _nextTextureId++;
        _textures[id] = texture;
        return new TextureHandle(id);
    }

    public void UpdateTexture(TextureHandle handle, ReadOnlySpan<byte> pixelData, int x, int y, int width, int height)
    {
        if (_textures.TryGetValue(handle.Id, out var texture))
            texture.Update(pixelData, x, y, width, height);
    }

    public void DestroyTexture(TextureHandle handle)
    {
        if (_textures.Remove(handle.Id, out var texture))
            texture.Dispose();
    }

    // --- Shader Management ---

    public ShaderHandle CreateShader(string vertexSource, string fragmentSource)
    {
        if (_gl == null) throw new InvalidOperationException("Not initialized");

        var shader = new GLShader(_gl, vertexSource, fragmentSource);
        var id = _nextShaderId++;
        _shaders[id] = shader;
        return new ShaderHandle(id);
    }

    public ShaderHandle CreateShaderFromBytecode(ReadOnlySpan<byte> vertexBytecode, ReadOnlySpan<byte> fragmentBytecode)
    {
        throw new NotSupportedException("OpenGL does not support precompiled shader bytecode. Use source code.");
    }

    public void DestroyShader(ShaderHandle handle)
    {
        // Don't delete default shaders
        if (handle == _defaultShader3D || handle == _defaultShader2D || handle == _defaultShaderText)
            return;

        if (_shaders.Remove(handle.Id, out var shader))
            shader.Dispose();
    }

    // --- Font Management ---

    public FontHandle CreateFont(string name, ReadOnlySpan<byte> ttfData)
    {
        if (_fontRenderer == null) return FontHandle.Invalid;

        var settings = new FontSystemSettings
        {
            // Higher resolution for sharper text
            FontResolutionFactor = FontResolutionFactor,
            KernelWidth = 0,
            KernelHeight = 0,
            TextureWidth = 2048,
            TextureHeight = 2048,
            // Enable premultiplied alpha for better blending
            PremultiplyAlpha = true
        };

        var fontSystem = new FontSystem(settings);
        fontSystem.AddFont(ttfData.ToArray());

        var id = _nextFontId++;
        _fontSystems[id] = fontSystem;
        return new FontHandle(id);
    }

    public FontHandle CreateFontFromFile(string path)
    {
        if (!File.Exists(path)) return FontHandle.Invalid;
        var data = File.ReadAllBytes(path);
        return CreateFont(Path.GetFileNameWithoutExtension(path), data);
    }

    public void DestroyFont(FontHandle handle)
    {
        if (_fontSystems.Remove(handle.Id, out var fontSystem))
        {
            fontSystem.Dispose();
        }
    }

    public (float width, float height) MeasureText(FontHandle font, string text, float fontSize)
    {
        if (!_fontSystems.TryGetValue(font.Id, out var fontSystem))
            return (text.Length * fontSize * 0.5f, fontSize);

        // Scale font size by resolution factor for correct measurement
        var spriteFont = fontSystem.GetFont(fontSize * FontResolutionFactor);
        var size = spriteFont.MeasureString(text);
        return (size.X, size.Y);
    }

    // --- Render Target Management ---

    public RenderTargetHandle CreateRenderTarget(RenderTargetDesc desc)
    {
        if (_gl == null) throw new InvalidOperationException("Not initialized");

        var rt = new GLRenderTarget(_gl, desc.Width, desc.Height, desc.HasDepth);
        var id = _nextRenderTargetId++;
        _renderTargets[id] = rt;
        return new RenderTargetHandle(id);
    }

    public TextureHandle GetRenderTargetTexture(RenderTargetHandle handle)
    {
        if (_renderTargets.TryGetValue(handle.Id, out var rt))
        {
            // Create a texture handle that references the RT's color texture
            // This is a special case - we store the GL handle directly
            return new TextureHandle((int)rt.ColorTextureHandle);
        }

        return TextureHandle.Invalid;
    }

    public void DestroyRenderTarget(RenderTargetHandle handle)
    {
        if (_renderTargets.Remove(handle.Id, out var rt))
            rt.Dispose();
    }

    // --- Command List Management ---

    public IRenderCommandList CreateCommandList() => new RenderCommandList();

    public void Submit(IRenderCommandList commands)
    {
        if (_gl == null || commands is not RenderCommandList cmdList) return;

        foreach (var cmd in cmdList.Commands)
        {
            ExecuteCommand(cmd);
        }
    }

    private void ExecuteCommand(RenderCommand cmd)
    {
        if (_gl == null) return;

        switch (cmd.Type)
        {
            case CommandType.SetPipeline:
                _currentShader = cmd.Shader;
                if (_shaders.TryGetValue(cmd.Shader.Id, out var shader))
                    shader.Use();
                break;

            case CommandType.SetTexture:
                if (_textures.TryGetValue(cmd.Texture.Id, out var tex))
                    tex.Bind((TextureUnit)((int)TextureUnit.Texture0 + cmd.TextureSlot));
                break;

            case CommandType.SetBlendMode:
                ApplyBlendMode(cmd.BlendMode);
                break;

            case CommandType.SetDepthTest:
                if (cmd.DepthTestEnabled)
                {
                    _gl.Enable(EnableCap.DepthTest);
                    _gl.DepthFunc(ConvertDepthFunc(cmd.DepthFunc));
                }
                else
                {
                    _gl.Disable(EnableCap.DepthTest);
                }

                break;

            case CommandType.SetDepthWrite:
                _gl.DepthMask(cmd.DepthWriteEnabled);
                break;

            case CommandType.SetCullMode:
                ApplyCullMode(cmd.CullMode);
                break;

            case CommandType.SetScissor:
                if (cmd.ScissorEnabled)
                {
                    _gl.Enable(EnableCap.ScissorTest);
                    _gl.Scissor((int)cmd.ScissorRect.X, _height - (int)(cmd.ScissorRect.Y + cmd.ScissorRect.Height),
                        (uint)cmd.ScissorRect.Width, (uint)cmd.ScissorRect.Height);
                }
                else
                {
                    _gl.Disable(EnableCap.ScissorTest);
                }

                break;

            case CommandType.SetProjection:
                _currentProjection = cmd.Matrix;
                _batch2D?.SetProjection(cmd.Matrix);
                break;

            case CommandType.SetView:
                _currentView = cmd.Matrix;
                break;

            case CommandType.SetUniformVec3:
                if (_shaders.TryGetValue(_currentShader.Id, out var shaderVec3))
                    shaderVec3.SetVec3(cmd.UniformName!, cmd.UniformVec3);
                break;

            case CommandType.SetUniformFloat:
                if (_shaders.TryGetValue(_currentShader.Id, out var shaderFloat))
                    shaderFloat.SetFloat(cmd.UniformName!, cmd.UniformFloat);
                break;

            case CommandType.SetUniformColor:
                if (_shaders.TryGetValue(_currentShader.Id, out var shaderColor))
                    shaderColor.SetVec4(cmd.UniformName!, new System.Numerics.Vector4(cmd.Color.R, cmd.Color.G, cmd.Color.B, cmd.Color.A));
                break;

            case CommandType.DrawMesh:
                DrawMeshInternal(cmd.Mesh, cmd.Matrix);
                break;

            case CommandType.DrawQuad:
                Setup2DShaderIfNeeded();
                _batch2D?.DrawQuad(cmd.Position, cmd.Size, cmd.Color);
                break;

            case CommandType.DrawLine:
                Setup2DShaderIfNeeded();
                _batch2D?.DrawLine(cmd.Position, cmd.LineEnd, cmd.Color, cmd.Thickness);
                break;

            case CommandType.DrawSprite:
                Setup2DShaderIfNeeded();
                if (_textures.TryGetValue(cmd.Texture.Id, out var spriteTex))
                    _batch2D?.DrawQuad(cmd.Position, cmd.Size, cmd.Color, spriteTex.Handle);
                break;

            case CommandType.DrawText:
                DrawTextInternal(cmd.Font, cmd.Text ?? "", cmd.Position, cmd.FontSize, cmd.Color);
                break;

            case CommandType.DrawTextBounded:
                DrawTextBoundedInternal(cmd.Font, cmd.Text ?? "", cmd.TextBounds, cmd.FontSize, cmd.Color, cmd.HAlign, cmd.VAlign);
                break;
        }
    }

    private void Setup2DShaderIfNeeded()
    {
        // No longer needed - GL2DBatch now manages its own state at flush time
        // This method is kept for API compatibility but does nothing
    }

    private void DrawTextInternal(FontHandle fontHandle, string text, Vector2 position, float fontSize, Color4 color)
    {
        if (_fontRenderer == null || string.IsNullOrEmpty(text)) return;
        if (!_fontSystems.TryGetValue(fontHandle.Id, out var fontSystem)) return;

        // Flush any pending 2D batched draws first
        _batch2D?.Flush();

        var font = fontSystem.GetFont(fontSize * FontResolutionFactor);
        var textColor = new FSColor(
            (byte)(color.R * 255),
            (byte)(color.G * 255),
            (byte)(color.B * 255),
            (byte)(color.A * 255)
        );

        _fontRenderer.Begin(_currentProjection);
        font.DrawText(_fontRenderer, text, position, textColor);
        _fontRenderer.End();
    }

    private void DrawTextBoundedInternal(FontHandle fontHandle, string text, Rect bounds, float fontSize, Color4 color, HorizontalAlign hAlign, VerticalAlign vAlign)
    {
        if (_fontRenderer == null || string.IsNullOrEmpty(text)) return;
        if (!_fontSystems.TryGetValue(fontHandle.Id, out var fontSystem)) return;

        // Flush any pending 2D batched draws first
        _batch2D?.Flush();

        var font = fontSystem.GetFont(fontSize * FontResolutionFactor);
        var textSize = font.MeasureString(text);

        // Calculate position based on alignment
        float x = hAlign switch
        {
            HorizontalAlign.Center => bounds.X + (bounds.Width - textSize.X) / 2,
            HorizontalAlign.Right => bounds.X + bounds.Width - textSize.X,
            _ => bounds.X
        };

        float y = vAlign switch
        {
            VerticalAlign.Center => bounds.Y + (bounds.Height - textSize.Y) / 2,
            VerticalAlign.Bottom => bounds.Y + bounds.Height - textSize.Y,
            _ => bounds.Y
        };

        var textColor = new FontStashSharp.FSColor(
            (byte)(color.R * 255),
            (byte)(color.G * 255),
            (byte)(color.B * 255),
            (byte)(color.A * 255)
        );

        _fontRenderer.Begin(_currentProjection);
        font.DrawText(_fontRenderer, text, new Vector2(x, y), textColor);
        _fontRenderer.End();
    }

    private void DrawMeshInternal(MeshHandle meshHandle, Matrix4x4 worldMatrix)
    {
        if (!_meshes.TryGetValue(meshHandle.Id, out var mesh)) return;
        if (!_shaders.TryGetValue(_currentShader.Id, out var shader)) return;

        shader.Use();
        shader.SetMat4("uModel", worldMatrix);
        shader.SetMat4("uView", _currentView);
        shader.SetMat4("uProjection", _currentProjection);

        mesh.Draw();
    }

    private void ApplyBlendMode(BlendMode mode)
    {
        if (_gl == null) return;

        switch (mode)
        {
            case BlendMode.Opaque:
                _gl.Disable(EnableCap.Blend);
                break;
            case BlendMode.AlphaBlend:
                _gl.Enable(EnableCap.Blend);
                _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                break;
            case BlendMode.Additive:
                _gl.Enable(EnableCap.Blend);
                _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
                break;
            case BlendMode.Premultiplied:
                _gl.Enable(EnableCap.Blend);
                _gl.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
                break;
        }
    }

    private void ApplyCullMode(CullMode mode)
    {
        if (_gl == null) return;

        switch (mode)
        {
            case CullMode.None:
                _gl.Disable(EnableCap.CullFace);
                break;
            case CullMode.Front:
                _gl.Enable(EnableCap.CullFace);
                _gl.CullFace(TriangleFace.Front);
                break;
            case CullMode.Back:
                _gl.Enable(EnableCap.CullFace);
                _gl.CullFace(TriangleFace.Back);
                break;
        }
    }

    private static DepthFunction ConvertDepthFunc(DepthFunc func) => func switch
    {
        DepthFunc.Never => DepthFunction.Never,
        DepthFunc.Less => DepthFunction.Less,
        DepthFunc.Equal => DepthFunction.Equal,
        DepthFunc.LessEqual => DepthFunction.Lequal,
        DepthFunc.Greater => DepthFunction.Greater,
        DepthFunc.NotEqual => DepthFunction.Notequal,
        DepthFunc.GreaterEqual => DepthFunction.Gequal,
        DepthFunc.Always => DepthFunction.Always,
        _ => DepthFunction.Less
    };

    // --- Frame Control ---

    public void BeginPass(RenderPass pass)
    {
        if (_gl == null) return;

        // Bind render target
        if (pass.Target.IsScreen)
        {
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            _gl.Viewport(0, 0, (uint)_width, (uint)_height);
        }
        else if (_renderTargets.TryGetValue(pass.Target.Id, out var rt))
        {
            rt.Bind();
            _gl.Viewport(0, 0, (uint)rt.Width, (uint)rt.Height);
        }

        // Clear
        _gl.ClearColor(pass.ClearColor.R, pass.ClearColor.G, pass.ClearColor.B, pass.ClearColor.A);

        var clearMask = ClearBufferMask.ColorBufferBit;
        if (pass.ClearDepth)
            clearMask |= ClearBufferMask.DepthBufferBit;

        _gl.Clear(clearMask);

        // Apply viewport
        if (pass.Viewport.Width > 0 && pass.Viewport.Height > 0)
        {
            _gl.Viewport((int)pass.Viewport.X, (int)pass.Viewport.Y,
                (uint)pass.Viewport.Width, (uint)pass.Viewport.Height);
        }

        // Begin 2D batch for this pass
        _batch2D?.Begin();
    }

    public void EndPass()
    {
        if (_gl == null) return;

        // Flush any remaining 2D batch content
        // The batch manages its own shader and blend state at flush time
        _batch2D?.End();

        // Restore default depth state
        _gl.DepthFunc(DepthFunction.Less);
        _gl.DepthMask(true);

        // Unbind render target
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void SwapBuffers()
    {
        // Buffer swap is handled by the windowing system (Silk.NET.Windowing)
        // This is a no-op for the OpenGL backend
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _batch2D?.Dispose();
        _fontRenderer?.Dispose();

        foreach (var mesh in _meshes.Values) mesh.Dispose();
        foreach (var tex in _textures.Values) tex.Dispose();
        foreach (var shader in _shaders.Values) shader.Dispose();
        foreach (var rt in _renderTargets.Values) rt.Dispose();
        foreach (var font in _fontSystems.Values) font.Dispose();

        _meshes.Clear();
        _textures.Clear();
        _shaders.Clear();
        _renderTargets.Clear();
        _fontSystems.Clear();
    }
}