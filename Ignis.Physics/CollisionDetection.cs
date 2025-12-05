using System.Numerics;

namespace Ignis.Physics;

/// <summary>
/// Static collision detection utilities.
/// All methods are branchless/SIMD-friendly where possible.
/// </summary>
public static class CollisionDetection
{
    /// <summary>
    /// Tests if two circles overlap.
    /// </summary>
    public static bool CircleVsCircle(Vector2 posA, float radiusA, Vector2 posB, float radiusB)
    {
        var delta = posB - posA;
        var distSq = delta.LengthSquared();
        var radiusSum = radiusA + radiusB;
        return distSq <= radiusSum * radiusSum;
    }
    
    /// <summary>
    /// Tests if two circles overlap and returns penetration info.
    /// </summary>
    public static bool CircleVsCircle(Vector2 posA, float radiusA, Vector2 posB, float radiusB, out Vector2 normal, out float depth)
    {
        var delta = posB - posA;
        var distSq = delta.LengthSquared();
        var radiusSum = radiusA + radiusB;
        
        if (distSq > radiusSum * radiusSum)
        {
            normal = Vector2.Zero;
            depth = 0;
            return false;
        }
        
        var dist = MathF.Sqrt(distSq);
        if (dist > 0.0001f)
        {
            normal = delta / dist;
            depth = radiusSum - dist;
        }
        else
        {
            // Circles are at same position - pick arbitrary normal
            normal = Vector2.UnitX;
            depth = radiusSum;
        }
        
        return true;
    }
    
    /// <summary>
    /// Tests if two AABBs overlap.
    /// </summary>
    public static bool BoxVsBox(Vector2 posA, Vector2 sizeA, Vector2 posB, Vector2 sizeB)
    {
        var halfA = sizeA * 0.5f;
        var halfB = sizeB * 0.5f;
        
        var minA = posA - halfA;
        var maxA = posA + halfA;
        var minB = posB - halfB;
        var maxB = posB + halfB;
        
        return minA.X <= maxB.X && maxA.X >= minB.X &&
               minA.Y <= maxB.Y && maxA.Y >= minB.Y;
    }
    
    /// <summary>
    /// Tests if two AABBs overlap and returns penetration info.
    /// </summary>
    public static bool BoxVsBox(Vector2 posA, Vector2 sizeA, Vector2 posB, Vector2 sizeB, out Vector2 normal, out float depth)
    {
        var halfA = sizeA * 0.5f;
        var halfB = sizeB * 0.5f;
        
        var delta = posB - posA;
        var overlapX = halfA.X + halfB.X - MathF.Abs(delta.X);
        var overlapY = halfA.Y + halfB.Y - MathF.Abs(delta.Y);
        
        if (overlapX <= 0 || overlapY <= 0)
        {
            normal = Vector2.Zero;
            depth = 0;
            return false;
        }
        
        if (overlapX < overlapY)
        {
            normal = new Vector2(MathF.Sign(delta.X), 0);
            depth = overlapX;
        }
        else
        {
            normal = new Vector2(0, MathF.Sign(delta.Y));
            depth = overlapY;
        }
        
        return true;
    }
    
    /// <summary>
    /// Tests if a circle and AABB overlap.
    /// </summary>
    public static bool CircleVsBox(Vector2 circlePos, float radius, Vector2 boxPos, Vector2 boxSize)
    {
        var halfSize = boxSize * 0.5f;
        var minBox = boxPos - halfSize;
        var maxBox = boxPos + halfSize;
        
        // Find closest point on box to circle center
        var closestX = MathF.Max(minBox.X, MathF.Min(circlePos.X, maxBox.X));
        var closestY = MathF.Max(minBox.Y, MathF.Min(circlePos.Y, maxBox.Y));
        var closest = new Vector2(closestX, closestY);
        
        var delta = circlePos - closest;
        return delta.LengthSquared() <= radius * radius;
    }
    
    /// <summary>
    /// Tests if a circle and AABB overlap and returns penetration info.
    /// </summary>
    public static bool CircleVsBox(Vector2 circlePos, float radius, Vector2 boxPos, Vector2 boxSize, out Vector2 normal, out float depth)
    {
        var halfSize = boxSize * 0.5f;
        var minBox = boxPos - halfSize;
        var maxBox = boxPos + halfSize;
        
        // Find closest point on box to circle center
        var closestX = MathF.Max(minBox.X, MathF.Min(circlePos.X, maxBox.X));
        var closestY = MathF.Max(minBox.Y, MathF.Min(circlePos.Y, maxBox.Y));
        var closest = new Vector2(closestX, closestY);
        
        var delta = circlePos - closest;
        var distSq = delta.LengthSquared();
        
        if (distSq > radius * radius)
        {
            normal = Vector2.Zero;
            depth = 0;
            return false;
        }
        
        var dist = MathF.Sqrt(distSq);
        if (dist > 0.0001f)
        {
            normal = delta / dist;
            depth = radius - dist;
        }
        else
        {
            // Circle center is inside box - find minimum penetration axis
            var penetrationX = halfSize.X - MathF.Abs(circlePos.X - boxPos.X) + radius;
            var penetrationY = halfSize.Y - MathF.Abs(circlePos.Y - boxPos.Y) + radius;
            
            if (penetrationX < penetrationY)
            {
                normal = new Vector2(MathF.Sign(circlePos.X - boxPos.X), 0);
                depth = penetrationX;
            }
            else
            {
                normal = new Vector2(0, MathF.Sign(circlePos.Y - boxPos.Y));
                depth = penetrationY;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Point vs circle test.
    /// </summary>
    public static bool PointInCircle(Vector2 point, Vector2 circlePos, float radius)
    {
        return (point - circlePos).LengthSquared() <= radius * radius;
    }
    
    /// <summary>
    /// Point vs AABB test.
    /// </summary>
    public static bool PointInBox(Vector2 point, Vector2 boxPos, Vector2 boxSize)
    {
        var halfSize = boxSize * 0.5f;
        var min = boxPos - halfSize;
        var max = boxPos + halfSize;
        
        return point.X >= min.X && point.X <= max.X &&
               point.Y >= min.Y && point.Y <= max.Y;
    }
}
