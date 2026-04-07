using System;
using System.Collections.Generic;

namespace OVFL.ECS
{
    public class Entity : IEquatable<Entity>
    {
        public readonly int ID;
        public readonly int Generation;
        public bool IsActive { get; internal set; }
        private readonly Dictionary<Type, IComponent> _components = new();

        public Entity(int id, int generation)
        {
            ID = id;
            Generation = generation;
            IsActive = true;
        }

        public T AddComponent<T>(T component) where T : class, IComponent
        {
            _components[typeof(T)] = component;
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

        public bool TryGetComponent<T>(out T component) where T : class, IComponent
        {
            component = GetComponent<T>();
            return component != null;
        }

        public bool HasComponent<T>() where T : class, IComponent
        {
            return _components.ContainsKey(typeof(T));
        }

        public void RemoveComponent<T>() where T : class, IComponent
        {
            _components.Remove(typeof(T));
        }

        public static readonly Entity Null = new Entity(-1, 0) { IsActive = false };
        public bool IsNull => ID < 0;
        public bool Equals(Entity other)
        {
            if (other is null) return false;

            if (ReferenceEquals(this, other)) return true;

            return ID == other.ID && Generation == other.Generation;
        }
        public override bool Equals(object obj) => obj is Entity other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(ID, Generation);
        public static bool operator ==(Entity left, Entity right)
        {
            if (left is null) return right is null;

            return left.Equals(right);
        }
        public static bool operator !=(Entity left, Entity right) => !(left == right);
        public override string ToString() => $"Entity({ID}:{Generation})";
    }
}
