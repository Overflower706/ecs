using NUnit.Framework;
using OVFL.ECS;
using System.Collections.Generic;

namespace Test
{
    [TestFixture]
    public class SystemsTests
    {
        // 모의 시스템 (Mock System)
        class MockSystem : ISetupSystem, ITickSystem
        {
            public Context Context { get; set; }
            public int SetupCount = 0;
            public int TickCount = 0;
            public List<string> ExecutionLog; // 실행 순서 기록용

            public MockSystem(List<string> log) => ExecutionLog = log;

            public void Setup()
            {
                SetupCount++;
                ExecutionLog.Add("Setup");
            }

            public void Tick()
            {
                TickCount++;
                ExecutionLog.Add("Tick");
            }
        }

        class MockTeardownSystem : ITeardownSystem
        {
            public Context Context { get; set; }
            public int TeardownCount = 0;

            public void Teardown() => TeardownCount++;
        }

        [Test]
        public void SetContext_ShouldInjectContextToAllSystems()
        {
            var context = new Context();
            var systems = new Systems(context);
            var log = new List<string>();
            var mockSys = new MockSystem(log);

            systems.AddSystem(mockSys); // AddSystem 시점에 주입됨

            Assert.IsNotNull(mockSys.Context);
            Assert.AreEqual(context, mockSys.Context);
        }

        [Test]
        public void LifeCycle_ShouldRunInOrder()
        {
            var context = new Context();
            var systems = new Systems(context);
            var log = new List<string>();

            // 시스템 등록
            systems.AddSystem(new MockSystem(log));

            // 실행
            systems.Setup(); // Log: "Setup"
            systems.Tick();  // Log: "Tick"
            systems.Tick();  // Log: "Tick"

            Assert.AreEqual(3, log.Count);
            Assert.AreEqual("Setup", log[0]);
            Assert.AreEqual("Tick", log[1]);
            Assert.AreEqual("Tick", log[2]);
        }

        [Test]
        public void UnregisterSystem_ShouldStopSystemFromRunning()
        {
            var context = new Context();
            var systems = new Systems(context);
            var log = new List<string>();
            var mockSys = new MockSystem(log);

            systems.AddSystem(mockSys);
            systems.Tick(); // Tick 1회

            systems.UnregisterSystem(mockSys);
            systems.Tick(); // 제거됐으므로 실행 안 됨

            Assert.AreEqual(1, mockSys.TickCount);
        }

        [Test]
        public void UnregisterAll_ShouldClearAllSystems()
        {
            var context = new Context();
            var systems = new Systems(context);
            var log = new List<string>();

            systems.AddSystem(new MockSystem(log));
            systems.UnregisterAll();

            systems.Setup();
            systems.Tick();

            Assert.AreEqual(0, log.Count);
        }

        [Test]
        public void Teardown_ShouldUnregisterAllSystemsAfterExecution()
        {
            var context = new Context();
            var systems = new Systems(context);
            var log = new List<string>();
            var mockSys = new MockSystem(log);
            var teardownSys = new MockTeardownSystem();

            systems.AddSystem(mockSys);
            systems.AddSystem(teardownSys);

            systems.Teardown();

            // Teardown이 실행됐는지 확인
            Assert.AreEqual(1, teardownSys.TeardownCount);

            // Teardown 이후 시스템이 해제됐는지 확인
            systems.Tick();
            Assert.AreEqual(0, mockSys.TickCount);
        }
    }
}
