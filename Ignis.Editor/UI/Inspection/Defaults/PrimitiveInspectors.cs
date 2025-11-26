using Ignis.Editor.UI.Inspection.Core;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Elements;
using Ignis.Engine.UI.Widgets;

namespace Ignis.Editor.UI.Inspection.Defaults;

public class FloatInspector : IInspector
{
    public IView CreateView(IAccessor accessor)
    {
        if (accessor is IAccessor<float> typed)
        {
            return Elements.FloatField("", typed.Signal);
        }
        if (accessor is IAccessor<int> intTyped)
        {
            // For int, show as readonly since FloatField expects Signal<float>
            return new Text { Content = intTyped.Signal.Value.ToString(), Color = Microsoft.Xna.Framework.Color.LightGray };
        }
        return new Text { Content = "Type Mismatch" };
    }
}

public class StringInspector : IInspector
{
    public IView CreateView(IAccessor accessor)
    {
        if (accessor is IAccessor<string?> typed)
        {
            return new TextField(typed.Signal);
        }
        if (accessor is IAccessor<string> typedNonNull)
        {
            // Create a signal wrapper that handles nullability
            var signal = new Signal<string?>(typedNonNull.Signal.Value);
            _ = new Effect(() => signal.Value = typedNonNull.Signal.Value);
            return new TextField(signal);
        }
        return new Text { Content = "Type Mismatch" };
    }
}

public class BoolInspector : IInspector
{
    public IView CreateView(IAccessor accessor)
    {
        if (accessor is IAccessor<bool> typed)
        {
            return new Checkbox("", typed.Signal);
        }
        return new Text { Content = "Type Mismatch" };
    }
}

public class ReadOnlyInspector : IInspector
{
    public IView CreateView(IAccessor accessor)
    {
        var val = accessor.GetValue();
        return new Text { Content = val?.ToString() ?? "null", Color = Microsoft.Xna.Framework.Color.Gray };
    }
}

