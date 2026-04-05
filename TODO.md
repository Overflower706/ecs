# TODO

## 다음 버전 (v1.6.0)

### Breaking Changes
- [x] `Systems()` 기본 생성자 제거 *(v1.6.0)*
- [x] `Systems.SetContext()` 제거 *(v1.6.0)*
- [x] `Context.GetEntities()` 제거 *(v1.6.0)*
- [x] `Context.GetEntitiesWithComponent<T>()` 제거 *(v1.6.0)*

## 버그 / 설계 개선

- [x] `AddComponent<T>(T component)` — 런타임 타입(`component.GetType()`)으로 저장 시 기반 타입으로 조회 불가 문제 검토 *(v1.5.6: typeof(T)로 수정)*
- [x] `Entity.Null` 생성 시 `IsActive = true`로 초기화되는 의미 불일치 검토 *(v1.5.6: IsActive = false로 수정)*

## 테스트 보완

- [x] `TryGetComponent` 성공/실패 케이스 *(v1.5.6)*
- [x] `AddComponent` 같은 타입 재등록(덮어쓰기) 케이스 *(v1.5.6)*
- [x] `Entity.Null` / `IsNull` 케이스 *(v1.5.6)*
- [x] `ICleanupSystem` 라이프사이클 *(v1.5.6)*
- [x] `IFixedTickSystem` 라이프사이클 *(v1.5.6)*
- [x] `Context.GetEntity` null 반환 케이스 *(v1.5.6)*
