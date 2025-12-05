using System.Numerics;
using CrucibleUI.Widgets;
using Ignis.Graphics;

namespace Samples.Common;

public class CrucibleRenderer
{
    private readonly IRenderingServer _server;
    private readonly FontHandle _font;

    public CrucibleRenderer(IRenderingServer server, FontHandle font)
    {
        _server = server;
        _font = font;

        // Hook up text measurement
        Label.TextMeasurer = (text, size) =>
        {
            var (w, h) = server.MeasureText(font, text, size);
            return (w, h);
        };
    }

    public void Render(Widget root, IRenderCommandList cmd)
    {
        RenderWidget(root, cmd);
    }

    private void RenderWidget(Widget widget, IRenderCommandList cmd)
    {
        if (!widget.IsVisible) return;

        var x = widget.ComputedX;
        var y = widget.ComputedY;
        var w = widget.ComputedWidth;
        var h = widget.ComputedHeight;

        // Draw Background
        var bgColor = widget.BackgroundColor;
        if (widget is Button btn)
        {
            if (btn.IsPressed) bgColor = btn.PressedBackgroundColor;
            else if (btn.IsHovered || btn.IsFocused) bgColor = btn.HoverBackgroundColor;
        }

        if (bgColor.A > 0)
        {
            cmd.DrawQuad(new Vector2(x, y), new Vector2(w, h), new Color4(bgColor.R, bgColor.G, bgColor.B, bgColor.A));
        }

        // Draw Border
        var borderColor = widget.BorderColorValue;
        if (widget.IsFocused)
        {
            // Highlight focused items if they don't have a specific focus style
            // For buttons, we handled background. For others, maybe border?
            if (!(widget is Button))
            {
                borderColor = new WidgetColor(1f, 1f, 1f, 1f);
            }
        }

        if (borderColor.A > 0)
        {
            UIRenderer.DrawBorder(cmd, x, y, w, h, 2f, new Color4(borderColor.R, borderColor.G, borderColor.B, borderColor.A));
        }

        // Widget specific rendering
        if (widget is Label label)
        {
            var color = label.TextColor;
            cmd.DrawText(_font, label.Text, new Vector2(x, y), label.FontSizeValue, new Color4(color.R, color.G, color.B, color.A));
        }
        else if (widget is Button button)
        {
            // Center text in button
            var (tw, th) = _server.MeasureText(_font, button.Text, button.FontSizeValue);
            var tx = x + (w - tw) / 2;
            var ty = y + (h - th) / 2;
            var color = button.TextColor;
            cmd.DrawText(_font, button.Text, new Vector2(tx, ty), button.FontSizeValue, new Color4(color.R, color.G, color.B, color.A));
        }
        else if (widget is Slider slider)
        {
            RenderSlider(slider, cmd, x, y, w, h);
        }

        // Render Children
        foreach (var child in widget.ChildWidgets)
        {
            RenderWidget(child, cmd);
        }
    }

    private void RenderSlider(Slider slider, IRenderCommandList cmd, float x, float y, float w, float h)
    {
        // Track
        var tc = slider.TrackColor;
        cmd.DrawQuad(new Vector2(x, y), new Vector2(w, h), new Color4(tc.R, tc.G, tc.B, tc.A));

        // Fill
        var range = slider.Max - slider.Min;
        var pct = (slider.Value - slider.Min) / range;
        var fillW = w * pct;

        var fc = slider.FillColor;
        if (slider.IsFocused) fc = new WidgetColor(fc.R * 1.2f, fc.G * 1.2f, fc.B * 1.2f, fc.A);

        if (fillW > 0)
        {
            cmd.DrawQuad(new Vector2(x, y), new Vector2(fillW, h), new Color4(fc.R, fc.G, fc.B, fc.A));
        }
    }
}
