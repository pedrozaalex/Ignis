using System.Globalization;
using Ignis.Editor.UI.Inspection.Core;
using Ignis.Engine.Assets;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Elements;
using Ignis.Engine.UI.Widgets;
using static Ignis.Engine.UI.Elements.Elements;

namespace Ignis.Editor.UI.Inspection.Defaults;

public class NumericInspector : IInspector
{
    private readonly Theme? _theme;

    public NumericInspector(Theme? theme = null)
    {
        _theme = theme;
    }

    public IView CreateView(IAccessor accessor)
    {
        if (accessor is IAccessor<float> typed)
        {
            return Elements.FloatField("", typed.Signal);
        }

        if (accessor is IAccessor<int> intTyped)
        {
            // For int, show as readonly since FloatField expects Signal<float>
            var color = _theme?.TextMuted ?? Microsoft.Xna.Framework.Color.LightGray;
            return new Text { Content = intTyped.Signal.Value.ToString(), Color = color };
        }

        return new Text { Content = "Type Mismatch" };
    }
}

public class StringInspector : IInspector
{
    public IView CreateView(IAccessor accessor)
    {
        return Row(
                    Label(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(accessor.Name))
                        .Width(Units.Percentage(.1f))
                        .Height(DefaultFontProvider.DefaultFontSize)
                    ,
                    Editor()
                        .Width(Units.Stretch(1))
                )
                .Width(Units.Stretch(1))
                .Height(24)
                .Gap(8)
                .AlignCenter()
            ;

        IView Editor()
        {
            if (accessor is IAccessor<string?> typed)
                return new TextField(typed.Signal);

            if (accessor is not IAccessor<string> typedNonNull)
                return new Text { Content = "Type Mismatch" };

            // Create a signal wrapper that handles nullability
            var signal = new Signal<string?>(typedNonNull.Signal.Value);
            _ = new Effect(() => signal.Value = typedNonNull.Signal.Value);

            return new TextField(signal);
        }
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
    private readonly Theme? _theme;

    public ReadOnlyInspector(Theme? theme = null)
    {
        _theme = theme;
    }

    public IView CreateView(IAccessor accessor)
    {
        var val = accessor.GetValue();
        var color = _theme?.TextMuted ?? Microsoft.Xna.Framework.Color.Gray;
        return new Text { Content = val?.ToString() ?? "null", Color = color };
    }
}