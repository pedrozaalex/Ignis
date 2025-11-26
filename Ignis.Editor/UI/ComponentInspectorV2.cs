using System.Reflection;
using Friflo.Engine.ECS;
using Ignis.Editor.UI.Inspection.Accessors;
using Ignis.Editor.UI.Inspection.Core;
using Ignis.Editor.UI.Inspection.Defaults;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Elements;
using Ignis.Engine.UI.Widgets;
using Microsoft.Xna.Framework;
using static Ignis.Engine.UI.Elements.Elements;

namespace Ignis.Editor.UI;

/// <summary>
/// Refactored ComponentInspector using the accessor-based architecture.
/// This version is extensible and supports nested struct editing.
/// </summary>
public class ComponentInspectorV2
{
    private readonly PropertyGrid _propertyGrid = new();
    private Entity _currentEntity;
    
    private readonly List<IAccessor> _activeAccessors = new();

    public IView View => _propertyGrid;

    public ComponentInspectorV2()
    {
        InitializeRegistry();
    }

    private void InitializeRegistry()
    {
        InspectorRegistry.Fallback = new ReadOnlyInspector();
        InspectorRegistry.Composite = new CompositeInspector();
        
        InspectorRegistry.Register<float>(new FloatInspector());
        InspectorRegistry.Register<int>(new FloatInspector());
        InspectorRegistry.Register<bool>(new BoolInspector());
        InspectorRegistry.Register<string>(new StringInspector());
        InspectorRegistry.Register<string?>(new StringInspector());
    }

    /// <summary>
    /// Call this every frame to sync ECS changes to UI.
    /// </summary>
    public void Update()
    {
        foreach (var accessor in _activeAccessors)
        {
            accessor.Update();
        }
    }

    public void Inspect(Entity entity)
    {
        if (entity.Id == _currentEntity.Id && !_currentEntity.IsNull) return;

        _currentEntity = entity;
        _propertyGrid.Clear();
        _activeAccessors.Clear();

        if (entity.IsNull) return;

        AddHeader(entity);

        foreach (var componentType in entity.Archetype.ComponentTypes)
        {
            InspectComponent(entity, componentType);
        }
    }

    private void AddHeader(Entity entity)
    {
        _propertyGrid.AddProperty("Entity ID", Label(entity.Id.ToString(), null, Color.Gray));
    }

    private void InspectComponent(Entity entity, ComponentType componentType)
    {
        var header = Panel(Label(componentType.Name).Padding(4))
            .Background(Color.FromNonPremultiplied(45, 45, 48, 255))
            .Border(Color.FromNonPremultiplied(30, 30, 30, 255))
            .Height(32)
            .Width(Units.Stretch(1));

        _propertyGrid.AddProperty("", header);

        var fields = componentType.Type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var accessorType = typeof(ComponentAccessor<>).MakeGenericType(field.FieldType);
            var accessor = (IAccessor)Activator.CreateInstance(accessorType, entity, componentType, field)!;

            _activeAccessors.Add(accessor);

            var inspector = InspectorRegistry.GetInspector(field.FieldType);
            var editor = inspector.CreateView(accessor);

            string label = field.Name == "value" ? "" : field.Name;
            _propertyGrid.AddProperty(label, editor);
        }
    }
}

