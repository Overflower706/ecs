using System;
using UnityEngine;

namespace OVFL.ECS
{
    public static class EventExtensions
    {
        internal static Entity CreateEvent<T>(this Context context, T eventComponent, bool isFixed = false) where T : EventComponent
        {
            var entity = context.CreateEntity();
            entity.AddComponent(eventComponent);
            entity.AddComponent(new EventMetadataComponent
            {
                CreatedTime = Time.time,
                EventTypeName = typeof(T).Name,
                IsFixed = isFixed,
#if UNITY_EDITOR
                StackTrace = Environment.StackTrace
#endif
            });
            return entity;
        }

        /// <summary>
        /// 모든 T 타입 Event를 처리합니다. 여러 System이 동일 Event를 처리할 수 있습니다.
        /// </summary>
        public static void ProcessEvents<T>(this Context context, Action<Entity, T> action) where T : class, IComponent
        {
            foreach (var entity in context.AllEntities)
            {
                if (entity.TryGetComponent<T>(out var eventComponent))
                    action(entity, eventComponent);
            }
        }

        /// <summary>
        /// 조건에 맞는 Event만 처리합니다.
        /// </summary>
        public static void ProcessEventsWhere<T>(this Context context, Func<T, bool> predicate, Action<Entity, T> action) where T : class, IComponent
        {
            foreach (var entity in context.AllEntities)
            {
                if (entity.TryGetComponent<T>(out var eventComponent) && predicate(eventComponent))
                    action(entity, eventComponent);
            }
        }
    }
}
