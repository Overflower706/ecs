using System;
using System.Collections.Generic;

namespace OVFL.ECS
{
    public class Entity
    {
        private readonly Dictionary<Type, IComponent> _components = new();
        public int ID { get; private set; }

        public Entity(int id)
        {
            ID = id;
        }

        public T AddComponent<T>(T component) where T : class, IComponent
        {
            _components[typeof(T)] = component;
            return component;
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
            return _components.Remove(typeof(T));
        }
    }
}