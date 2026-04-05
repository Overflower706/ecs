# OVFL ECS (OVFL Entity Component System)

ECS의 장점은 여러 가지가 있습니다.<br>
메모리 최적화로 캐싱에 유리하기도 하고 데이터와 기능이 분리돼 있어 유지보수에도 이점이 있죠.<br>
다만 메모리 최적화를 위한 구현 과정에서 학습 곡선이 급격히 오른다는 문제가 있다고 생각합니다.<br>
아직 ECS에 익숙하지 않은, 아직은 객체 지향 개발에 익숙한 개발자분이 ECS에 점차 익숙해질 수 있도록 지원하고자 학습용 패키지를 만들었습니다.

## 🎯 프로젝트 목표

- Context가 Entity 배열을 갖고 있고, System이 해당 배열을 순회하면서 Component를 처리해나간다는 것이 선명하게 드러날 것
- Entity는 ID 값만을 갖고 있고 여러 Component를 담는 그릇임이 코드상에서 나타날 것
- 그 결과, 데이터와 기능의 분리가 어떻게 동작되는지 이해하는데에 도움이 됐으면 합니다.

## 📦 설치 방법

### Unity Package Manager로 설치

#### 방법 1: Package Manager 사용
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
       "com.ovfl.ecs": "https://github.com/Overflower706/ecs.git"
     }
   }
   ```

#### 방법 3: 특정 버전 설치
git 주소 끝에 해당 버전을 입력하면 됩니다.
```json
{
  "dependencies": {
    "com.ovfl.ecs": "https://github.com/Overflower706/ecs.git#v1.5.4"
  }
}
```

## 🏗️ 아키텍처

### 핵심 구성 요소

#### 1. **Entity (엔티티)**
```csharp
public class Entity
{
    public readonly int ID;
    public readonly int Generation;
    public bool IsActive { get; internal set; }
    private readonly Dictionary<Type, IComponent> _components;
}

var entity = context.CreateEntity();
entity.AddComponent(new PositionComponent { X = 10, Y = 20 });
entity.AddComponent(new VelocityComponent { VX = 1, VY = 0 });
```

- 고유 ID를 가진 객체
- 컴포넌트들을 담는 컨테이너 역할
- ID는 재사용되며, 이로 인한 오인을 방지하기 위해 Generation과 IsActive가 존재합니다.<br>
ex 1. ID 1을 갖고 있다가 해당 Entity가 삭제됐다면, IsActive를 통해 삭제됐음을 알 수 있음.<br>
ex 2. ID 1을 갖고 있다가 해당 Entity가 삭제되고 새로운 Entity가 생성되면서 ID 1을 할당 받음. 하지만 Generation은 1과 2로 다르기 때문에 둘을 구분할 수 있음.<br>
<br>
※ ECS에서는 Entity를 struct로 하는게 맞습니다만, Entity는 Component를 담고 있는 그릇임을 강조하기 위해 class로 구현하였습니다.

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
- 로직을 포함하지 않습니다.<br>
<br>
※ 마찬가지로 ECS에서는 Component를 struct로 고정하는게 맞습니다.(메모리상 이점을 얻기 위함) 다만 이 프로젝트는 메모리상 이점보단 데이터와 기능 분리를 통한 유지보수에 초점을 맞추고자 더 넓은 폭으로 지원하고자 class로도 사용할 수 있습니다. 익숙해졌다면 이제 실제로 사용하는 Sparse Set ECS 구현체나 Archetype ECS(Unity DOTS가 대표적이죠)로 넘어갑시다.

#### 3. **System (시스템)**
```csharp
public class MovementSystem : ITickSystem
{
    public Context Context { get; set; }

    public void Tick()
    {
        foreach (var entity in Context.AllEntities)
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
- 5가지 라이프사이클 지원:
  - `ISetupSystem`: 초기화 시 한 번 실행
  - `ITickSystem`: 매 프레임 실행
  - `ICleanupSystem`: Tick 이후 정리 작업
  - `IFixedTickSystem`: 일정 주기(Unity의 FixedUpdate)마다 실행
  - `ITeardownSystem`: 마무리 시 한 번 실행

#### 4. **Context (컨텍스트)**
```csharp
var context = new Context();
var entity = context.CreateEntity();
context.DestroyEntity(entity);

// 모든 엔티티 순회
foreach (var e in context.AllEntities) { ... }
```

- 엔티티들을 관리하는 월드
- 엔티티 생성/파괴 담당
- 시스템들이 접근하는 엔티티 컬렉션 제공

#### 5. **Systems (시스템 관리자)**
```csharp
var systems = new Systems(context)
    .AddSystem(new MovementSystem())
    .AddSystem(new RenderSystem());

systems.Setup();    // 초기화
systems.Tick();     // 매 프레임
systems.Cleanup();  // 정리
systems.Teardown(); // 마무리 (자동으로 UnregisterAll 호출)
```

- System을 일괄적으로 관리하기 위한 클래스입니다.

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
    public Context Context { get; set; }

    public void Tick()
    {
        // 일반적인 ECS에서는 이를 쉽게 할 수 있도록 쿼리(Query)를 지원합니다.
        // 다만 그것이 'Context 내 배열로 존재하는 Entity를 순회하며 처리'라는 구조를 가린다고 생각해서 이를 유지합니다.
        foreach (var entity in Context.AllEntities)
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
public class GameECSRunner : MonoBehaviour
{
    private Context _context;
    private Systems _systems;
    
    void Start()
    {
        // 컨텍스트와 시스템 초기화
        _context = new Context();
        _systems = new Systems(_context);

        _systems.AddSystem(new MovementSystem());
        
        // 엔티티 생성
        var player = _context.CreateEntity();
        player.AddComponent(new PositionComponent { X = 0, Y = 0 });
        player.AddComponent(new VelocityComponent { VX = 1, VY = 0 });
        
        // 시스템 초기화
        _systems.Setup();
    }
    
    void Update()
    {
        // 매 프레임 실행
        _systems.Tick();
        _systems.Cleanup();
    }

    void FixedUpdate()
    {
        _systems.FixedTick();
    }
    
    void OnDestroy()
    {
        // 정리
        _systems.Teardown();
    }
}
```

## 🧪 테스트

현재 프로젝트는 기능이 원활히 동작하는 상태를 유지하기 위해 테스트 코드를 포함하고 있습니다.<br>
Unity Test Runner 프레임워크를 사용합니다.

### 테스트 실행
1. Unity Editor에서 **Window > General > Test Runner** 열기
2. **EditMode** 탭 선택
3. **Run All** 또는 개별 테스트 실행

### 테스트 구조
```
Tests/Editor/
├── EntityComponentTests.cs  # 엔티티 및 컴포넌트 기능 테스트
├── ContextTests.cs          # 컨텍스트 기능 테스트
├── SystemsTests.cs          # 시스템 관리 테스트
└── IntegrationTest.cs       # 통합 테스트
```

## 📁 프로젝트 구조

```
Runtime/
├── Component/
│   └── IComponent.cs        # 컴포넌트 인터페이스
├── Entity/
│   └── Entity.cs            # 엔티티 클래스
├── Context/
│   └── Context.cs           # 컨텍스트 클래스
└── System/
    ├── ISystem.cs            # 시스템 인터페이스들
    └── Systems.cs            # 시스템 관리 클래스

Tests/Editor/
├── *.cs                     # 테스트 파일들
```

## 🎮 Unity 통합

### Assembly Definition
- **OVFL.ECS**: 메인 ECS 라이브러리
- **Editor**: 테스트 전용 (Edit Mode)

### 사용 요구사항
- Unity 6000.1 이상
- .NET Standard 2.1

## 📄 라이선스

MIT 라이센스를 적용합니다.

## 🤝 기여

버그 리포트나 개선 제안은 언제든 환영합니다!
