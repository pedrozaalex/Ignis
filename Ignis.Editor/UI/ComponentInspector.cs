using System.Numerics;
using System.Reflection;
using Friflo.Engine.ECS;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Elements;
using Ignis.Engine.UI.Widgets;
using Microsoft.Xna.Framework;
using static Ignis.Engine.UI.Elements.Elements;
using Vector3 = System.Numerics.Vector3;
using Quaternion = System.Numerics.Quaternion;

namespace Ignis.Editor.UI;

public class ComponentInspector
{
    private readonly PropertyGrid _propertyGrid = new();
    private Entity _currentEntity;

    public IView View => _propertyGrid;

    public void Inspect(Entity entity)
    {
        if (entity.Id == _currentEntity.Id && !_currentEntity.IsNull)
            return;

        _currentEntity = entity;
        _propertyGrid.Clear();

        if (entity.IsNull)
            return;

        AddEntityIdField(entity);

        foreach (var componentType in entity.Archetype.ComponentTypes)
        {
            AddComponentSection(entity, componentType);
        }
    }

    private void AddEntityIdField(Entity entity)
    {
        var idLabel = new Text
        {
            Content = $"ID: {entity.Id}",
            Color = Color.Gray,
            Layout = { PaddingBottom = Units.Pixels(10) }
        };
        _propertyGrid.AddProperty("Entity", idLabel);
    }

    private void AddComponentSection(Entity entity, ComponentType componentType)
    {
        var componentName = componentType.Name;

        var header = new Panel(new Text
        {
            Content = componentName,
            Color = Color.White, // Bold/White for contrast
            Layout = { PaddingTop = Units.Pixels(5) }
        })
        {
            Layout =
            {
                Height = Units.Pixels(28),
                PaddingLeft = Units.Pixels(8),
                PaddingTop = Units.Pixels(2),
                Width = Units.Stretch(1) // Ensure header stretches
            },
            BackgroundColor = Color.FromNonPremultiplied(60, 60, 60, 255) // Slightly lighter for header separation
        };

        _propertyGrid.AddProperty("", header);

        InspectComponentFields(entity, componentType);
    }

    private void InspectComponentFields(Entity entity, ComponentType componentType)
    {
        var componentClrType = componentType.Type;

        var fieldInfos = componentClrType.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fieldInfos)
        {
            var fieldType = field.FieldType;
            var component = GetComponentValue(entity, componentType);
            var fieldValue = field.GetValue(component);

            var editor = CreateFieldEditor(entity, componentType, field, fieldType, fieldValue);
            if (editor != null)
            {
                // Design Polish: Suppress the "value" label if the field name is "value"
                // This prevents "Position -> value" redundancy.
                string label = field.Name == "value" ? "" : field.Name;
                _propertyGrid.AddProperty(label, editor);
            }
        }
    }

    private object GetComponentValue(Entity entity, ComponentType componentType)
    {
        var getMethod = typeof(Entity).GetMethod("GetComponent", 1, Type.EmptyTypes);
        var genericMethod = getMethod!.MakeGenericMethod(componentType.Type);
        return genericMethod.Invoke(entity, null)!;
    }

    private IView? CreateFieldEditor(Entity entity, ComponentType componentType, FieldInfo field, Type fieldType, object? value)
    {
        if (fieldType == typeof(Vector3))
        {
            return CreateVector3Editor(entity, componentType, field, (Vector3)(value ?? Vector3.Zero));
        }

        if (fieldType == typeof(Quaternion))
        {
            return CreateQuaternionEditor(entity, componentType, field, (Quaternion)(value ?? Quaternion.Identity));
        }

        if (fieldType == typeof(float))
        {
            return CreateFloatEditor(entity, componentType, field, (float)(value ?? 0f));
        }

        if (fieldType == typeof(int))
        {
            return CreateIntEditor(entity, componentType, field, (int)(value ?? 0));
        }

        if (fieldType == typeof(bool))
        {
            return CreateBoolEditor(entity, componentType, field, (bool)(value ?? false));
        }

        if (fieldType == typeof(string))
        {
            return CreateStringEditor(entity, componentType, field, (string)(value ?? ""));
        }

        return null;
        
        return new Text
        {
            Content = value?.ToString() ?? "null",
            Color = Color.Gray
        };
    }

    private IView CreateVector3Editor(Entity entity, ComponentType componentType, FieldInfo field, Vector3 value)
    {
        // Horizontal Layout: [X val] [Y val] [Z val]
        var container = new Panel
        {
            Layout = { LayoutType = LayoutType.Row, ColumnGap = Units.Pixels(2) },
            BackgroundColor = Color.Transparent
        };

        // Standard axis colors (Red, Green, Blue)
        var colX = new Color(200, 60, 60);
        var colY = new Color(60, 180, 60);
        var colZ = new Color(60, 100, 220);

        container.AddChild(CreateFloatAxisField(entity, componentType, field, "X", value.X, (v, newVal) => new Vector3(newVal, v.Y, v.Z), colX));
        container.AddChild(CreateFloatAxisField(entity, componentType, field, "Y", value.Y, (v, newVal) => new Vector3(v.X, newVal, v.Z), colY));
        container.AddChild(CreateFloatAxisField(entity, componentType, field, "Z", value.Z, (v, newVal) => new Vector3(v.X, v.Y, newVal), colZ));

        return container;
    }

    private IView CreateFloatAxisField(Entity entity, ComponentType componentType, FieldInfo field, string axis, float initialValue, Func<Vector3, float, Vector3> updater, Color axisColor)
    {
        var axisContainer = new Panel
        {
            Layout = { LayoutType = LayoutType.Row, Width = Units.Stretch(1) },
            BackgroundColor = Color.FromNonPremultiplied(30, 30, 30, 255)
        };

        // Axis Label (Color coded background)
        var axisLabel = new Panel(new Text { Content = axis, Color = Color.White, FontSize = 12 }) // Small font
        {
            BackgroundColor = axisColor,
            Layout =
            {
                Width = Units.Pixels(12),
                Height = Units.Stretch(1),
                PaddingLeft = Units.Pixels(3),
                PaddingTop = Units.Pixels(3)
            }
        };

        var textSignal = new Engine.Reactive.Signal<string?>(initialValue.ToString("0.##")); // Compact format
        var textField = new TextField(textSignal)
        {
            Layout = { Width = Units.Stretch(1), Height = Units.Pixels(20) }, // Compact height
            BackgroundColor = Color.Transparent
        };

        axisContainer.AddChild(axisLabel);
        axisContainer.AddChild(textField);

        return axisContainer;
    }

    private IView CreateQuaternionEditor(Entity entity, ComponentType componentType, FieldInfo field, Quaternion value)
    {
        // Display simplified Euler angles if possible, or raw quaternion for now
        return new Text
        {
            Content = $"({value.X:F2}, {value.Y:F2}, {value.Z:F2}, {value.W:F2})",
            Color = Color.Gray
        };
    }

    private IView CreateFloatEditor(Entity entity, ComponentType componentType, FieldInfo field, float value)
    {
        var signal = new Engine.Reactive.Signal<string?>(value.ToString("0.###"));

        return new TextField(signal)
        {
            Layout = { Width = Units.Stretch(1), Height = Units.Pixels(20) }
        };
    }

    private IView CreateIntEditor(Entity entity, ComponentType componentType, FieldInfo field, int value)
    {
        var signal = new Engine.Reactive.Signal<string?>(value.ToString());

        return new TextField(signal)
        {
            Layout = { Width = Units.Stretch(1), Height = Units.Pixels(20) }
        };
    }

    private IView CreateBoolEditor(Entity entity, ComponentType componentType, FieldInfo field, bool value)
    {
        var signal = new Engine.Reactive.Signal<bool>(value);

        return new Checkbox("", signal)
        {
            Layout = { Height = Units.Pixels(20) }
        };
    }

    private IView CreateStringEditor(Entity entity, ComponentType componentType, FieldInfo field, string value)
    {
        var signal = new Engine.Reactive.Signal<string?>(value);

        return new TextField(signal)
        {
            Layout = { Width = Units.Stretch(1), Height = Units.Pixels(20) }
        };
    }
}