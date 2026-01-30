using NUnit.Framework;
using OVFL.ECS;

namespace Test
{
    [TestFixture]
    public class EntityComponentTests
    {
        // 테스트용 더미 컴포넌트
        class Position : IComponent { public float x, y; }
        class Velocity : IComponent { public float x, y; }

        [Test]
        public void AddComponent_ShouldStoreAndReturnComponent()
        {
            var entity = new Entity(0, 1);
            var pos = entity.AddComponent<Position>();

            Assert.IsNotNull(pos);
            Assert.IsTrue(entity.HasComponent<Position>());
        }

        [Test]
        public void GetComponent_ShouldReturnCorrectInstance()
        {
            var entity = new Entity(0, 1);
            var originalPos = entity.AddComponent<Position>();
            originalPos.x = 10;

            var retrievedPos = entity.GetComponent<Position>();

            Assert.AreSame(originalPos, retrievedPos); // 참조가 같은지 확인
            Assert.AreEqual(10, retrievedPos.x);
        }

        [Test]
        public void RemoveComponent_ShouldWork()
        {
            var entity = new Entity(0, 1);
            entity.AddComponent<Position>();

            entity.RemoveComponent<Position>();

            Assert.IsFalse(entity.HasComponent<Position>());
            Assert.IsNull(entity.GetComponent<Position>());
        }

        [Test]
        public void Entity_Equality_ShouldCheckIDAndGeneration()
        {
            var e1 = new Entity(1, 1);
            var e2 = new Entity(1, 1);
            var e3 = new Entity(1, 2); // 세대가 다름
            var e4 = new Entity(2, 1); // ID가 다름

            Assert.AreEqual(e1, e2); // 값 동등성 (IEquatable)
            Assert.AreNotEqual(e1, e3);
            Assert.AreNotEqual(e1, e4);

            // HashSet 등에서 키로 쓸 때 중요
            Assert.AreEqual(e1.GetHashCode(), e2.GetHashCode());
        }
    }
}