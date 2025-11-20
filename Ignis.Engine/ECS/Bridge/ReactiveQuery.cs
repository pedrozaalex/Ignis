using Friflo.Engine.ECS;
using Ignis.Engine.Reactive;

namespace Ignis.Engine.ECS.Bridge
{
    /// <summary>
    /// ReactiveQuery - A SignalList of entities that matches a query.
    /// Automatically updates when entities enter or leave the query filter.
    /// </summary>
    public class ReactiveQuery : SignalList<Entity>
    {
        private readonly ArchetypeQuery _query;

        public ReactiveQuery(ArchetypeQuery query)
        {
            _query = query;

            // Initial population
            foreach (var entity in query.Entities)
            {
                Add(entity);
            }

            // Subscribe to structural changes
            // Note: Friflo's event system may need specific setup
            // For now, we'll use a polling strategy via Update()
        }

        /// <summary>
        /// Polls the query for changes. Should be called once per frame.
        /// </summary>
        public void Update()
        {
            var currentEntities = _query.Entities.ToHashSet();
            var trackedEntities = Items.ToHashSet();

            // Find additions
            foreach (var entity in currentEntities.Where(entity => !trackedEntities.Contains(entity)))
            {
                Add(entity);
            }

            // Find removals
            foreach (var entity in trackedEntities.Where(entity => !currentEntities.Contains(entity)).ToList())
            {
                Remove(entity);
            }
        }
    }
}

