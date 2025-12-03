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
    private readonly Theme _theme;

    public IView View => _propertyGrid;

    public ComponentInspectorV2(Theme theme)
    {
        _theme = theme;
        InitializeRegistry();
    }

    private void InitializeRegistry()
    {
        InspectorRegistry.Fallback = new ReadOnlyInspector(_theme);
        InspectorRegistry.Composite = new CompositeInspector(_theme);

        InspectorRegistry.Register<float>(new NumericInspector(_theme));
        InspectorRegistry.Register<int>(new NumericInspector(_theme));
        InspectorRegistry.Register<bool>(new BoolInspector());
        InspectorRegistry.Register<string>(new StringInspector());
        InspectorRegistry.Register<string?>(new StringInspector());

        InspectorRegistry.Register<Position>(new Vector3ComponentInspector());
        InspectorRegistry.Register<Rotation>(new QuaternionComponentInspector(_theme));
        InspectorRegistry.Register<Scale3>(new Vector3ComponentInspector());
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
        _propertyGrid.AddProperty("Entity ID", Label(entity.Id.ToString(), null, _theme.TextMuted));
    }

    private void InspectComponent(Entity entity, ComponentType componentType)
    {
        var header = Panel(Title(componentType.Name))
            .Background(_theme.SurfaceOverlay)
            .Border(_theme.Border)
            .Padding(8)
            .AlignCenter()
            ;

        var section = Panel()
                .Padding(8)
                .Gap(4)
            ;
        
        // Check if there's a registered inspector for the component type itself
        var componentInspector = InspectorRegistry.GetInspector(componentType.Type);

        // If there's a specific inspector (not Composite or Fallback), use it for the whole component
        if (componentInspector != InspectorRegistry.Composite &&
            componentInspector != InspectorRegistry.Fallback)
        {
            // Create a single accessor for the entire component
            var accessorType = typeof(ComponentAccessor<>).MakeGenericType(componentType.Type);

            // For component-level access, we need a special accessor that reads the whole component
            // For now, we'll create a wrapper that can be used with the inspector
            var accessor = CreateComponentAccessor(entity, componentType);
            _activeAccessors.Add(accessor);

            var editor = componentInspector.CreateView(accessor);
            section.AddChild(editor);
        }
        else
        {
            // No specific inspector - iterate over fields
            var fields = componentType.Type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            if (fields.Length == 0) return; // Skip empty components

            foreach (var field in fields)
            {
                var accessorType = typeof(ComponentAccessor<>).MakeGenericType(field.FieldType);
                var accessor = (IAccessor)Activator.CreateInstance(accessorType, entity, componentType, field)!;

                _activeAccessors.Add(accessor);

                var inspector = InspectorRegistry.GetInspector(field.FieldType);
                var editor = inspector.CreateView(accessor);

                string label = field.Name == "value" ? "" : field.Name;
                // _propertyGrid.AddProperty(label, editor);
                section.AddChild(editor);
            }
        }
        
        _propertyGrid.AddProperty("", Column(header, section));
    }

    private IAccessor CreateComponentAccessor(Entity entity, ComponentType componentType)
    {
        // Create an accessor that accesses the component as a whole
        var accessorType = typeof(WholeComponentAccessor<>).MakeGenericType(componentType.Type);
        return (IAccessor)Activator.CreateInstance(accessorType, entity, componentType)!;
    }
}