using NUnit.Framework;
using OVFL.ECS;
using System.Collections.Generic;

namespace Test.OVFL.ECS
{
    /// <summary>
    /// Systems 클래스와 시스템 인터페이스들에 대한 테스트
    /// </summary>
    [TestFixture]
    public class SystemsTests
    {
        // 테스트용 컴포넌트
        private class PositionComponent : IComponent
        {
            public float X { get; set; }
            public float Y { get; set; }
        }

        private class VelocityComponent : IComponent
        {
            public float VX { get; set; }
            public float VY { get; set; }
        }

        // 테스트용 시스템들
        private class TestSetupSystem : ISetupSystem
        {
            public Context Context { get; set; }
            public bool WasSetupCalled { get; private set; }
            public Context SetupContext { get; private set; }

            public void Setup(Context context)
            {
                WasSetupCalled = true;
                SetupContext = context;
            }
        }

        private class TestTickSystem : ITickSystem
        {
            public Context Context { get; set; }
            public int TickCallCount { get; private set; }
            public Context LastTickContext { get; private set; }

            public void Tick(Context context)
            {
                TickCallCount++;
                LastTickContext = context;
            }
        }

        private class TestCleanupSystem : ICleanupSystem
        {
            public Context Context { get; set; }
            public bool WasCleanupCalled { get; private set; }
            public Context CleanupContext { get; private set; }

            public void Cleanup(Context context)
            {
                WasCleanupCalled = true;
                CleanupContext = context;
            }
        }

        private class TestTeardownSystem : ITeardownSystem
        {
            public Context Context { get; set; }
            public bool WasTeardownCalled { get; private set; }
            public Context TeardownContext { get; private set; }

            public void Teardown(Context context)
            {
                WasTeardownCalled = true;
                TeardownContext = context;
            }
        }

        // 복합 시스템 (여러 인터페이스 구현)
        private class ComplexSystem : ISetupSystem, ITickSystem, ICleanupSystem, ITeardownSystem
        {
            public Context Context { get; set; }
            public bool WasSetupCalled { get; private set; }
            public bool WasTickCalled { get; private set; }
            public bool WasCleanupCalled { get; private set; }
            public bool WasTeardownCalled { get; private set; }

            public void Setup(Context context) => WasSetupCalled = true;
            public void Tick(Context context) => WasTickCalled = true;
            public void Cleanup(Context context) => WasCleanupCalled = true;
            public void Teardown(Context context) => WasTeardownCalled = true;
        }

        // 순서 테스트용 시스템
        private class OrderTrackingSystem : ISetupSystem, ITickSystem, ICleanupSystem, ITeardownSystem
        {
            public Context Context { get; set; }
            private static readonly List<string> _callOrder = new List<string>();
            private readonly string _systemName;

            public OrderTrackingSystem(string systemName)
            {
                _systemName = systemName;
            }

            public static List<string> CallOrder => _callOrder;
            public static void ClearOrder() => _callOrder.Clear();

            public void Setup(Context context) => _callOrder.Add($"{_systemName}.Setup");
            public void Tick(Context context) => _callOrder.Add($"{_systemName}.Tick");
            public void Cleanup(Context context) => _callOrder.Add($"{_systemName}.Cleanup");
            public void Teardown(Context context) => _callOrder.Add($"{_systemName}.Teardown");
        }

        private Systems _systems;
        private Context _context;

        [SetUp]
        public void SetUp()
        {
            _systems = new Systems();
            _context = new Context();
            _systems.SetContext(_context);
            OrderTrackingSystem.ClearOrder();
        }

        [Test]
        public void Add_SetupSystem_ShouldBeCallableInSetup()
        {
            // Arrange
            var setupSystem = new TestSetupSystem();

            // Act
            _systems.AddSystem(setupSystem);
            _systems.Setup(_context);

            // Assert
            Assert.IsTrue(setupSystem.WasSetupCalled, "Setup 시스템의 Setup 메서드가 호출되어야 합니다");
            Assert.AreSame(_context, setupSystem.SetupContext, "올바른 Context가 전달되어야 합니다");
        }

        [Test]
        public void Add_TickSystem_ShouldBeCallableInTick()
        {
            // Arrange
            var tickSystem = new TestTickSystem();

            // Act
            _systems.AddSystem(tickSystem);
            _systems.Tick(_context);
            _systems.Tick(_context);

            // Assert
            Assert.AreEqual(2, tickSystem.TickCallCount, "Tick 시스템의 Tick 메서드가 호출된 횟수가 정확해야 합니다");
            Assert.AreSame(_context, tickSystem.LastTickContext, "올바른 Context가 전달되어야 합니다");
        }

        [Test]
        public void Add_CleanupSystem_ShouldBeCallableInCleanup()
        {
            // Arrange
            var cleanupSystem = new TestCleanupSystem();

            // Act
            _systems.AddSystem(cleanupSystem);
            _systems.Cleanup(_context);

            // Assert
            Assert.IsTrue(cleanupSystem.WasCleanupCalled, "Cleanup 시스템의 Cleanup 메서드가 호출되어야 합니다");
            Assert.AreSame(_context, cleanupSystem.CleanupContext, "올바른 Context가 전달되어야 합니다");
        }

        [Test]
        public void Add_TeardownSystem_ShouldBeCallableInTeardown()
        {
            // Arrange
            var teardownSystem = new TestTeardownSystem();

            // Act
            _systems.AddSystem(teardownSystem);
            _systems.Teardown(_context);

            // Assert
            Assert.IsTrue(teardownSystem.WasTeardownCalled, "Teardown 시스템의 Teardown 메서드가 호출되어야 합니다");
            Assert.AreSame(_context, teardownSystem.TeardownContext, "올바른 Context가 전달되어야 합니다");
        }

        [Test]
        public void Add_ComplexSystem_ShouldBeCalledInAllPhases()
        {
            // Arrange
            var complexSystem = new ComplexSystem();

            // Act
            _systems.AddSystem(complexSystem);
            _systems.Setup(_context);
            _systems.Tick(_context);
            _systems.Cleanup(_context);
            _systems.Teardown(_context);

            // Assert
            Assert.IsTrue(complexSystem.WasSetupCalled, "Setup이 호출되어야 합니다");
            Assert.IsTrue(complexSystem.WasTickCalled, "Tick이 호출되어야 합니다");
            Assert.IsTrue(complexSystem.WasCleanupCalled, "Cleanup이 호출되어야 합니다");
            Assert.IsTrue(complexSystem.WasTeardownCalled, "Teardown이 호출되어야 합니다");
        }

        [Test]
        public void Add_MultipleSystemsOfSameType_ShouldAllBeCalled()
        {
            // Arrange
            var tickSystem1 = new TestTickSystem();
            var tickSystem2 = new TestTickSystem();

            // Act
            _systems.AddSystem(tickSystem1);
            _systems.AddSystem(tickSystem2);
            _systems.Tick(_context);

            // Assert
            Assert.AreEqual(1, tickSystem1.TickCallCount, "첫 번째 시스템이 호출되어야 합니다");
            Assert.AreEqual(1, tickSystem2.TickCallCount, "두 번째 시스템이 호출되어야 합니다");
        }

        [Test]
        public void Systems_CallOrder_ShouldBeInAdditionOrder()
        {
            // Arrange
            var system1 = new OrderTrackingSystem("System1");
            var system2 = new OrderTrackingSystem("System2");
            var system3 = new OrderTrackingSystem("System3");

            // Act
            _systems.AddSystem(system1);
            _systems.AddSystem(system2);
            _systems.AddSystem(system3);

            _systems.Setup(_context);

            // Assert
            var expectedOrder = new[] { "System1.Setup", "System2.Setup", "System3.Setup" };
            CollectionAssert.AreEqual(expectedOrder, OrderTrackingSystem.CallOrder, "시스템들이 추가된 순서대로 호출되어야 합니다");
        }

        [Test]
        public void Systems_MultiplePhases_ShouldMaintainOrder()
        {
            // Arrange
            var system1 = new OrderTrackingSystem("A");
            var system2 = new OrderTrackingSystem("B");

            // Act
            _systems.AddSystem(system1);
            _systems.AddSystem(system2);

            _systems.Setup(_context);
            _systems.Tick(_context);
            _systems.Cleanup(_context);
            _systems.Teardown(_context);

            // Assert
            var expectedOrder = new[]
            {
                "A.Setup", "B.Setup",
                "A.Tick", "B.Tick",
                "A.Cleanup", "B.Cleanup",
                "A.Teardown", "B.Teardown"
            };
            CollectionAssert.AreEqual(expectedOrder, OrderTrackingSystem.CallOrder, "모든 단계에서 시스템 순서가 유지되어야 합니다");
        }

        [Test]
        public void Add_ReturnsSystemsInstance_ForMethodChaining()
        {
            // Arrange
            var system1 = new TestSetupSystem();
            var system2 = new TestTickSystem();

            // Act & Assert
            var result = _systems.AddSystem(system1).AddSystem(system2);

            Assert.AreSame(_systems, result, "Add 메서드는 Systems 인스턴스를 반환하여 메서드 체이닝을 지원해야 합니다");
        }

        [Test]
        public void Setup_WithoutSystems_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _systems.Setup(_context), "시스템이 없어도 Setup 호출 시 예외가 발생하지 않아야 합니다");
        }

        [Test]
        public void Tick_WithoutSystems_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _systems.Tick(_context), "시스템이 없어도 Tick 호출 시 예외가 발생하지 않아야 합니다");
        }

        [Test]
        public void Cleanup_WithoutSystems_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _systems.Cleanup(_context), "시스템이 없어도 Cleanup 호출 시 예외가 발생하지 않아야 합니다");
        }

        [Test]
        public void Teardown_WithoutSystems_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _systems.Teardown(_context), "시스템이 없어도 Teardown 호출 시 예외가 발생하지 않아야 합니다");
        }

        [Test]
        public void Systems_FullLifecycle_Integration()
        {
            // Arrange
            var setupSystem = new TestSetupSystem();
            var tickSystem = new TestTickSystem();
            var cleanupSystem = new TestCleanupSystem();
            var teardownSystem = new TestTeardownSystem();

            // Act
            _systems.AddSystem(setupSystem)
                   .AddSystem(tickSystem)
                   .AddSystem(cleanupSystem)
                   .AddSystem(teardownSystem);

            // 전체 라이프사이클 실행
            _systems.Setup(_context);
            _systems.Tick(_context);
            _systems.Tick(_context);
            _systems.Cleanup(_context);
            _systems.Teardown(_context);

            // Assert
            Assert.IsTrue(setupSystem.WasSetupCalled, "Setup 시스템이 호출되어야 합니다");
            Assert.AreEqual(2, tickSystem.TickCallCount, "Tick 시스템이 2번 호출되어야 합니다");
            Assert.IsTrue(cleanupSystem.WasCleanupCalled, "Cleanup 시스템이 호출되어야 합니다");
            Assert.IsTrue(teardownSystem.WasTeardownCalled, "Teardown 시스템이 호출되어야 합니다");
        }
    }
}
