using NUnit.Framework;
using OVFL.ECS;
using System.Linq;

namespace Test.OVFL.ECS
{
    /// <summary>
    /// 전체 ECS 시스템의 통합 테스트
    /// 실제 사용 시나리오를 검증합니다
    /// </summary>
    [TestFixture]
    public class ECSIntegrationTests
    {
        // 테스트용 컴포넌트들
        private class PositionComponent : IComponent
        {
            public float X { get; set; }
            public float Y { get; set; }

            public PositionComponent(float x = 0, float y = 0)
            {
                X = x;
                Y = y;
            }
        }

        private class VelocityComponent : IComponent
        {
            public float VX { get; set; }
            public float VY { get; set; }

            public VelocityComponent(float vx = 0, float vy = 0)
            {
                VX = vx;
                VY = vy;
            }
        }

        private class HealthComponent : IComponent
        {
            public int CurrentHealth { get; set; }
            public int MaxHealth { get; set; }

            public HealthComponent(int maxHealth = 100)
            {
                MaxHealth = maxHealth;
                CurrentHealth = maxHealth;
            }
        }

        private class NameComponent : IComponent
        {
            public string Name { get; set; }

            public NameComponent(string name = "Entity")
            {
                Name = name;
            }
        }

        // 테스트용 시스템들
        private class MovementSystem : ITickSystem
        {
            public Context Context { get; set; }
            public int ProcessedEntityCount { get; private set; }

            public void Tick(Context context)
            {
                ProcessedEntityCount = 0;
                var entities = context.GetEntities();

                foreach (var entity in entities)
                {
                    var position = entity.GetComponent<PositionComponent>();
                    var velocity = entity.GetComponent<VelocityComponent>();

                    if (position != null && velocity != null)
                    {
                        position.X += velocity.VX;
                        position.Y += velocity.VY;
                        ProcessedEntityCount++;
                    }
                }
            }
        }

        private class HealthRegenSystem : ITickSystem
        {
            public Context Context { get; set; }
            public int ProcessedEntityCount { get; private set; }

            public void Tick(Context context)
            {
                ProcessedEntityCount = 0;
                var entities = context.GetEntities();

                foreach (var entity in entities)
                {
                    var health = entity.GetComponent<HealthComponent>();
                    if (health != null && health.CurrentHealth < health.MaxHealth)
                    {
                        health.CurrentHealth = System.Math.Min(health.MaxHealth, health.CurrentHealth + 1);
                        ProcessedEntityCount++;
                    }
                }
            }
        }

        private class InitializationSystem : ISetupSystem
        {
            public Context Context { get; set; }
            public bool WasInitialized { get; private set; }
            public int InitializedEntityCount { get; private set; }

            public void Setup(Context context)
            {
                WasInitialized = true;
                InitializedEntityCount = context.GetEntities().Count;
            }
        }

        private class CleanupSystem : ICleanupSystem
        {
            public Context Context { get; set; }
            public int CleanupCallCount { get; private set; }

            public void Cleanup(Context context)
            {
                CleanupCallCount++;
                // 체력이 0 이하인 엔티티들을 찾아서 표시 (실제로는 제거할 수 있음)
                var entities = context.GetEntities();
                foreach (var entity in entities)
                {
                    var health = entity.GetComponent<HealthComponent>();
                    if (health != null && health.CurrentHealth <= 0)
                    {
                        // 실제 게임에서는 여기서 엔티티를 제거하거나 DeadComponent를 추가할 수 있음
                    }
                }
            }
        }

        private Context _context;
        private Systems _systems;

        [SetUp]
        public void SetUp()
        {
            _context = new Context();
            _systems = new Systems();
            _systems.SetContext(_context);
        }

        [Test]
        public void FullECSWorkflow_CreateEntityWithComponents()
        {
            // Arrange & Act
            var player = _context.CreateEntity();
            player.AddComponent(new PositionComponent(10, 20));
            player.AddComponent(new VelocityComponent(1, 0));
            player.AddComponent(new HealthComponent(100));
            player.AddComponent(new NameComponent("Player"));

            // Assert
            Assert.IsTrue(player.HasComponent<PositionComponent>(), "플레이어는 Position 컴포넌트를 가져야 합니다");
            Assert.IsTrue(player.HasComponent<VelocityComponent>(), "플레이어는 Velocity 컴포넌트를 가져야 합니다");
            Assert.IsTrue(player.HasComponent<HealthComponent>(), "플레이어는 Health 컴포넌트를 가져야 합니다");
            Assert.IsTrue(player.HasComponent<NameComponent>(), "플레이어는 Name 컴포넌트를 가져야 합니다");

            var position = player.GetComponent<PositionComponent>();
            var name = player.GetComponent<NameComponent>();
            Assert.AreEqual(10, position.X, "플레이어 X 위치가 올바르게 설정되어야 합니다");
            Assert.AreEqual("Player", name.Name, "플레이어 이름이 올바르게 설정되어야 합니다");
        }

        [Test]
        public void MovementSystem_ShouldUpdatePosition()
        {
            // Arrange
            var entity = _context.CreateEntity();
            entity.AddComponent(new PositionComponent(0, 0));
            entity.AddComponent(new VelocityComponent(2, 3));

            var movementSystem = new MovementSystem();
            _systems.AddSystem(movementSystem);

            // Act
            _systems.Tick(_context);

            // Assert
            var position = entity.GetComponent<PositionComponent>();
            Assert.AreEqual(2, position.X, "X 위치가 velocity만큼 업데이트되어야 합니다");
            Assert.AreEqual(3, position.Y, "Y 위치가 velocity만큼 업데이트되어야 합니다");
            Assert.AreEqual(1, movementSystem.ProcessedEntityCount, "하나의 엔티티가 처리되어야 합니다");
        }

        [Test]
        public void MovementSystem_ShouldIgnoreEntitiesWithoutRequiredComponents()
        {
            // Arrange
            var entityWithPosition = _context.CreateEntity();
            entityWithPosition.AddComponent(new PositionComponent(0, 0));

            var entityWithVelocity = _context.CreateEntity();
            entityWithVelocity.AddComponent(new VelocityComponent(1, 1));

            var entityWithBoth = _context.CreateEntity();
            entityWithBoth.AddComponent(new PositionComponent(5, 5));
            entityWithBoth.AddComponent(new VelocityComponent(2, 2));

            var movementSystem = new MovementSystem();
            _systems.AddSystem(movementSystem);

            // Act
            _systems.Tick(_context);

            // Assert
            Assert.AreEqual(1, movementSystem.ProcessedEntityCount, "필요한 컴포넌트를 모두 가진 엔티티만 처리되어야 합니다");

            var processedPosition = entityWithBoth.GetComponent<PositionComponent>();
            Assert.AreEqual(7, processedPosition.X, "처리된 엔티티의 위치가 업데이트되어야 합니다");
            Assert.AreEqual(7, processedPosition.Y, "처리된 엔티티의 위치가 업데이트되어야 합니다");
        }

        [Test]
        public void MultipleSystemsWorkflow_ShouldProcessDifferentAspects()
        {
            // Arrange
            var entity = _context.CreateEntity();
            entity.AddComponent(new PositionComponent(0, 0));
            entity.AddComponent(new VelocityComponent(1, 1));
            var healthComponent = new HealthComponent(100); // 최대 체력 100
            healthComponent.CurrentHealth = 95; // 현재 체력을 95로 설정
            entity.AddComponent(healthComponent);

            var movementSystem = new MovementSystem();
            var healthRegenSystem = new HealthRegenSystem();

            _systems.AddSystem(movementSystem).AddSystem(healthRegenSystem);

            // Act
            _systems.Tick(_context);

            // Assert
            var position = entity.GetComponent<PositionComponent>();
            var health = entity.GetComponent<HealthComponent>();

            Assert.AreEqual(1, position.X, "위치가 이동 시스템에 의해 업데이트되어야 합니다");
            Assert.AreEqual(1, position.Y, "위치가 이동 시스템에 의해 업데이트되어야 합니다");
            Assert.AreEqual(96, health.CurrentHealth, "체력이 재생 시스템에 의해 업데이트되어야 합니다");

            Assert.AreEqual(1, movementSystem.ProcessedEntityCount, "이동 시스템이 엔티티를 처리해야 합니다");
            Assert.AreEqual(1, healthRegenSystem.ProcessedEntityCount, "체력 재생 시스템이 엔티티를 처리해야 합니다");
        }

        [Test]
        public void FullLifecycleWorkflow_WithMultipleEntitiesAndSystems()
        {
            // Arrange
            var initSystem = new InitializationSystem();
            var movementSystem = new MovementSystem();
            var cleanupSystem = new CleanupSystem();

            _systems.AddSystem(initSystem)
                   .AddSystem(movementSystem)
                   .AddSystem(cleanupSystem);

            // 여러 엔티티 생성
            var player = _context.CreateEntity();
            player.AddComponent(new PositionComponent(0, 0));
            player.AddComponent(new VelocityComponent(1, 0));
            player.AddComponent(new HealthComponent(100));

            var enemy = _context.CreateEntity();
            enemy.AddComponent(new PositionComponent(10, 10));
            enemy.AddComponent(new VelocityComponent(-1, 0));
            enemy.AddComponent(new HealthComponent(50));

            var staticObject = _context.CreateEntity();
            staticObject.AddComponent(new PositionComponent(5, 5));
            // Velocity 없음 - 이동하지 않아야 함

            // Act - 전체 라이프사이클 실행
            _systems.Setup(_context);
            _systems.Tick(_context);
            _systems.Cleanup(_context);

            // Assert
            Assert.IsTrue(initSystem.WasInitialized, "초기화 시스템이 실행되어야 합니다");
            Assert.AreEqual(3, initSystem.InitializedEntityCount, "초기화 시 3개의 엔티티가 존재해야 합니다");

            // 이동 가능한 엔티티들만 이동
            Assert.AreEqual(2, movementSystem.ProcessedEntityCount, "이동 가능한 2개의 엔티티만 처리되어야 합니다");

            var playerPos = player.GetComponent<PositionComponent>();
            var enemyPos = enemy.GetComponent<PositionComponent>();
            var staticPos = staticObject.GetComponent<PositionComponent>();

            Assert.AreEqual(1, playerPos.X, "플레이어가 이동해야 합니다");
            Assert.AreEqual(9, enemyPos.X, "적이 이동해야 합니다");
            Assert.AreEqual(5, staticPos.X, "정적 객체는 이동하지 않아야 합니다");

            Assert.AreEqual(1, cleanupSystem.CleanupCallCount, "정리 시스템이 호출되어야 합니다");
        }

        [Test]
        public void EntityDestruction_ShouldRemoveFromProcessing()
        {
            // Arrange
            var entity1 = _context.CreateEntity();
            entity1.AddComponent(new PositionComponent(0, 0));
            entity1.AddComponent(new VelocityComponent(1, 1));

            var entity2 = _context.CreateEntity();
            entity2.AddComponent(new PositionComponent(5, 5));
            entity2.AddComponent(new VelocityComponent(2, 2));

            var movementSystem = new MovementSystem();
            _systems.AddSystem(movementSystem);

            // 첫 번째 틱 - 두 엔티티 모두 처리
            _systems.Tick(_context);
            Assert.AreEqual(2, movementSystem.ProcessedEntityCount, "첫 번째 틱에서 두 엔티티가 처리되어야 합니다");

            // Act - 엔티티 하나 제거
            _context.DestroyEntity(entity1);

            // 두 번째 틱 - 남은 엔티티만 처리
            _systems.Tick(_context);

            // Assert
            Assert.AreEqual(1, movementSystem.ProcessedEntityCount, "두 번째 틱에서 남은 엔티티만 처리되어야 합니다");
            Assert.AreEqual(1, _context.GetEntities().Count, "컨텍스트에 하나의 엔티티만 남아있어야 합니다");
        }

        [Test]
        public void ComponentModification_ShouldAffectSystemProcessing()
        {
            // Arrange
            var entity = _context.CreateEntity();
            entity.AddComponent(new PositionComponent(0, 0));
            // Velocity 컴포넌트 없음

            var movementSystem = new MovementSystem();
            _systems.AddSystem(movementSystem);

            // 첫 번째 틱 - 이동 불가
            _systems.Tick(_context);
            Assert.AreEqual(0, movementSystem.ProcessedEntityCount, "Velocity가 없으면 처리되지 않아야 합니다");

            // Act - Velocity 컴포넌트 추가
            entity.AddComponent(new VelocityComponent(3, 4));

            // 두 번째 틱 - 이동 가능
            _systems.Tick(_context);

            // Assert
            Assert.AreEqual(1, movementSystem.ProcessedEntityCount, "Velocity 추가 후 처리되어야 합니다");

            var position = entity.GetComponent<PositionComponent>();
            Assert.AreEqual(3, position.X, "X 위치가 업데이트되어야 합니다");
            Assert.AreEqual(4, position.Y, "Y 위치가 업데이트되어야 합니다");

            // Act - Velocity 컴포넌트 제거
            entity.RemoveComponent<VelocityComponent>();

            // 세 번째 틱 - 다시 이동 불가
            _systems.Tick(_context);
            Assert.AreEqual(0, movementSystem.ProcessedEntityCount, "Velocity 제거 후 처리되지 않아야 합니다");
        }
    }
}
