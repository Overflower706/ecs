using System;
using System.Collections.Generic;

namespace OVFL.ECS
{
    public class Entity : IEquatable<Entity>
    {
        private readonly Dictionary<Type, IComponent> _components = new();
        public readonly int ID;
        public readonly int Generation;

        public Entity(int id, int generation)
        {
            ID = id;
            Generation = generation;
        }

        public T AddComponent<T>(T component) where T : class, IComponent
        {
            var componentType = component.GetType();
            _components[componentType] = component;
            return component;
        }

        public IComponent AddComponent(IComponent component)
        {
            if (component == null) return null;

            var componentType = component.GetType();
            _components[componentType] = component;
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
                return true;
            }
            return false;
        }

        public static readonly Entity Null = new Entity(0, 0);
        public bool IsNull => ID == 0 && Generation == 0;
        public bool Equals(Entity other) => ID == other.ID && Generation == other.Generation;
        public override bool Equals(object obj) => obj is Entity other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(ID, Generation);
        public static bool operator ==(Entity left, Entity right) => left.Equals(right);
        public static bool operator !=(Entity left, Entity right) => !left.Equals(right);
        public override string ToString() => $"Entity({ID}:{Generation})";
    }
}
