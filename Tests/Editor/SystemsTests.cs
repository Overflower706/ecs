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
    }
}
