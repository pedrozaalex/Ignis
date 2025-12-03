namespace Crucible.Core.Types;

/// <summary>
/// A type which represents the computed size of a node after layout.
/// </summary>
public readonly struct Size : IEquatable<Size>
{
    /// <summary>
    /// The computed size on the main axis.
    /// </summary>
    public float Main { get; }
    
    /// <summary>
    /// The computed size on the cross axis.
    /// </summary>
    public float Cross { get; }

    public Size(float main, float cross)
    {
        Main = main;
        Cross = cross;
    }

    public bool Equals(Size other) => Main.Equals(other.Main) && Cross.Equals(other.Cross);
    public override bool Equals(object? obj) => obj is Size other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Main, Cross);
    
    public static bool operator ==(Size left, Size right) => left.Equals(right);
    public static bool operator !=(Size left, Size right) => !left.Equals(right);

    public override string ToString() => $"Size(main: {Main}, cross: {Cross})";
}
