using Ignis.Editor.UI.Inspection.Core;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Widgets;
using Microsoft.Xna.Framework;
using static Ignis.Engine.UI.Elements.Elements;

namespace Ignis.Editor.UI.Inspection.Defaults;

/// <summary>
/// Custom inspector for Vector3-like components that use FieldOffset unions.
/// These components have both a 'value' field and separate x/y/z fields that overlap in memory,
/// which would normally cause the CompositeInspector to show duplicates.
/// This inspector only shows the x, y, z fields.
/// </summary>
public class Vector3ComponentInspector : IInspector
{
    public IView CreateView(IAccessor accessor)
    {
        var type = accessor.Type;
        var xField = type.GetField("x");
        var yField = type.GetField("y");
        var zField = type.GetField("z");

        if (xField == null || yField == null || zField == null)
        {
            // Fallback to composite if fields not found
            return InspectorRegistry.Composite.CreateView(accessor);
        }

        var container = new Panel
        {
            Layout = { LayoutType = LayoutType.Row, ColumnGap = Units.Pixels(4) },
            BackgroundColor = Color.Transparent
        };

        // Color coding for axes
        var colX = new Color(200, 60, 60);
        var colY = new Color(60, 180, 60);
        var colZ = new Color(60, 100, 220);

        // Create nested accessors for each axis
        var xAccessor = AccessorFactory.CreateNested(accessor, xField);
        var yAccessor = AccessorFactory.CreateNested(accessor, yField);
        var zAccessor = AccessorFactory.CreateNested(accessor, zField);

        container.AddChild(CreateAxisField("X: ", xAccessor, colX));
        container.AddChild(CreateAxisField("Y: ", yAccessor, colY));
        container.AddChild(CreateAxisField("Z: ", zAccessor, colZ));

        return container;
    }

    private IView CreateAxisField(string label, IAccessor accessor, Color axisColor)
    {
        // Create Input with number validation for the float value
        if (accessor is not IAccessor<float> floatAccessor)
            return Label("Invalid accessor type");

        // Convert float signal to string signal for Input
        var stringSignal = new Signal<string?>(floatAccessor.Signal.Value.ToString("F2"));

        // Sync string to float (user typing -> ECS)
        var input = new NumberInput(stringSignal);

        // Sync float to string (ECS -> UI display)
        // This is handled by the accessor Update() call in ComponentInspectorV2.Update()
        // But we need to update the string when the float changes
        _ = new Effect(() =>
        {
            var floatValue = floatAccessor.Signal.Value;
            var currentString = stringSignal.Value;
            var expectedString = floatValue.ToString("F2");

            // Only update if different to avoid cursor jumping during typing
            if (currentString == expectedString || string.IsNullOrEmpty(currentString)) return;

            // User finished typing, try to parse and update
            if (float.TryParse(currentString, out var parsed)) floatAccessor.SetValue(parsed);
        });

        return Row(
                Label(label).Height(14),
                AxisIndicator(),
                input
            )
            .AlignCenter()
            .Height(24)
            .FillWidth();

        IView AxisIndicator()
        {
            return Panel()
                .Background(axisColor)
                .PaddingLeft(2)
                .Width(4)
                .FillHeight();
        }
    }
}

/// <summary>
/// Inspector for Quaternion-like rotation components with FieldOffset unions.
/// Shows x, y, z, w fields (ignoring the 'value' field).
/// </summary>
public class QuaternionComponentInspector(Theme? theme = null) : IInspector
{
    public IView CreateView(IAccessor accessor)
    {
        var type = accessor.Type;
        var xField = type.GetField("x");
        var yField = type.GetField("y");
        var zField = type.GetField("z");
        var wField = type.GetField("w");

        if (xField == null || yField == null || zField == null || wField == null)
        {
            return InspectorRegistry.Composite.CreateView(accessor);
        }

        var container = new Panel
        {
            Layout = { LayoutType = LayoutType.Column, RowGap = Units.Pixels(2) },
            BackgroundColor = Color.Transparent
        };

        // Create nested accessors
        var xAccessor = AccessorFactory.CreateNested(accessor, xField);
        var yAccessor = AccessorFactory.CreateNested(accessor, yField);
        var zAccessor = AccessorFactory.CreateNested(accessor, zField);
        var wAccessor = AccessorFactory.CreateNested(accessor, wField);

        var floatInspector = InspectorRegistry.GetInspector(typeof(float));

        container.AddChild(CreateLabeledField("X", xAccessor, floatInspector));
        container.AddChild(CreateLabeledField("Y", yAccessor, floatInspector));
        container.AddChild(CreateLabeledField("Z", zAccessor, floatInspector));
        container.AddChild(CreateLabeledField("W", wAccessor, floatInspector));

        return container;
    }

    private IView CreateLabeledField(string labelText, IAccessor accessor, IInspector inspector)
    {
        var row = new Panel
        {
            Layout = { LayoutType = LayoutType.Row, ColumnGap = Units.Pixels(4) }
        };

        var labelColor = theme?.TextMuted ?? Color.LightGray;
        var label = Label(labelText, null, labelColor).Width(20);
        var editor = inspector.CreateView(accessor);

        if (editor is IView view)
        {
            view.Layout.Width = Units.Stretch(1);
        }

        row.AddChild(label);
        row.AddChild(editor);

        return row;
    }
}