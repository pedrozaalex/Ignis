using Friflo.Engine.ECS;

namespace Ignis.Editor.Systems;

public class SelectionSystem
{
    private readonly Engine.Reactive.Signal<Entity> _selectedEntity = new(default);

    public Engine.Reactive.Signal<Entity> SelectedEntity => _selectedEntity;

    public void Select(Entity entity)
    {
        _selectedEntity.Value = entity;
    }

    public void Clear()
    {
        _selectedEntity.Value = default;
    }

    public bool IsSelected(Entity entity)
    {
        return !_selectedEntity.Value.IsNull && _selectedEntity.Value.Id == entity.Id;
    }
}
