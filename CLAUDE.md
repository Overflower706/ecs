# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 언어

한국어로 답해줘.

## 프로젝트 철학

Unity용 ECS 패키지. 기존 ECS 구현체(Sparse Set, Archetype 등)가 데이터 지향 최적화에 집중하느라 학습 곡선이 급격한 문제를 해결하기 위해 만들어짐.

**핵심 목표: ECS의 유지보수성(데이터와 기능의 분리)을 현업에서 챙기되, 학습 곡선은 완만하게 유지**

이 목표를 위해 내린 의도적 설계 결정들:
- `Entity`를 struct 대신 **class**로 구현 — 컴포넌트를 담는 그릇이라는 개념을 코드 레벨에서 드러냄
- `IComponent`도 struct 강제 없음 — 메모리 최적화보다 유지보수성 우선
- Query 시스템 미제공 — `Context`가 Entity 배열을 가지고 System이 그것을 순회한다는 구조를 숨기지 않기 위함
- 이벤트 시스템 제공 — `EventComponent` + `EventQueueComponent` + Publisher/Cleanup System 쌍. Unity DOTS의 EntityCommandBuffer 패턴을 싱글스레드 환경에 맞게 재해석한 구조

새 기능을 추가하거나 변경할 때 이 철학에 반하는 방향(복잡한 추상화, 과도한 최적화 API)은 피할 것.

## 작업 흐름

작업을 시작하기 전에 반드시 해당 버전에서 구현할 내용을 확정한다. 구현 완료 후에는 다음 순서로 마무리한다:

1. `package.json`의 `version` 필드 업데이트
2. `CHANGELOG.md` 업데이트
3. 변경사항 커밋 및 버전 태그 생성 후 push

다음 작업은 push가 완료된 이후에 시작한다.

## 테스트 실행

Unity Test Runner(Edit Mode) 사용. Unity Editor에서만 실행 가능:
1. **Window > General > Test Runner** 열기
2. **EditMode** 탭 → **Run All** 또는 개별 테스트 실행

테스트 파일: `Tests/Editor/`

## 아키텍처

### 구성 요소 관계

```
Systems
  └── AddSystem(ISystem) → Context 자동 주입
        └── ISetupSystem / ITickSystem / ICleanupSystem / IFixedTickSystem / ITeardownSystem

Context
  ├── CreateEntity() / DestroyEntity(entity)
  ├── AllEntities (IEnumerable<Entity>)
  └── GetEntity(id) / IsAlive(entity)

Entity
  ├── ID, Generation, IsActive
  └── AddComponent / GetComponent<T> / TryGetComponent<T> / HasComponent<T> / RemoveComponent<T>
```

### Context 내부 구조

Sparse Set 패턴으로 구현:
- `_entityIndices`: Sparse 배열 (ID → Dense 인덱스), 초기 1024, 초과 시 2배 확장
- `_entities`: Dense 배열 (실제 Entity 목록)
- Entity 삭제 시 Swap & Pop으로 Dense 배열 연속성 유지
- ID 재사용: `_availableIDs` Queue로 관리, Generation으로 오인 방지
- `IsAlive(entity)`: Generation 비교로 유효성 검사

### Systems 라이프사이클

하나의 System이 여러 인터페이스를 동시 구현 가능. `AddSystem()` 시 자동으로 각 리스트에 분류됨.

| 인터페이스 | 호출 메서드 | Unity 대응 |
|---|---|---|
| `ISetupSystem` | `systems.Setup()` | `Start()` |
| `ITickSystem` | `systems.Tick()` | `Update()` |
| `ICleanupSystem` | `systems.Cleanup()` | `Update()` 직후 |
| `IFixedTickSystem` | `systems.FixedTick()` | `FixedUpdate()` |
| `ITeardownSystem` | `systems.Teardown()` | `OnDestroy()` |

`Teardown()` 호출 시 `UnregisterAll()`이 자동 실행됨.

모든 `ISystem` 구현체는 `Context Context { get; set; }` 프로퍼티를 가져야 하며, `AddSystem()` 시점에 자동 주입됨.

### Deprecated API (v1.6.0 제거 예정)

- `Systems()` 기본 생성자 → `Systems(Context context)` 사용
- `SetContext(context)` → 생성자로 전달
- `Context.GetEntities()` → `Context.AllEntities` 사용
- `Context.GetEntitiesWithComponent<T>()` → `AllEntities` 직접 순회로 대체
