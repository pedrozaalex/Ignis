using System.Reflection;
using Ignis.Editor.UI.Inspection.Core;
using Ignis.Engine.Reactive;

namespace Ignis.Editor.UI.Inspection.Accessors;

public class ComponentAccessor<T> : IAccessor<T>
{
    private readonly Friflo.Engine.ECS.Entity _entity;
    private readonly Friflo.Engine.ECS.ComponentType _componentType;
    private readonly FieldInfo _field;
    
    public Signal<T> Signal { get; }

    public string Name => _field.Name;
    public Type Type => typeof(T);

    public ComponentAccessor(Friflo.Engine.ECS.Entity entity, Friflo.Engine.ECS.ComponentType componentType, FieldInfo field)
    {
        _entity = entity;
        _componentType = componentType;
        _field = field;

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
        var comp = GetEntityComponent();
        var val = _field.GetValue(comp);
        return val is T t ? t : default!;
    }

    public void SetValue(object? value)
    {
        var comp = GetEntityComponent();
        _field.SetValue(comp, value);
        SetEntityComponent(comp);
    }

    public void Update()
    {
        var current = GetValueTyped();
        
        if (!EqualityComparer<T>.Default.Equals(current, Signal.Value))
        {
            Signal.Value = current;
        }
    }

    private object GetEntityComponent()
    {
        var method = typeof(Friflo.Engine.ECS.Entity).GetMethod(nameof(Friflo.Engine.ECS.Entity.GetComponent), Type.EmptyTypes)
            ?.MakeGenericMethod(_componentType.Type);
        return method?.Invoke(_entity, null) ?? Activator.CreateInstance(_componentType.Type)!;
    }

    private void SetEntityComponent(object component)
    {
        var method = typeof(Friflo.Engine.ECS.Entity).GetMethod(nameof(Friflo.Engine.ECS.Entity.AddComponent), 1, [_componentType.Type])
            ?.MakeGenericMethod(_componentType.Type);
        method?.Invoke(_entity, [component]);
    }
}

