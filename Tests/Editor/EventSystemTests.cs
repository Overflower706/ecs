using NUnit.Framework;
using OVFL.ECS;
using System.Collections.Generic;

namespace Test
{
    [TestFixture]
    public class EventSystemTests
    {
        class DamageEvent : EventComponent
        {
            public int Amount;
        }

        class HealEvent : EventComponent
        {
            public int Amount;
        }

        // ───────────────────────────────────────────
        // RaiseEvent / EventPublisherSystem
        // ───────────────────────────────────────────

        [Test]
        public void RaiseEvent_AfterPublisherTick_CreatesEventEntity()
        {
            var context = new Context();
            var publisher = new EventPublisherSystem { Context = context };

            context.RaiseEvent(new DamageEvent { Amount = 10 });
            publisher.Tick();

            var found = false;
            context.ProcessEvents<DamageEvent>((entity, e) => found = true);
            Assert.IsTrue(found);
        }

        [Test]
        public void RaiseEvent_BeforePublisherTick_DoesNotCreateEventEntity()
        {
            var context = new Context();

            context.RaiseEvent(new DamageEvent { Amount = 10 });

            var found = false;
            context.ProcessEvents<DamageEvent>((entity, e) => found = true);
            Assert.IsFalse(found);
        }

        [Test]
        public void ProcessEvents_ShouldReceiveAllRaisedEvents()
        {
            var context = new Context();
            var publisher = new EventPublisherSystem { Context = context };

            context.RaiseEvent(new DamageEvent { Amount = 10 });
            context.RaiseEvent(new DamageEvent { Amount = 20 });
            context.RaiseEvent(new DamageEvent { Amount = 30 });
            publisher.Tick();

            var received = new List<int>();
            context.ProcessEvents<DamageEvent>((entity, e) => received.Add(e.Amount));

            Assert.AreEqual(3, received.Count);
            CollectionAssert.Contains(received, 10);
            CollectionAssert.Contains(received, 20);
            CollectionAssert.Contains(received, 30);
        }

        [Test]
        public void ProcessEventsWhere_ShouldFilterEvents()
        {
            var context = new Context();
            var publisher = new EventPublisherSystem { Context = context };

            context.RaiseEvent(new DamageEvent { Amount = 5 });
            context.RaiseEvent(new DamageEvent { Amount = 15 });
            publisher.Tick();

            var received = new List<int>();
            context.ProcessEventsWhere<DamageEvent>(
                e => e.Amount > 10,
                (entity, e) => received.Add(e.Amount));

            Assert.AreEqual(1, received.Count);
            Assert.AreEqual(15, received[0]);
        }

        // ───────────────────────────────────────────
        // EventCleanupSystem
        // ───────────────────────────────────────────

        [Test]
        public void EventCleanupSystem_ShouldDestroyEventEntities()
        {
            var context = new Context();
            var publisher = new EventPublisherSystem { Context = context };
            var cleanup = new EventCleanupSystem { Context = context };

            context.RaiseEvent(new DamageEvent { Amount = 10 });
            publisher.Tick();
            cleanup.Cleanup();
            context.FlushDestroyQueue();

            var found = false;
            context.ProcessEvents<DamageEvent>((entity, e) => found = true);
            Assert.IsFalse(found);
        }

        [Test]
        public void EventCleanupSystem_ShouldNotDestroyFixedEventEntities()
        {
            var context = new Context();
            var fixedPublisher = new FixedEventPublisherSystem { Context = context };
            var cleanup = new EventCleanupSystem { Context = context };

            context.RaiseFixedEvent(new DamageEvent { Amount = 10 });
            fixedPublisher.FixedTick();
            cleanup.Cleanup();
            context.FlushDestroyQueue();

            // FixedEvent는 EventCleanupSystem이 건드리지 않아야 함
            var found = false;
            context.ProcessEvents<DamageEvent>((entity, e) => found = true);
            Assert.IsTrue(found);
        }

        // ───────────────────────────────────────────
        // RaiseFixedEvent / FixedEventPublisherSystem
        // ───────────────────────────────────────────

        [Test]
        public void RaiseFixedEvent_AfterFixedPublisherTick_CreatesEventEntity()
        {
            var context = new Context();
            var publisher = new FixedEventPublisherSystem { Context = context };

            context.RaiseFixedEvent(new DamageEvent { Amount = 10 });
            publisher.FixedTick();

            var found = false;
            context.ProcessEvents<DamageEvent>((entity, e) => found = true);
            Assert.IsTrue(found);
        }

        [Test]
        public void RaiseFixedEvent_BeforeFixedPublisherTick_DoesNotCreateEventEntity()
        {
            var context = new Context();

            context.RaiseFixedEvent(new DamageEvent { Amount = 10 });

            var found = false;
            context.ProcessEvents<DamageEvent>((entity, e) => found = true);
            Assert.IsFalse(found);
        }

        // ───────────────────────────────────────────
        // FixedEventCleanupSystem
        // ───────────────────────────────────────────

        [Test]
        public void FixedEventCleanupSystem_ShouldDestroyFixedEventEntities()
        {
            var context = new Context();
            var publisher = new FixedEventPublisherSystem { Context = context };
            var cleanup = new FixedEventCleanupSystem { Context = context };

            context.RaiseFixedEvent(new DamageEvent { Amount = 10 });
            publisher.FixedTick();
            cleanup.FixedCleanup();
            context.FlushDestroyQueue();

            var found = false;
            context.ProcessEvents<DamageEvent>((entity, e) => found = true);
            Assert.IsFalse(found);
        }

        [Test]
        public void FixedEventCleanupSystem_ShouldNotDestroyNormalEventEntities()
        {
            var context = new Context();
            var publisher = new EventPublisherSystem { Context = context };
            var fixedCleanup = new FixedEventCleanupSystem { Context = context };

            context.RaiseEvent(new DamageEvent { Amount = 10 });
            publisher.Tick();
            fixedCleanup.FixedCleanup();
            context.FlushDestroyQueue();

            // 일반 Event는 FixedEventCleanupSystem이 건드리지 않아야 함
            var found = false;
            context.ProcessEvents<DamageEvent>((entity, e) => found = true);
            Assert.IsTrue(found);
        }

        // ───────────────────────────────────────────
        // 격리 테스트 (일반 Event ↔ FixedEvent)
        // ───────────────────────────────────────────

        [Test]
        public void NormalAndFixedEvents_AreCleanedUpIndependently()
        {
            var context = new Context();
            var publisher = new EventPublisherSystem { Context = context };
            var fixedPublisher = new FixedEventPublisherSystem { Context = context };
            var cleanup = new EventCleanupSystem { Context = context };
            var fixedCleanup = new FixedEventCleanupSystem { Context = context };

            context.RaiseEvent(new DamageEvent { Amount = 1 });
            context.RaiseFixedEvent(new HealEvent { Amount = 2 });

            publisher.Tick();
            fixedPublisher.FixedTick();

            // Update Cleanup: 일반 이벤트만 삭제
            cleanup.Cleanup();
            context.FlushDestroyQueue();

            var damageFound = false;
            var healFound = false;
            context.ProcessEvents<DamageEvent>((entity, e) => damageFound = true);
            context.ProcessEvents<HealEvent>((entity, e) => healFound = true);

            Assert.IsFalse(damageFound, "일반 이벤트는 Cleanup 후 삭제되어야 함");
            Assert.IsTrue(healFound, "FixedEvent는 Cleanup으로 삭제되지 않아야 함");

            // FixedUpdate Cleanup: FixedEvent 삭제
            fixedCleanup.FixedCleanup();
            context.FlushDestroyQueue();

            healFound = false;
            context.ProcessEvents<HealEvent>((entity, e) => healFound = true);
            Assert.IsFalse(healFound, "FixedEvent는 FixedCleanup 후 삭제되어야 함");
        }

        // ───────────────────────────────────────────
        // EventMetadataComponent
        // ───────────────────────────────────────────

        [Test]
        public void NormalEvent_HasIsFixedFalse()
        {
            var context = new Context();
            var publisher = new EventPublisherSystem { Context = context };

            context.RaiseEvent(new DamageEvent { Amount = 10 });
            publisher.Tick();

            EventMetadataComponent meta = null;
            foreach (var entity in context.AllEntities)
            {
                if (entity.TryGetComponent<EventMetadataComponent>(out var m))
                    meta = m;
            }

            Assert.IsNotNull(meta);
            Assert.IsFalse(meta.IsFixed);
        }

        [Test]
        public void FixedEvent_HasIsFixedTrue()
        {
            var context = new Context();
            var publisher = new FixedEventPublisherSystem { Context = context };

            context.RaiseFixedEvent(new DamageEvent { Amount = 10 });
            publisher.FixedTick();

            EventMetadataComponent meta = null;
            foreach (var entity in context.AllEntities)
            {
                if (entity.TryGetComponent<EventMetadataComponent>(out var m))
                    meta = m;
            }

            Assert.IsNotNull(meta);
            Assert.IsTrue(meta.IsFixed);
        }
    }
}
