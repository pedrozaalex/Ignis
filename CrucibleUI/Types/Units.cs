using System.Runtime.CompilerServices;

namespace CrucibleUI.Types;

/// <summary>
/// Represents the type of units for spacing and sizing.
/// </summary>
public enum UnitsKind : byte
{
    /// <summary>
    /// Automatically determine the value.
    /// </summary>
    Auto,
    
    /// <summary>
    /// A number of logical pixels.
    /// </summary>
    Pixels,
    
    /// <summary>
    /// A percentage of the parent dimension.
    /// </summary>
    Percentage,
    
    /// <summary>
    /// A factor of the remaining free space.
    /// </summary>
    Stretch
}

/// <summary>
/// Units which describe spacing and size.
/// </summary>
/// <remarks>
/// <para>
/// When applied to space (left, right, top, bottom) with Auto, the spacing may be overridden 
/// by the parent's child-space on the same side.
/// </para>
/// <para>
/// When applied to size (width, height) Auto will either size to fit its children, or if there 
/// are no children the node will be sized based on the content_size property of the node.
/// </para>
/// <para>
/// For Stretch units, the remaining free space is the parent space minus the space and size of 
/// any fixed-size nodes in that axis. The remaining free space is then shared between any stretch 
/// nodes based on the ratio of their stretch factors.
/// </para>
/// </remarks>
public readonly struct Units : IEquatable<Units>
{
    /// <summary>
    /// The type of units.
    /// </summary>
    public UnitsKind Kind { get; }
    
    /// <summary>
    /// The value (pixels, percentage, or stretch factor).
    /// </summary>
    public float Value { get; }

    private Units(UnitsKind kind, float value)
    {
        Kind = kind;
        Value = value;
    }

    /// <summary>
    /// Creates a units value in pixels.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Units Pixels(float value) => new(UnitsKind.Pixels, value);

    /// <summary>
    /// Creates a units value as a percentage of the parent.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Units Percentage(float value) => new(UnitsKind.Percentage, value);

    /// <summary>
    /// Creates a units value as a stretch factor.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Units Stretch(float value) => new(UnitsKind.Stretch, value);

    /// <summary>
    /// Auto units - automatically determine the value.
    /// </summary>
    public static Units Auto => new(UnitsKind.Auto, 0);

    /// <summary>
    /// Returns true if the value is a stretch factor.
    /// </summary>
    public bool IsStretch => Kind == UnitsKind.Stretch;

    /// <summary>
    /// Returns true if the value is auto.
    /// </summary>
    public bool IsAuto => Kind == UnitsKind.Auto;

    /// <summary>
    /// Returns the units converted to pixels or a provided default.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ToPx(float parentValue, float defaultValue)
    {
        return Kind switch
        {
            UnitsKind.Pixels => Value,
            UnitsKind.Percentage => Value / 100.0f * parentValue,
            UnitsKind.Stretch => defaultValue,
            UnitsKind.Auto => defaultValue,
            _ => defaultValue
        };
    }

    public bool Equals(Units other) => Kind == other.Kind && Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is Units other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Kind, Value);
    
    public static bool operator ==(Units left, Units right) => left.Equals(right);
    public static bool operator !=(Units left, Units right) => !left.Equals(right);

    public override string ToString()
    {
        return Kind switch
        {
            UnitsKind.Auto => "auto",
            UnitsKind.Pixels => $"{Value}px",
            UnitsKind.Percentage => $"{Value}%",
            UnitsKind.Stretch => $"{Value}s",
            _ => "unknown"
        };
    }
}
