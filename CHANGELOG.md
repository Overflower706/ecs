# Changelog

All notable changes to this project will be documented in this file.

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
