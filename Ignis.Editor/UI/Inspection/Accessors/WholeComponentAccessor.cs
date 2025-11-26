using Ignis.Editor.UI.Inspection.Core;
using Ignis.Engine.Reactive;

namespace Ignis.Editor.UI.Inspection.Accessors;

/// <summary>
/// Accessor that treats the entire component as a single value.
/// Used when a component type has a registered inspector (e.g., Position, Rotation).
/// </summary>
public class WholeComponentAccessor<T> : IAccessor<T> where T : struct
{
    private readonly Friflo.Engine.ECS.Entity _entity;
    private readonly Friflo.Engine.ECS.ComponentType _componentType;
    
    public Signal<T> Signal { get; }

    public string Name => _componentType.Name;
    public Type Type => typeof(T);

    public WholeComponentAccessor(Friflo.Engine.ECS.Entity entity, Friflo.Engine.ECS.ComponentType componentType)
    {
        _entity = entity;
        _componentType = componentType;

        var initial = GetValueTyped();
        Signal = new Signal<T>(initial);

        // UI -> ECS: Watch signal changes and write to ECS
        _ = new Effect(() =>
        {
            var val = Signal.Value;
            SetValue(val);
        });
    }

    public object? GetValue() => GetValueTyped();

    private T GetValueTyped()
    {
        var method = typeof(Friflo.Engine.ECS.Entity).GetMethod(nameof(Friflo.Engine.ECS.Entity.GetComponent), Type.EmptyTypes)
            ?.MakeGenericMethod(_componentType.Type);
        var component = method?.Invoke(_entity, null);
        return component is T t ? t : default!;
    }

    public void SetValue(object? value)
    {
        if (value is not T typedValue) return;
        
        var method = typeof(Friflo.Engine.ECS.Entity).GetMethod(nameof(Friflo.Engine.ECS.Entity.AddComponent), 1, [_componentType.Type])
            ?.MakeGenericMethod(_componentType.Type);
        method?.Invoke(_entity, [typedValue]);
    }

    public void Update()
    {
        var current = GetValueTyped();
        
        if (!EqualityComparer<T>.Default.Equals(current, Signal.Value))
        {
            Signal.Value = current;
        }
    }
}

