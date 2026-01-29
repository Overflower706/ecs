using System;
using System.Collections.Generic;

namespace OVFL.ECS
{
    public class Context
    {
        private readonly List<Entity> _entities = new();
        private readonly Dictionary<Type, HashSet<Entity>> _componentToEntities = new();
        private readonly Queue<int> _availableIDs = new();
        private readonly List<int> _generations = new();
        private int _nextEntityID = 1;

        public Context()
        {
            _generations.Add(0);
            var entity = new Entity(0, 0);
            _entities.Add(entity);
        }

        public Entity CreateEntity()
        {
            // ID 풀에서 재사용 가능한 ID가 있으면 사용, 없으면 새 ID 생성
            int entityID;

            if (_availableIDs.Count > 0)
            {
                entityID = _availableIDs.Dequeue();
            }
            else
            {
                entityID = _nextEntityID++;
                _generations.Add(1);
            }

            var entity = new Entity(entityID, _generations[entityID]);
            _entities.Add(entity);

            return entity;
        }

        public bool DestroyEntity(Entity entity)
        {
            if (!IsAlive(entity)) return false;

            if (_entities.Remove(entity))
            {
                // 모든 캐시에서 엔티티 제거
                foreach (var componentSet in _componentToEntities.Values)
                {
                    componentSet.Remove(entity);
                }

                _generations[entity.ID]++;
                _availableIDs.Enqueue(entity.ID);

                return true;
            }
            return false;
        }

        public bool IsAlive(Entity entity)
        {
            if (entity == Entity.Null) return false;

            if (entity.ID <= 0 || entity.ID >= _generations.Count) return false;

            return _generations[entity.ID] == entity.Generation;
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
    }
}
