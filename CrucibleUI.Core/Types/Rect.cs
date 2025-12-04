namespace CrucibleUI.Core.Types;

/// <summary>
/// A type which represents the computed bounds of a node (position and size).
/// </summary>
public readonly struct Rect : IEquatable<Rect>
{
    public float PosX { get; }
    public float PosY { get; }
    public float Width { get; }
    public float Height { get; }

    public Rect(float posX, float posY, float width, float height)
    {
        PosX = posX;
        PosY = posY;
        Width = width;
        Height = height;
    }

    public bool Equals(Rect other) => 
        PosX.Equals(other.PosX) && 
        PosY.Equals(other.PosY) && 
        Width.Equals(other.Width) && 
        Height.Equals(other.Height);

    public override bool Equals(object? obj) => obj is Rect other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(PosX, PosY, Width, Height);
    
    public static bool operator ==(Rect left, Rect right) => left.Equals(right);
    public static bool operator !=(Rect left, Rect right) => !left.Equals(right);

    public override string ToString() => $"Rect {{ posx: {PosX}, posy: {PosY}, width: {Width}, height: {Height} }}";
}
