using System;
using System.Collections.Generic;

namespace OVFL.ECS
{
    public class Entity
    {
        private readonly Dictionary<Type, IComponent> _components = new();
        public int ID { get; private set; }

        // 컴포넌트 변경 이벤트
        public event Action<Entity, Type> OnComponentAdded;
        public event Action<Entity, Type> OnComponentRemoved;

        public Entity(int id)
        {
            ID = id;
        }

        public T AddComponent<T>(T component) where T : class, IComponent
        {
            var componentType = component.GetType();
            _components[componentType] = component;
            OnComponentAdded?.Invoke(this, componentType);
            return component;
        }

        public IComponent AddComponent(IComponent component)
        {
            if (component == null) return null;

            var componentType = component.GetType();
            _components[componentType] = component;
            OnComponentAdded?.Invoke(this, componentType);
            return component;
        }

        public T AddComponent<T>() where T : class, IComponent, new()
        {
            var component = new T();
            return AddComponent(component);
        }

        public T GetComponent<T>() where T : class, IComponent
        {
            _components.TryGetValue(typeof(T), out var component);
            return component as T;
        }

        public bool HasComponent<T>() where T : class, IComponent
        {
            return _components.ContainsKey(typeof(T));
        }

        public bool RemoveComponent<T>() where T : class, IComponent
        {
            var componentType = typeof(T);
            if (_components.Remove(componentType))
            {
                OnComponentRemoved?.Invoke(this, componentType);
                return true;
            }
            return false;
        }
    }
}
