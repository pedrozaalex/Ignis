namespace CrucibleUI.Widgets;

/// <summary>
/// RGBA color value for widget styling.
/// </summary>
public readonly struct WidgetColor : IEquatable<WidgetColor>
{
    public float R { get; }
    public float G { get; }
    public float B { get; }
    public float A { get; }

    public WidgetColor(float r, float g, float b, float a = 1f)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public static WidgetColor Transparent => new(0, 0, 0, 0);
    public static WidgetColor White => new(1, 1, 1, 1);
    public static WidgetColor Black => new(0, 0, 0, 1);

    public bool Equals(WidgetColor other) =>
        R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A);

    public override bool Equals(object? obj) => obj is WidgetColor other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(R, G, B, A);

    public static bool operator ==(WidgetColor left, WidgetColor right) => left.Equals(right);
    public static bool operator !=(WidgetColor left, WidgetColor right) => !left.Equals(right);
}
