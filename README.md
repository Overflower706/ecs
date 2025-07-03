# OVFL ECS (Entity Component System)

순수하고 학습하기 쉬운 커스텀 ECS 구현체입니다.

## 🎯 프로젝트 목표

- **재사용 가능한 커스텀 ECS**: 다른 프로젝트에서도 활용할 수 있는 순수한 형태의 ECS 구현
- **학습 친화적 구조**: 개념 이해와 학습이 쉬운 명확한 ECS 아키텍처

## 📦 설치 방법

### Unity Package Manager로 설치

#### 방법 1: Package Manager UI 사용
1. Unity에서 **Window > Package Manager** 열기
2. 좌상단 **+** 버튼 클릭
3. **Add package from git URL...** 선택
4. 다음 URL 입력:
   ```
   https://github.com/Overflower706/ecs.git
   ```

#### 방법 2: manifest.json 직접 편집
1. 프로젝트의 `Packages/manifest.json` 파일 열기
2. dependencies 섹션에 추가:
   ```json
   {
     "dependencies": {
       "com.ovfl.ecs": "https://github.com/Overflower706/ecs.git#v1.0.0"
     }
   }
   ```

#### 방법 3: 특정 버전 설치
```json
{
  "dependencies": {
    "com.ovfl.ecs": "https://github.com/Overflower706/ecs.git#v1.0.0"
  }
}
```

### 요구사항
- Unity 2020.3 LTS 이상
- .NET Standard 2.1 호환

## 🏗️ 아키텍처

### 핵심 구성 요소

#### 1. **Entity (엔티티)**
```csharp
var entity = context.CreateEntity();
entity.AddComponent(new PositionComponent(10, 20));
entity.AddComponent(new VelocityComponent(1, 0));
```

- 고유 ID를 가진 게임 객체
- 컴포넌트들을 담는 컨테이너 역할
- 컴포넌트 추가, 제거, 조회 기능

#### 2. **Component (컴포넌트)**
```csharp
public class PositionComponent : IComponent
{
    public float X { get; set; }
    public float Y { get; set; }
}
```

- `IComponent` 인터페이스 구현
- 순수한 데이터 구조
- 로직을 포함하지 않음

#### 3. **System (시스템)**
```csharp
public class MovementSystem : ITickSystem
{
    public void Tick(Context context)
    {
        foreach (var entity in context.GetEntities())
        {
            var position = entity.GetComponent<PositionComponent>();
            var velocity = entity.GetComponent<VelocityComponent>();
            
            if (position != null && velocity != null)
            {
                position.X += velocity.VX;
                position.Y += velocity.VY;
            }
        }
    }
}
```

- 엔티티와 컴포넌트를 처리하는 로직
- 4가지 라이프사이클 지원:
  - `ISetupSystem`: 초기화 시 한 번 실행
  - `ITickSystem`: 매 프레임 실행
  - `ICleanupSystem`: Tick 이후 정리 작업
  - `ITeardownSystem`: 마무리 시 한 번 실행

#### 4. **Context (컨텍스트)**
```csharp
var context = new Context();
var entity = context.CreateEntity();
context.DestroyEntity(entity);
```

- 엔티티들을 관리하는 월드
- 엔티티 생성/파괴 담당
- 시스템들이 접근하는 엔티티 컬렉션 제공

#### 5. **Systems (시스템 관리자)**
```csharp
var systems = new Systems()
    .Add(new MovementSystem())
    .Add(new RenderSystem());

systems.Setup(context);    // 초기화
systems.Tick(context);     // 매 프레임
systems.Cleanup(context);  // 정리
systems.Teardown(context); // 마무리
```

## 🚀 사용 예제

### 기본 사용법

```csharp
using OVFL.ECS;

// 1. 컴포넌트 정의
public class PositionComponent : IComponent
{
    public float X { get; set; }
    public float Y { get; set; }
}

public class VelocityComponent : IComponent
{
    public float VX { get; set; }
    public float VY { get; set; }
}

// 2. 시스템 정의
public class MovementSystem : ITickSystem
{
    public void Tick(Context context)
    {
        foreach (var entity in context.GetEntities())
        {
            var position = entity.GetComponent<PositionComponent>();
            var velocity = entity.GetComponent<VelocityComponent>();
            
            if (position != null && velocity != null)
            {
                position.X += velocity.VX;
                position.Y += velocity.VY;
            }
        }
    }
}

// 3. ECS 설정 및 실행
public class GameMain
{
    private Context _context;
    private Systems _systems;
    
    void Start()
    {
        // 컨텍스트와 시스템 초기화
        _context = new Context();
        _systems = new Systems()
            .Add(new MovementSystem());
        
        // 엔티티 생성
        var player = _context.CreateEntity();
        player.AddComponent(new PositionComponent { X = 0, Y = 0 });
        player.AddComponent(new VelocityComponent { VX = 1, VY = 0 });
        
        // 시스템 초기화
        _systems.Setup(_context);
    }
    
    void Update()
    {
        // 매 프레임 실행
        _systems.Tick(_context);
        _systems.Cleanup(_context);
    }
    
    void OnDestroy()
    {
        // 정리
        _systems.Teardown(_context);
    }
}
```

### 고급 사용법

```csharp
// 복합 시스템 (여러 인터페이스 구현)
public class HealthSystem : ISetupSystem, ITickSystem, ICleanupSystem
{
    public void Setup(Context context)
    {
        // 초기화 로직
    }
    
    public void Tick(Context context)
    {
        // 매 프레임 체력 관련 로직
    }
    
    public void Cleanup(Context context)
    {
        // 체력이 0인 엔티티 정리
    }
}

// 컴포넌트 동적 추가/제거
var entity = context.CreateEntity();
entity.AddComponent(new HealthComponent { CurrentHealth = 100 });

if (entity.HasComponent<HealthComponent>())
{
    var health = entity.GetComponent<HealthComponent>();
    health.CurrentHealth -= 10;
}

if (health.CurrentHealth <= 0)
{
    entity.RemoveComponent<HealthComponent>();
    entity.AddComponent(new DeadComponent());
}
```

## 🧪 테스트

프로젝트는 Unity Test Runner를 사용한 포괄적인 테스트 스위트를 포함합니다.

### 테스트 실행
1. Unity Editor에서 **Window > General > Test Runner** 열기
2. **EditMode** 탭 선택
3. **Run All** 또는 개별 테스트 실행

### 테스트 구조
```
Assets/Tests/Editor/
├── IComponentTests.cs        # 컴포넌트 인터페이스 테스트
├── EntityTests.cs           # 엔티티 기능 테스트
├── ContextTests.cs          # 컨텍스트 기능 테스트
├── SystemsTests.cs          # 시스템 관리 테스트
└── ECSIntegrationTests.cs   # 통합 테스트
```

## 📁 프로젝트 구조

```
Assets/Scripts/
├── Component/
│   └── IComponent.cs        # 컴포넌트 인터페이스
├── Entity/
│   └── Entity.cs           # 엔티티 클래스
├── Context/
│   └── Context.cs          # 컨텍스트 클래스
├── System/
│   ├── Interfaces/
│   │   └── ISystem.cs      # 시스템 인터페이스들
│   └── Systems.cs          # 시스템 관리 클래스
└── OVFL.ECS.asmdef         # 어셈블리 정의

Assets/Tests/Editor/
├── *.cs                    # 테스트 파일들
└── Editor.asmdef          # 테스트 어셈블리 정의
```

## 🎮 Unity 통합

### Assembly Definition
- **OVFL.ECS**: 메인 ECS 라이브러리
- **Editor**: 테스트 전용 (Edit Mode)

### 사용 요구사항
- Unity 2022.3 LTS 이상
- .NET Standard 2.1

## 🔧 확장 가능성

### 향후 개선 가능한 기능들
- **그룹 시스템**: 특정 컴포넌트 조합을 가진 엔티티 그룹화
- **이벤트 시스템**: 컴포넌트 추가/제거 이벤트
- **쿼리 시스템**: 복잡한 엔티티 검색
- **성능 최적화**: 메모리 풀링, 캐싱

### 현재 설계의 장점
- **단순함**: 복잡한 최적화 없이 ECS 개념에 집중
- **명확성**: 각 구성 요소의 역할이 명확
- **확장성**: 기본 구조를 유지하면서 기능 추가 가능

## 📄 라이선스

이 프로젝트는 학습 및 연구 목적으로 제작되었습니다.

## 🤝 기여

버그 리포트나 개선 제안은 언제든 환영합니다!
