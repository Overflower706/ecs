using System;
using System.Collections.Generic;

namespace OVFL.ECS
{
    public class Context
    {
        private readonly List<Entity> _entities = new();
        private readonly Dictionary<Type, HashSet<Entity>> _componentToEntities = new();
        private int _nextEntityID = 1;

        public Entity CreateEntity()
        {
            var entity = new Entity(_nextEntityID++);
            _entities.Add(entity);

            // 엔티티에 컴포넌트 변경 이벤트 구독
            entity.OnComponentAdded += OnEntityComponentAdded;
            entity.OnComponentRemoved += OnEntityComponentRemoved;

            return entity;
        }

        public bool DestroyEntity(Entity entity)
        {
            if (_entities.Remove(entity))
            {
                // 모든 캐시에서 엔티티 제거
                foreach (var componentSet in _componentToEntities.Values)
                {
                    componentSet.Remove(entity);
                }

                entity.OnComponentAdded -= OnEntityComponentAdded;
                entity.OnComponentRemoved -= OnEntityComponentRemoved;
                return true;
            }
            return false;
        }

        public IReadOnlyList<Entity> GetEntities()
        {
            return _entities.AsReadOnly();
        }

        public List<Entity> GetEntitiesWithComponent<T>() where T : class, IComponent
        {
            var componentType = typeof(T);

            if (_componentToEntities.TryGetValue(componentType, out var entitySet))
            {
                return new List<Entity>(entitySet);
            }

            return new List<Entity>();
        }

        private void OnEntityComponentAdded(Entity entity, Type componentType)
        {
            if (!_componentToEntities.TryGetValue(componentType, out var entitySet))
            {
                entitySet = new HashSet<Entity>();
                _componentToEntities[componentType] = entitySet;
            }

            entitySet.Add(entity);
        }

        private void OnEntityComponentRemoved(Entity entity, Type componentType)
        {
            if (_componentToEntities.TryGetValue(componentType, out var entitySet))
            {
                entitySet.Remove(entity);
            }
        }
    }
}
