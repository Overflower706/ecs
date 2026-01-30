using System;
using System.Collections.Generic;

namespace OVFL.ECS
{
    public class Context
    {
        private int[] _entityIndices = new int[1024]; // Sparse 배열
        private readonly List<Entity> _entities = new(); // Dense 배열
        public IEnumerable<Entity> AllEntities => _entities;

        private readonly Queue<int> _availableIDs = new();
        private readonly List<int> _generations = new();
        private int _nextEntityID = 0;

        public Context()
        {
            Array.Fill(_entityIndices, -1);
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

                if (id >= _entityIndices.Length)
                {
                    int oldSize = _entityIndices.Length;
                    Array.Resize(ref _entityIndices, oldSize * 2);
                    // 영역 확장시 -1로 초기화
                    for (int i = oldSize; i < _entityIndices.Length; i++)
                        _entityIndices[i] = -1;
                }
            }

            var entity = new Entity(id, generation);
            _entities.Add(entity);
            _entityIndices[id] = _entities.Count - 1;

            return entity;
        }

        public bool DestroyEntity(Entity entity)
        {
            if (!IsAlive(entity)) return false;

            int idToRemove = entity.ID;
            int indexToRemove = _entityIndices[idToRemove];
            int lastIndex = _entities.Count - 1;

            // Swap & Pop: 맨 뒤의 엔티티를 삭제할 칸으로 이동
            if (indexToRemove != lastIndex)
            {
                Entity lastEntity = _entities[lastIndex];
                _entities[indexToRemove] = lastEntity;
                _entityIndices[lastEntity.ID] = indexToRemove;
            }

            _entities.RemoveAt(lastIndex);
            _entityIndices[idToRemove] = -1; // 이제 0번 ID라도 여기서 -1로 밀림

            _generations[idToRemove]++;
            _availableIDs.Enqueue(idToRemove);
            entity.IsActive = false;

            return true;
        }

        public Entity GetEntity(int id)
        {
            // 0번 ID도 유효하므로 id >= 0 체크
            if (id < 0 || id >= _generations.Count) return null;

            int index = _entityIndices[id];
            if (index == -1) return null;

            Entity entity = _entities[index];
            return (entity.Generation == _generations[id]) ? entity : null;
        }

        // TODO : v1.6.0에서 제거
        [Obsolete("GetEntitiesWithComponent는 더 이상 사용되지 않습니다.")]
        public List<Entity> GetEntitiesWithComponent<T>() where T : class, IComponent
        {
            var result = new List<Entity>();
            foreach (var entity in _entities)
            {
                if (entity.HasComponent<T>())
                {
                    result.Add(entity);
                }
            }
            return result;
        }

        // TODO : v1.6.0에서 제거
        [Obsolete("GetEntities는 더 이상 사용되지 않습니다. 대신 AllEntities를 사용하세요.")]
        public List<Entity> GetEntities()
        {
            return new List<Entity>(_entities);
        }

        public bool IsAlive(Entity entity)
        {
            if (entity == null) return false;
            if (entity.ID < 0 || entity.ID >= _generations.Count) return false;
            return _generations[entity.ID] == entity.Generation;
        }
    }
}