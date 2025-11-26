using System.Reflection;
using Ignis.Editor.UI.Inspection.Core;
using Ignis.Engine.Reactive;

namespace Ignis.Editor.UI.Inspection.Accessors;

public class NestedAccessor<TParent, TChild> : IAccessor<TChild>
{
    private readonly IAccessor<TParent> _parent;
    private readonly FieldInfo _field;

    public Signal<TChild> Signal { get; }
    public string Name => _field.Name;
    public Type Type => typeof(TChild);

    public NestedAccessor(IAccessor<TParent> parent, FieldInfo field)
    {
        _parent = parent;
        _field = field;
        
        Signal = new Signal<TChild>(GetValueTyped());
        
        // UI -> ECS: Watch signal changes and write to parent
        _ = new Effect(() =>
        {
            var val = Signal.Value;
            SetValue(val);
        });
    }

    public object? GetValue() => GetValueTyped();

    private TChild GetValueTyped()
    {
        var parentVal = _parent.GetValue(); 
        if (parentVal == null) return default!;
        
        var val = _field.GetValue(parentVal);
        return val is TChild t ? t : default!;
    }

    public void SetValue(object? value)
    {
        var parentVal = _parent.GetValue();
        if (parentVal == null) return;

        _field.SetValue(parentVal, value);

        _parent.SetValue(parentVal);
    }

    public void Update()
    {
        _parent.Update();

        var current = GetValueTyped();
        if (!EqualityComparer<TChild>.Default.Equals(current, Signal.Value))
        {
            Signal.Value = current;
        }
    }
}

