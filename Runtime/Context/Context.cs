using System;
using System.Collections.Generic;

namespace OVFL.ECS
{
    public class Context
    {
        private readonly List<Entity> _entities = new();
        private readonly Queue<int> _availableIDs = new();
        private readonly List<int> _generations = new();
        private int _nextEntityID = 1;

        public Context()
        {
            _generations.Add(0);
        }

        public Entity CreateEntity()
        {
            int id;
            int generation;

            if (_availableIDs.Count > 0)
            {
                id = _availableIDs.Dequeue();
                generation = _generations[id];
            }
            else
            {
                id = _nextEntityID++;
                generation = 1;
                _generations.Add(generation);
            }

            var entity = new Entity(id, generation);
            _entities.Add(entity);

            return entity;
        }

        public bool DestroyEntity(Entity entity)
        {
            if (!IsAlive(entity)) return false;

            if (_entities.Remove(entity))
            {
                _generations[entity.ID]++;
                _availableIDs.Enqueue(entity.ID);

                return true;
            }
            return false;
        }

        public bool IsAlive(Entity entity)
        {
            if (entity == null) return false;
            if (entity.ID <= 0 || entity.ID >= _generations.Count) return false;
            return _generations[entity.ID] == entity.Generation;
        }

        public IReadOnlyList<Entity> GetEntities()
        {
            return _entities;
        }

        public IEnumerable<Entity> GetEntitiesWithComponent<T>() where T : class, IComponent
        {
            foreach (var entity in _entities)
            {
                if (entity.HasComponent<T>())
                {
                    yield return entity;
                }
            }
        }
    }
}