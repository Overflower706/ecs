# TODO

## 다음 버전 (v1.6.0)

### Breaking Changes
- [ ] `Systems()` 기본 생성자 제거
- [ ] `Systems.SetContext()` 제거
- [ ] `Context.GetEntities()` 제거
- [ ] `Context.GetEntitiesWithComponent<T>()` 제거

## 버그 / 설계 개선

- [ ] `AddComponent<T>(T component)` — 런타임 타입(`component.GetType()`)으로 저장 시 기반 타입으로 조회 불가 문제 검토
- [ ] `Entity.Null` 생성 시 `IsActive = true`로 초기화되는 의미 불일치 검토

## 테스트 보완

- [ ] `TryGetComponent` 성공/실패 케이스
- [ ] `AddComponent` 같은 타입 재등록(덮어쓰기) 케이스
- [ ] `Entity.Null` / `IsNull` 케이스
- [ ] `ICleanupSystem` 라이프사이클
- [ ] `IFixedTickSystem` 라이프사이클
- [ ] `Context.GetEntity` null 반환 케이스
