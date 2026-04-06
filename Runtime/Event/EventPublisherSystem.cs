namespace OVFL.ECS
{
    /// <summary>
    /// 대기 중인 Event를 Entity로 변환해 발행합니다. 시스템 목록 맨 앞에 등록해야 합니다.
    /// </summary>
    public class EventPublisherSystem : ITickSystem
    {
        public Context Context { get; set; }

        public void Tick()
        {
            while (Context.PendingEvents.Count > 0)
                Context.PendingEvents.Dequeue().Invoke();
        }
    }
}
