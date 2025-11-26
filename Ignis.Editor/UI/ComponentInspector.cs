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

        // Style: Darker header with bottom border
        var header = new Panel(new Text
        {
            Content = componentName,
            Color = Color.White, 
            Layout = { PaddingTop = Units.Pixels(6), PaddingLeft = Units.Pixels(4) }
        })
        {
            Layout =
            {
                Height = Units.Pixels(32),
                Width = Units.Stretch(1),
                PaddingBottom = Units.Pixels(0),
                // Margin top for separation between components
                PaddingTop = Units.Pixels(8) 
            },
            BackgroundColor = Color.FromNonPremultiplied(45, 45, 48, 255),
            BorderColor = Color.FromNonPremultiplied(30, 30, 30, 255),
            BorderThickness = 1f
        };

        _propertyGrid.AddProperty("", header);

        InspectComponentFields(entity, componentType);
    }

    private void InspectComponentFields(Entity entity, ComponentType componentType)
    {
        var componentClrType = componentType.Type;

        foreach (var field in componentClrType.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            var fieldType = field.FieldType;
            var component = GetComponentValue(entity, componentType);
            var fieldValue = field.GetValue(component);

            var editor = CreateFieldEditor(entity, componentType, field, fieldType, fieldValue);
            if (editor != null)
            {
                // If field is 'value', hide the label to avoid redundancy with header
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

        return new Text
        {
            Content = value?.ToString() ?? "null",
            Color = Color.Gray
        };
    }

    private IView CreateVector3Editor(Entity entity, ComponentType componentType, FieldInfo field, Vector3 value)
    {
        var container = new Panel
        {
            Layout = { LayoutType = LayoutType.Row, ColumnGap = Units.Pixels(4) },
            BackgroundColor = Color.Transparent
        };

        var colX = new Color(200, 60, 60);
        var colY = new Color(60, 180, 60);
        var colZ = new Color(60, 100, 220);

        container.AddChild(CreateFloatAxisField(entity, componentType, field, "", value.X, (v, newVal) => new Vector3(newVal, v.Y, v.Z), colX));
        container.AddChild(CreateFloatAxisField(entity, componentType, field, "", value.Y, (v, newVal) => new Vector3(v.X, newVal, v.Z), colY));
        container.AddChild(CreateFloatAxisField(entity, componentType, field, "", value.Z, (v, newVal) => new Vector3(v.X, v.Y, newVal), colZ));

        return container;
    }

    private IView CreateFloatAxisField(Entity entity, ComponentType componentType, FieldInfo field, string axis, float initialValue, Func<Vector3, float, Vector3> updater, Color axisColor)
    {
        var axisContainer = new Panel
        {
            Layout = { LayoutType = LayoutType.Row, Width = Units.Stretch(1) },
            BackgroundColor = Color.FromNonPremultiplied(40, 40, 40, 255), // Darker input bg
            CornerRadius = 2f // Slight rounding
        };

        // Colored bar indicator (thin strip)
        var axisIndicator = new Panel
        {
            BackgroundColor = axisColor,
            Layout =
            {
                Width = Units.Pixels(4),
                Height = Units.Stretch(1)
            }
        };

        var textSignal = new Engine.Reactive.Signal<string?>(initialValue.ToString("0.##"));
        var textField = new TextField(textSignal)
        {
            Layout = { Width = Units.Stretch(1), Height = Units.Pixels(22) },
            BackgroundColor = Color.Transparent // Transparent so it sits on axisContainer color
        };

        axisContainer.AddChild(axisIndicator);
        axisContainer.AddChild(textField);

        return axisContainer;
    }

    private IView CreateQuaternionEditor(Entity entity, ComponentType componentType, FieldInfo field, Quaternion value)
    {
        return new Text
        {
            Content = $"({value.X:F2}, {value.Y:F2}, {value.Z:F2}, {value.W:F2})",
            Color = Color.Gray
        };
    }

    private IView CreateFloatEditor(Entity entity, ComponentType componentType, FieldInfo field, float value)
    {
        var signal = new Engine.Reactive.Signal<string?>(value.ToString("0.###"));

        // Use wrapper panel for background styling
        var panel = new Panel(new TextField(signal) { BackgroundColor = Color.Transparent })
        {
            BackgroundColor = Color.FromNonPremultiplied(40, 40, 40, 255),
            CornerRadius = 2f,
            Layout = { Width = Units.Stretch(1), Height = Units.Pixels(22) }
        };
        return panel;
    }

    private IView CreateIntEditor(Entity entity, ComponentType componentType, FieldInfo field, int value)
    {
        var signal = new Engine.Reactive.Signal<string?>(value.ToString());
        var panel = new Panel(new TextField(signal) { BackgroundColor = Color.Transparent })
        {
            BackgroundColor = Color.FromNonPremultiplied(40, 40, 40, 255),
            CornerRadius = 2f,
            Layout = { Width = Units.Stretch(1), Height = Units.Pixels(22) }
        };
        return panel;
    }

    private IView CreateBoolEditor(Entity entity, ComponentType componentType, FieldInfo field, bool value)
    {
        var signal = new Engine.Reactive.Signal<bool>(value);
        return new Checkbox("", signal) { Layout = { Height = Units.Pixels(22) } };
    }

    private IView CreateStringEditor(Entity entity, ComponentType componentType, FieldInfo field, string value)
    {
        var signal = new Engine.Reactive.Signal<string?>(value);
        var panel = new Panel(new TextField(signal) { BackgroundColor = Color.Transparent })
        {
            BackgroundColor = Color.FromNonPremultiplied(40, 40, 40, 255),
            CornerRadius = 2f,
            Layout = { Width = Units.Stretch(1), Height = Units.Pixels(22) }
        };
        return panel;
    }
}