using System;
using System.Collections.Generic;

namespace OVFL.ECS
{
    public class Context
    {
        private readonly List<Entity> _entities = new();
        private readonly Dictionary<Type, HashSet<Entity>> _componentToEntities = new();
        private readonly Queue<int> _availableIDs = new();
        private int _nextEntityID = 1;

        public Entity CreateEntity()
        {
            // ID 풀에서 재사용 가능한 ID가 있으면 사용, 없으면 새 ID 생성
            int entityID = _availableIDs.Count > 0 ? _availableIDs.Dequeue() : _nextEntityID++;

            var entity = new Entity(entityID);
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

                // 제거된 엔티티의 ID를 풀에 추가하여 재사용 가능하게 함
                _availableIDs.Enqueue(entity.ID);

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
