namespace OVFL.ECS
{
    /// <summary>
    /// 대기 중인 FixedEvent를 Entity로 변환해 발행합니다. 시스템 목록 맨 앞에 등록해야 합니다.
    /// </summary>
    public class FixedEventPublisherSystem : IFixedTickSystem
    {
        public Context Context { get; set; }

        public void FixedTick()
        {
            while (Context.PendingFixedEvents.Count > 0)
                Context.PendingFixedEvents.Dequeue().Invoke();
        }
    }
}
