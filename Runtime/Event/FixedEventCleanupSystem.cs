namespace OVFL.ECS
{
    /// <summary>
    /// FixedUpdate 프레임 후미에 모든 FixedEvent Entity를 삭제합니다. 시스템 목록 맨 뒤에 등록해야 합니다.
    /// </summary>
    public class FixedEventCleanupSystem : IFixedCleanupSystem
    {
        public Context Context { get; set; }

        public void FixedCleanup()
        {
            foreach (var entity in Context.AllEntities)
            {
                if (entity.TryGetComponent<EventMetadataComponent>(out var meta) && meta.IsFixed)
                    Context.DestroyEntity(entity);
            }
        }
    }
}
