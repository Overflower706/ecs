using System.Collections.Generic;

namespace OVFL.ECS
{
    public class Context
    {
        private readonly List<Entity> _entities = new();
        private int _nextEntityID = 1;

        public Entity CreateEntity()
        {
            var entity = new Entity(_nextEntityID++);
            _entities.Add(entity);
            return entity;
        }

        public bool DestroyEntity(Entity entity)
        {
            return _entities.Remove(entity);
        }

        public IReadOnlyList<Entity> GetEntities()
        {
            return _entities.AsReadOnly();
        }

        public List<Entity> GetEntitiesWithComponent<T>() where T : class, IComponent
        {
            var result = new List<Entity>();

            for (int i = 0; i < _entities.Count; i++)
            {
                if (_entities[i].HasComponent<T>())
                {
                    result.Add(_entities[i]);
                }
            }

            return result;
        }
    }
}
