using System.Numerics;
using Ignis.Graphics;

namespace Samples.Common;

/// <summary>
/// Reusable UI drawing utilities for sample scenes.
/// Reduces boilerplate code for common UI patterns like centered text, sliders, and borders.
/// </summary>
public sealed class UIRenderer
{
    private readonly IRenderingServer _server;
    private readonly FontHandle _font;

    public UIRenderer(IRenderingServer server, FontHandle font)
    {
        _server = server;
        _font = font;
    }

    // Static convenience methods for one-off calls
    public static void DrawText(IRenderCommandList cmd, FontHandle font, string text, float x, float y, float size, Color4 color)
    {
        if (!font.IsValid) return;
        cmd.DrawText(font, text, new Vector2(x, y), size, color);
    }

    public static void DrawCenteredText(IRenderCommandList cmd, IRenderingServer server, FontHandle font,
        string text, float x, float y, float size, Color4 color)
    {
        if (!font.IsValid) return;
        var (textWidth, textHeight) = server.MeasureText(font, text, size);
        cmd.DrawText(font, text, new Vector2(x - textWidth / 2, y - textHeight / 2), size, color);
    }

    public static void DrawBorder(IRenderCommandList cmd, float x, float y, float w, float h, float thickness, Color4 color)
    {
        cmd.DrawQuad(new Vector2(x, y), new Vector2(w, thickness), color); // Top
        cmd.DrawQuad(new Vector2(x, y + h - thickness), new Vector2(w, thickness), color); // Bottom
        cmd.DrawQuad(new Vector2(x, y), new Vector2(thickness, h), color); // Left
        cmd.DrawQuad(new Vector2(x + w - thickness, y), new Vector2(thickness, h), color); // Right
    }

    public static void DrawSlider(IRenderCommandList cmd, float x, float y, float width, float height,
        float value, Color4 trackColor, Color4 fillColor)
    {
        // Track
        cmd.DrawQuad(new Vector2(x, y), new Vector2(width, height), trackColor);

        // Fill
        var fillWidth = width * Math.Clamp(value, 0f, 1f);
        if (fillWidth > 0)
        {
            cmd.DrawQuad(new Vector2(x, y), new Vector2(fillWidth, height), fillColor);
        }
    }

    // Instance methods using stored server/font
    public void DrawText(IRenderCommandList cmd, string text, float x, float y, float size, Color4 color)
    {
        DrawText(cmd, _font, text, x, y, size, color);
    }

    public void DrawCenteredText(IRenderCommandList cmd, string text, float x, float y, float size, Color4 color)
    {
        DrawCenteredText(cmd, _server, _font, text, x, y, size, color);
    }

    public void DrawSliderWithLabel(IRenderCommandList cmd, string label, float labelX, float sliderX, float y,
        float sliderWidth, float sliderHeight, float value, bool isSelected, Color4 textColor)
    {
        // Label
        DrawText(cmd, label, labelX, y - sliderHeight / 2 - 2, 20f, textColor);

        // Slider
        var trackColor = new Color4(0.2f, 0.2f, 0.3f, 1f);
        var fillColor = isSelected ? new Color4(0.4f, 0.7f, 1f, 1f) : new Color4(0.3f, 0.5f, 0.8f, 1f);
        DrawSlider(cmd, sliderX, y - sliderHeight / 2, sliderWidth, sliderHeight, value, trackColor, fillColor);

        // Value text
        DrawText(cmd, $"{(int)(value * 100)}%", sliderX + sliderWidth + 20f, y - 8, 16f, textColor);

        // Selection arrows
        if (isSelected)
        {
            var arrowColor = new Color4(1f, 0.8f, 0.2f, 1f);
            DrawText(cmd, "<", sliderX - 25f, y - 10, 20f, arrowColor);
            DrawText(cmd, ">", sliderX + sliderWidth + 70f, y - 10, 20f, arrowColor);
        }
    }

    public void DrawMenuItem(IRenderCommandList cmd, string text, float x, float y, bool isSelected,
        float selectedSize = 28f, float normalSize = 24f)
    {
        var color = isSelected ? Color4.White : new Color4(0.6f, 0.6f, 0.7f, 1f);
        var size = isSelected ? selectedSize : normalSize;

        DrawCenteredText(cmd, text, x, y, size, color);

        if (isSelected)
        {
            var arrowColor = new Color4(1f, 0.8f, 0.2f, 1f);
            DrawCenteredText(cmd, "> ", x - 100f, y, size, arrowColor);
            DrawCenteredText(cmd, " <", x + 100f, y, size, arrowColor);
        }
    }

    public void DrawLevelBox(IRenderCommandList cmd, float x, float y, float size,
        int levelNum, string? levelName, bool isSelected, bool isUnlocked)
    {
        // Background
        Color4 boxColor;
        if (isSelected)
            boxColor = isUnlocked ? new Color4(0.3f, 0.5f, 0.8f, 1f) : new Color4(0.4f, 0.2f, 0.2f, 1f);
        else if (isUnlocked)
            boxColor = new Color4(0.15f, 0.2f, 0.3f, 1f);
        else
            boxColor = new Color4(0.1f, 0.1f, 0.15f, 0.5f);

        cmd.DrawQuad(new Vector2(x, y), new Vector2(size, size), boxColor);

        // Border
        if (isSelected)
        {
            var borderColor = isUnlocked ? new Color4(0.5f, 0.8f, 1f, 1f) : new Color4(0.8f, 0.3f, 0.3f, 1f);
            UIRenderer.DrawBorder(cmd, x, y, size, size, 3f, borderColor);
        }

        // Level number
        var numColor = isUnlocked ? Color4.White : new Color4(0.4f, 0.4f, 0.4f, 1f);
        DrawCenteredText(cmd, levelNum.ToString(), x + size / 2, y + size * 0.25f, 32f, numColor);

        // Level name
        if (!string.IsNullOrEmpty(levelName))
        {
            var nameColor = isUnlocked ? new Color4(0.7f, 0.7f, 0.8f, 1f) : new Color4(0.3f, 0.3f, 0.35f, 1f);
            DrawCenteredText(cmd, levelName, x + size / 2, y + size * 0.6f, 12f, nameColor);
        }

        // Lock icon
        if (!isUnlocked)
        {
            DrawCenteredText(cmd, "[LOCKED]", x + size / 2, y + size * 0.8f, 10f, new Color4(0.5f, 0.3f, 0.3f, 1f));
        }
    }
}
