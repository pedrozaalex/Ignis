using Ignis.Engine.Core;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI;
using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Ignis.Engine.UI.Elements.Elements;
using ReactiveEffect = Ignis.Engine.Reactive.Effect;

namespace Ignis.Samples;

/// <summary>
/// TransformInspectorSample - Demonstrates the declarative UI API with Vector3 editing.
/// This example shows how to use Signal.Lens() for editing struct fields without boilerplate.
/// </summary>
public class TransformInspectorSample : IgnisGame
{
    private UIContext? _uiContext;
    private SpriteBatch? _spriteBatch;

    // Reactive state
    private readonly Signal<Vector3> _position = new(new Vector3(10, 20, 30));
    private readonly Signal<Vector3> _rotation = new(Vector3.Zero);
    private readonly Signal<Vector3> _scale = new(Vector3.One);

    // Derived state
    private readonly Computed<string> _statusText;
    private readonly Computed<bool> _rotationNonZero;

    public TransformInspectorSample() : base(new IgnisApp(new EngineSettings
    {
        WindowTitle = "Ignis UI - Transform Inspector (Declarative API Demo)",
        WindowWidth = 600,
        WindowHeight = 800
    }))
    {
        _statusText = Computed<string>.From(() =>
            $"POS: ({_position.Value.X:F2}, {_position.Value.Y:F2}, {_position.Value.Z:F2})"
        );
        _rotationNonZero = Computed<bool>.From(() => _rotation.Value != Vector3.Zero);
    }

    protected override void Initialize()
    {
        base.Initialize();

        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _uiContext = new UIContext(GraphicsDevice);

        LoadFont();

        System.Console.WriteLine("=== Transform Inspector Sample ===");
        System.Console.WriteLine("Demonstrating Declarative UI with Signal.Lens()");
        System.Console.WriteLine("==================================");

        var ui = BuildTransformInspector();
        _uiContext.SetRoot(ui);

        SetupReactiveLogging();
    }

    private void LoadFont()
    {
        var contentPath = Path.Combine(Directory.GetCurrentDirectory(), Content.RootDirectory);
        var fontSpriteFontPath = Path.Combine(contentPath, "DefaultFont.spritefont");

        if (!File.Exists(fontSpriteFontPath))
        {
            GenerateDefaultFontFile(fontSpriteFontPath);
            ContentBuilder.BuildFont(contentPath, "DefaultFont.spritefont");
        }

        try
        {
            var font = Content.Load<SpriteFont>("DefaultFont");
            _uiContext?.SetDefaultFont(font);
            System.Console.WriteLine("✓ Font loaded successfully");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"⚠ Could not load font: {ex.Message}");
        }
    }

    private static void GenerateDefaultFontFile(string path)
    {
        var fontContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<XnaContent xmlns:Graphics=""Microsoft.Xna.Framework.Content.Pipeline.Graphics"">
  <Asset Type=""Graphics:FontDescription"">
    <FontName>Arial</FontName>
    <Size>14</Size>
    <Spacing>0</Spacing>
    <UseKerning>true</UseKerning>
    <Style>Regular</Style>
    <CharacterRegions>
      <CharacterRegion>
        <Start>&#32;</Start>
        <End>&#126;</End>
      </CharacterRegion>
    </CharacterRegions>
  </Asset>
</XnaContent>";
        File.WriteAllText(path, fontContent);
    }

    /// <summary>
    /// The Declarative UI Layout - This is the key demo!
    /// Shows: Method chaining, Signal.Lens(), Computed values, conditional rendering
    /// Uses children-last API for better readability
    /// </summary>
    private IView BuildTransformInspector()
    {
        return Panel()
            .Background(new Color(30, 30, 32))
            .Width(Units.Stretch(1))
            .Height(Units.Stretch(1))
            .Padding(30)
            .AlignCenter()
            .Children(
                Column(
                    // Header
                    Label("Transform Inspector", null, new Color(100, 200, 255))
                        .Padding(20)
                        .PaddingBottom(10),

                    Label("Declarative API Demo", null, Color.Gray)
                        .PaddingLeft(20)
                        .PaddingBottom(20),

                    // Position Vector3 Field using Signal.Lens()
                    Vector3Field("Position", _position)
                        .PaddingLeft(20)
                        .PaddingRight(20)
                        .PaddingBottom(10),

                    // Rotation with conditional Reset button
                    Row(
                        Vector3Field("Rotation", _rotation)
                            .Width(Units.Stretch(1)),

                        // Only shows button if rotation is not zero
                        Bind.If(
                            _rotationNonZero,
                            () => Button("Reset", () => _rotation.Value = Vector3.Zero)
                                .Width(80)
                                .PaddingLeft(10)
                        )
                    )
                    .PaddingLeft(20)
                    .PaddingRight(20)
                    .PaddingBottom(10),

                    // Scale
                    Vector3Field("Scale", _scale)
                        .PaddingLeft(20)
                        .PaddingRight(20)
                        .PaddingBottom(20),

                    // Separator
                    Rule()
                        .PaddingBottom(15),

                    // Status Bar (Derived Signal)
                    Label(_statusText, null, Color.LightGray)
                        .Padding(20)
                        .PaddingBottom(10),

                    // Action Buttons
                    Row(
                        Button("Reset All", ResetAllTransforms)
                            .Width(120),

                        Spacer(10),

                        Button("Randomize", RandomizeTransforms)
                            .Width(120)
                    )
                    .Padding(20)
                    .AlignCenter()
                )
            );
    }

    /// <summary>
    /// Reusable UI Component helper - creates a field for editing Vector3
    /// using Signal.Lens() to create bidirectional bindings to X, Y, Z components
    /// </summary>
    private IView Vector3Field(string label, Signal<Vector3> vector)
    {
        return Column(
            Label(label, null, Color.White)
                .PaddingBottom(5),

            Row(
                // We create "Lenses" (2-way bindings) to individual struct fields
                // vector.Lens(v => v.X, (v, x) => v with { X = x })
                // creates a Signal<float> that writes back to the Signal<Vector3>
                FloatFieldWithLabel("X:", vector.Lens(v => v.X, (v, x) => new Vector3(x, v.Y, v.Z)))
                    .Width(Units.Stretch(1)),

                Spacer(10),

                FloatFieldWithLabel("Y:", vector.Lens(v => v.Y, (v, y) => new Vector3(v.X, y, v.Z)))
                    .Width(Units.Stretch(1)),

                Spacer(10),

                FloatFieldWithLabel("Z:", vector.Lens(v => v.Z, (v, z) => new Vector3(v.X, v.Y, z)))
                    .Width(Units.Stretch(1))
            )
        );
    }

    /// <summary>
    /// Helper for creating a compact float field with inline label
    /// </summary>
    private IView FloatFieldWithLabel(string label, Signal<float> value)
    {
        return Row(
            Label(label, null, Color.LightGray)
                .Width(25)
                .PaddingTop(6),

            Label(Computed<string>.From(() => value.Value.ToString("F2")), null, Color.White)
                .Width(Units.Stretch(1))
                .PaddingTop(6)
                .PaddingLeft(5)
        );
    }

    private void ResetAllTransforms()
    {
        _position.Value = Vector3.Zero;
        _rotation.Value = Vector3.Zero;
        _scale.Value = Vector3.One;
        System.Console.WriteLine("[ACTION] Reset all transforms");
    }

    private void RandomizeTransforms()
    {
        var rand = new Random();
        _position.Value = new Vector3(
            (float)(rand.NextDouble() * 100 - 50),
            (float)(rand.NextDouble() * 100 - 50),
            (float)(rand.NextDouble() * 100 - 50)
        );
        _rotation.Value = new Vector3(
            (float)(rand.NextDouble() * 360),
            (float)(rand.NextDouble() * 360),
            (float)(rand.NextDouble() * 360)
        );
        _scale.Value = new Vector3(
            (float)(rand.NextDouble() * 2 + 0.5),
            (float)(rand.NextDouble() * 2 + 0.5),
            (float)(rand.NextDouble() * 2 + 0.5)
        );
        System.Console.WriteLine("[ACTION] Randomized transforms");
    }

    private void SetupReactiveLogging()
    {
        new ReactiveEffect(() =>
        {
            System.Console.WriteLine($"[REACTIVE] Position: {_position.Value}");
        });

        new ReactiveEffect(() =>
        {
            System.Console.WriteLine($"[REACTIVE] Rotation: {_rotation.Value}");
        });

        new ReactiveEffect(() =>
        {
            System.Console.WriteLine($"[REACTIVE] Scale: {_scale.Value}");
        });
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _uiContext?.Update(gameTime);

        // Simulate automatic rotation for demo
        var totalSeconds = gameTime.TotalGameTime.TotalSeconds;
        if (totalSeconds % 5.0 < 0.016 && totalSeconds > 1.0)
        {
            _rotation.Value = new Vector3(
                _rotation.Value.X + 45,
                _rotation.Value.Y,
                _rotation.Value.Z
            );
            System.Console.WriteLine("[AUTO] Incremented rotation");
        }
    }

    protected override void OnRenderUI(SpriteBatch spriteBatch)
    {
        base.OnRenderUI(spriteBatch);
        _uiContext?.Draw(spriteBatch);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _spriteBatch?.Dispose();
            _uiContext?.Dispose();
        }
        base.Dispose(disposing);
    }
}

