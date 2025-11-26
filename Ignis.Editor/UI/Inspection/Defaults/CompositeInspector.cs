using System.Reflection;
using Ignis.Editor.UI.Inspection.Core;
using Ignis.Engine.UI;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Widgets;
using Microsoft.Xna.Framework;
using static Ignis.Engine.UI.Elements.Elements;

namespace Ignis.Editor.UI.Inspection.Defaults;

public class CompositeInspector : IInspector
{
    private readonly Theme? _theme;

    public CompositeInspector(Theme? theme = null)
    {
        _theme = theme;
    }

    public IView CreateView(IAccessor accessor)
    {
        var type = accessor.Type;
        
        var container = new Panel 
        { 
            Layout = { LayoutType = LayoutType.Column, RowGap = Units.Pixels(2) } 
        };

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var field in fields)
        {
            var childAccessor = AccessorFactory.CreateNested(accessor, field);
            
            var childInspector = InspectorRegistry.GetInspector(field.FieldType);
            
            var labelColor = _theme?.TextMuted ?? Color.LightGray;
            var label = Label(field.Name, null, labelColor).Width(100);
            var editor = childInspector.CreateView(childAccessor);
            
            container.AddChild(Row(label, editor));
        }

        return container;
    }
}

public static class AccessorFactory
{
    public static IAccessor CreateNested(IAccessor parent, FieldInfo childField)
    {
        var accessorType = typeof(Accessors.NestedAccessor<,>).MakeGenericType(parent.Type, childField.FieldType);
        return (IAccessor)Activator.CreateInstance(accessorType, parent, childField)!;
    }
}

