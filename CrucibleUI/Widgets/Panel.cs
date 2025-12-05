using CrucibleUI.Types;

namespace CrucibleUI.Widgets;

/// <summary>
/// A simple container panel that can hold children and has a background.
/// </summary>
public class Panel : Widget
{
    public Panel()
    {
        LayoutTypeValue = LayoutType.Column;
    }

    // Fluent builders with concrete return type
    public Panel Width(Units value) => Width<Panel>(value);
    public Panel Height(Units value) => Height<Panel>(value);
    public Panel Padding(Units value) => Padding<Panel>(value);
    public Panel PaddingHorizontal(Units value) => PaddingHorizontal<Panel>(value);
    public Panel PaddingVertical(Units value) => PaddingVertical<Panel>(value);
    public Panel Gap(Units value) => Gap<Panel>(value);
    public Panel Row() => Row<Panel>();
    public Panel Column() => Column<Panel>();
    public Panel Stretch() => Stretch<Panel>();
    public Panel Alignment(Alignment value) => Alignment<Panel>(value);
    public Panel Background(float r, float g, float b, float a = 1f) => Background<Panel>(r, g, b, a);
    public Panel BorderColor(float r, float g, float b, float a = 1f) => BorderColor<Panel>(r, g, b, a);
    public Panel CornerRadius(float radius) => CornerRadius<Panel>(radius);
    public Panel Visible(bool visible) => Visible<Panel>(visible);
    public Panel Disabled(bool disabled) => Disabled<Panel>(disabled);
    public Panel Children(params Widget[] children) => Children<Panel>(children);
}
