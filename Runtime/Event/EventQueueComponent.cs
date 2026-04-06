using System.Collections.Generic;

namespace OVFL.ECS
{
    public class EventQueueComponent : IComponent
    {
        public Queue<EventComponent> PendingEvents { get; } = new Queue<EventComponent>();

        public void Enqueue(EventComponent eventComponent)
        {
            PendingEvents.Enqueue(eventComponent);
        }
    }
}
