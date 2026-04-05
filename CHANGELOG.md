# Changelog

All notable changes to this project will be documented in this file.

## [1.5.6] - 2026-04-06

### Fixed
- **Entity.AddComponent&lt;T&gt;(T component) 타입 키 불일치 수정** — 런타임 타입(`component.GetType()`) 대신 컴파일 타임 타입(`typeof(T)`)으로 저장하도록 변경. `GetComponent`, `HasComponent`, `RemoveComponent`와 일관성 유지.
- **Entity.Null.IsActive 의미 불일치 수정** — `Entity.Null` 생성 시 `IsActive = false`로 초기화. null 객체가 활성 상태로 표시되던 논리적 모순 해결.

### Tests
- `TryGetComponent` 성공/실패 케이스 추가
- `AddComponent` 같은 타입 재등록(덮어쓰기) 케이스 추가
- `Entity.Null` — `IsNull`, `IsActive` 케이스 추가
- `Context.GetEntity` 잘못된 ID → null 반환 케이스 추가
- `ICleanupSystem` 라이프사이클 케이스 추가
- `IFixedTickSystem` 라이프사이클 케이스 추가

## [1.5.5] - 2026-04-06

### Added
- **Context.FlushDestroyQueue()** 메서드 추가 — 삭제 큐에 쌓인 엔티티를 즉시 일괄 제거
  ```csharp
  context.FlushDestroyQueue(); // Systems 없이 직접 사용 시 수동 호출
  ```

### Changed
- **DestroyEntity() 지연 삭제로 변경** — 호출 즉시 삭제하지 않고 큐에 등록 후 `Tick()` / `FixedTick()` 완료 시 자동 처리
  ```csharp
  // 이제 Tick 내에서 안전하게 호출 가능
  foreach (var entity in Context.AllEntities)
  {
      if (isDead) Context.DestroyEntity(entity); // 예외 없음
  }
  ```
- **AllEntities 필터링** — `IsActive=false`인 엔티티(삭제 예약된 엔티티) 제외
- **IsAlive() 조기 반환** — `IsActive=false`이면 즉시 false 반환
- **Systems.Tick() / Systems.FixedTick()** — 모든 시스템 실행 후 `FlushDestroyQueue()` 자동 호출

## [1.6.0] - 2026-04-06

### Removed (Breaking Changes)
- **Systems() 기본 생성자 제거** — `Systems(Context context)` 생성자를 사용하세요.
  ```csharp
  // Before (더 이상 동작하지 않음)
  var systems = new Systems();
  systems.SetContext(context);
  
  // After
  var systems = new Systems(context);
  ```
- **Systems.SetContext() 제거** — 생성자로 Context를 전달하세요.
- **Context.GetEntities() 제거** — `Context.AllEntities`를 사용하세요.
  ```csharp
  // Before (더 이상 동작하지 않음)
  var entities = context.GetEntities();
  
  // After
  var entities = context.AllEntities;
  ```
- **Context.GetEntitiesWithComponent\<T\>() 제거** — `AllEntities`를 직접 순회하세요.
  ```csharp
  // Before (더 이상 동작하지 않음)
  var players = context.GetEntitiesWithComponent<PlayerComponent>();
  
  // After
  var players = context.AllEntities.Where(e => e.HasComponent<PlayerComponent>());
  ```

## [1.5.4] - 2026-03-10

### Added
- **Systems.UnregisterSystem()** 메서드 추가 — 특정 시스템을 모든 라이프사이클 리스트에서 제거
  ```csharp
  systems.UnregisterSystem(mySystem);
  ```
- **Systems.UnregisterAll()** 메서드 추가 — 등록된 모든 시스템을 일괄 해제
  ```csharp
  systems.UnregisterAll();
  ```
- **Teardown() 자동 Unregister**: `Teardown()` 실행 완료 후 `UnregisterAll()`이 자동으로 호출되어 모든 시스템이 해제됨

## [1.5.0] - 2026-01-30

### Changed
- 0번째 Entity 또는 별도의 null Entity 처리 추가
- Entity Pooling 기능 추가
- Try Get Component를 추가
- Systems에 Context를 받는 생성자가 추가됐습니다. 이제 SetContext 대신 Systems 생성자를 사용해야합니다.
- Reactive 기능 제거

## [1.3.0] - 2025-08-08

### Changed
- **Entity.AddComponent() 세부 구현 변경**: 런타임 타입 기반으로 추적됨
  ```csharp
  // Before - 복잡한 리플렉션 코드 필요
  var methods = typeof(Entity).GetMethods(BindingFlags.Public | BindingFlags.Instance);
  // ...복잡한 리플렉션 로직
  
  // After - 간단한 비제네릭 메서드 사용
  entity.AddComponent(notifyComponent); // IComponent 타입으로 직접 추가
  ```
- **컴포넌트 쿼리 성능 대폭 개선**: 선형 검색에서 해시 기반 캐싱으로 변경
  ```csharp
  // 기존: O(n) - 매번 모든 엔티티 순회
  // 개선: O(1) - 컴포넌트 타입별 엔티티 캐시 활용
  var entities = context.GetEntitiesWithComponent<PlayerComponent>(); // 즉시 반환
  ```
- **Context 클래스 캐싱 시스템**: 컴포넌트 추가/제거 시 자동 캐시 업데이트
  - `Dictionary<Type, HashSet<Entity>>` 기반 캐시 구조
  - Entity의 컴포넌트 변경 이벤트(`OnComponentAdded`, `OnComponentRemoved`)와 연동
  - 엔티티 제거 시 모든 관련 캐시에서 자동 정리
- **시스템 확장성 향상**: 시스템 수 증가 시에도 성능 저하 없음
  - 매 프레임 컴포넌트 검색 비용을 O(시스템 수 × 엔티티 수)에서 O(시스템 수)로 감소
  - NotifySystem 등에서 복잡한 리플렉션 코드 제거로 가독성 및 유지보수성 향상

### Performance Improvements
- **GetEntitiesWithComponent<T>()** 메서드 성능 최적화
- 대용량 엔티티 환경에서 쿼리 성능 대폭 향상 (1000개 엔티티 기준 100회 조회 시 100ms 이내)
- 메모리 사용량 최적화: 중복 검색 제거로 CPU 캐시 효율성 증대

## [1.2.0] - 2025-07-14

### Changed
- **Systems.Add()** → **Systems.AddSystem()** 메서드명 변경 (명확성 향상)
- **Systems.SetContext()** 메서드 추가로 Context 자동 할당 지원
- **ISystem 인터페이스 메서드 시그니처 변경**
  ```csharp
  // Before
  void Setup(Context context);
  void Tick(Context context);
  void Cleanup(Context context);
  void Teardown(Context context);
  
  // After
  void Setup();
  void Tick();
  void Cleanup();
  void Teardown();
  ```
- **ISystem.Context 속성** 추가
  ```csharp
  // 모든 시스템이 Context 속성을 가지며, 시스템 등록 시 자동으로 할당됨
  public class MySystem : ITickSystem
  {
      public Context Context { get; set; }  // 자동 할당
      
      public void Tick()  // Context 파라미터 제거
      {
          // this.Context 사용
          var entities = this.Context.GetEntities();
      }
  }
  ```
- 타입 안정성 향상
- 코드 간결성 개선 (Context 파라미터 제거로 메서드 시그니처 단순화)
- 시스템 등록 시 Context 자동 할당으로 사용성 향상

## [1.1.0] - 2025-07-09

### Added
- **AddComponent<T>()** 제네릭 메서드 추가
  ```csharp
  // Before
  entity.AddComponent(new GridComponent());
  
  // After
  entity.AddComponent<GridComponent>();
  ```
- **GetEntitiesWithComponent<T>()** 메서드 추가
  ```csharp
  // Context에서 특정 Component를 가진 Entity들 조회 (성능 최적화된 for문 사용)
  var gameEntities = context.GetEntitiesWithComponent<GameStateComponent>();
  ```
- **Systems.AddSystem<T>()** 제네릭 메서드 추가
  ```csharp
  // Before
  Systems.AddSystem(new DataSystem());
  
  // After
  Systems.AddSystem<DataSystem>();
  ```

## [1.0.0] - 2025-07-03

### Added
- 초기 ECS 구현체 릴리즈
- **IComponent 인터페이스**: 순수한 데이터 컴포넌트 정의
- **Entity 클래스**: 컴포넌트 관리 (추가, 제거, 조회, 존재 확인)
- **Context 클래스**: 엔티티 생성 및 관리
- **Systems 클래스**: 시스템 등록 및 라이프사이클 관리
- **시스템 라이프사이클 인터페이스들**:
  - `ISetupSystem`: 초기화 시 한 번 실행
  - `ITickSystem`: 매 프레임 실행
  - `ICleanupSystem`: Tick 이후 정리 작업
  - `ITeardownSystem`: 마무리 시 한 번 실행
- **포괄적인 Unit Test 스위트**: 모든 핵심 기능에 대한 테스트
- **통합 테스트**: 실제 사용 시나리오 검증
- **Unity Package Manager 지원**: Git URL을 통한 패키지 설치

### Features
- **순수한 C# 구현**: Unity 의존성 최소화로 재사용성 극대화
- **체이닝 메서드 지원**: `systems.AddSystem(sys1).AddSystem(sys2)` 형태로 편리한 사용
- **타입 안전성 보장**: 제네릭을 활용한 컴파일 타임 타입 검사
- **학습 친화적인 구조**: 복잡한 최적화 없이 ECS 개념에 집중
- **메모리 효율적**: Dictionary 기반 컴포넌트 저장
- **확장 가능한 아키텍처**: 기본 구조 유지하면서 기능 추가 가능

### Technical Details
- Unity 2020.3 이상 지원
- .NET Standard 2.1 호환
- NUnit 기반 테스트 프레임워크
- Edit Mode 테스트 지원
- Assembly Definition 파일 포함
