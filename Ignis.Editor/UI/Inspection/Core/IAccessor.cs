using Ignis.Engine.Reactive;

namespace Ignis.Editor.UI.Inspection.Core;

/// <summary>
/// Represents a handle to a property that can be read, written, and monitored.
/// </summary>
public interface IAccessor
{
    string Name { get; }
    Type Type { get; }
    
    /// <summary>
    /// Reads the current value from the source (ECS).
    /// </summary>
    object? GetValue();

    /// <summary>
    /// Writes a value back to the source (ECS).
    /// </summary>
    void SetValue(object? value);

    /// <summary>
    /// Call this every frame to sync the UI Signal with the ECS data 
    /// (in case physics or scripts changed the value).
    /// </summary>
    void Update();
}

/// <summary>
/// Typed version for convenience in Inspectors.
/// </summary>
public interface IAccessor<T> : IAccessor
{
    /// <summary>
    /// The reactive signal bound to the UI.
    /// </summary>
    Signal<T> Signal { get; }
}

