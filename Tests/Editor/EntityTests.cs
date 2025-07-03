using NUnit.Framework;
using OVFL.ECS;

namespace Test.OVFL.ECS
{
    /// <summary>
    /// Entity 클래스에 대한 테스트
    /// </summary>
    [TestFixture]
    public class EntityTests
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

        private Entity _entity;

        [SetUp]
        public void SetUp()
        {
            _entity = new Entity(1);
        }

        [Test]
        public void Entity_HasCorrectID()
        {
            // Arrange & Act
            var entity = new Entity(42);

            // Assert
            Assert.AreEqual(42, entity.ID, "엔티티 ID가 올바르게 설정되어야 합니다");
        }

        [Test]
        public void AddComponent_ShouldAddComponent()
        {
            // Arrange
            var position = new PositionComponent(10, 20);

            // Act
            var addedComponent = _entity.AddComponent(position);

            // Assert
            Assert.AreSame(position, addedComponent, "추가된 컴포넌트가 반환되어야 합니다");
            Assert.IsTrue(_entity.HasComponent<PositionComponent>(), "엔티티가 추가된 컴포넌트를 가져야 합니다");
        }

        [Test]
        public void GetComponent_ShouldReturnCorrectComponent()
        {
            // Arrange
            var position = new PositionComponent(5, 15);
            _entity.AddComponent(position);

            // Act
            var retrievedComponent = _entity.GetComponent<PositionComponent>();

            // Assert
            Assert.AreSame(position, retrievedComponent, "올바른 컴포넌트가 반환되어야 합니다");
            Assert.AreEqual(5, retrievedComponent.X, "컴포넌트 데이터가 올바르게 보존되어야 합니다");
            Assert.AreEqual(15, retrievedComponent.Y, "컴포넌트 데이터가 올바르게 보존되어야 합니다");
        }

        [Test]
        public void GetComponent_WhenNotExists_ShouldReturnNull()
        {
            // Act
            var component = _entity.GetComponent<PositionComponent>();

            // Assert
            Assert.IsNull(component, "존재하지 않는 컴포넌트는 null을 반환해야 합니다");
        }

        [Test]
        public void HasComponent_WhenExists_ShouldReturnTrue()
        {
            // Arrange
            _entity.AddComponent(new HealthComponent());

            // Act & Assert
            Assert.IsTrue(_entity.HasComponent<HealthComponent>(), "존재하는 컴포넌트에 대해 true를 반환해야 합니다");
        }

        [Test]
        public void HasComponent_WhenNotExists_ShouldReturnFalse()
        {
            // Act & Assert
            Assert.IsFalse(_entity.HasComponent<PositionComponent>(), "존재하지 않는 컴포넌트에 대해 false를 반환해야 합니다");
        }

        [Test]
        public void RemoveComponent_WhenExists_ShouldReturnTrue()
        {
            // Arrange
            _entity.AddComponent(new NameComponent("TestEntity"));

            // Act
            var removed = _entity.RemoveComponent<NameComponent>();

            // Assert
            Assert.IsTrue(removed, "존재하는 컴포넌트 제거 시 true를 반환해야 합니다");
            Assert.IsFalse(_entity.HasComponent<NameComponent>(), "제거 후 컴포넌트가 존재하지 않아야 합니다");
            Assert.IsNull(_entity.GetComponent<NameComponent>(), "제거 후 컴포넌트 조회 시 null을 반환해야 합니다");
        }

        [Test]
        public void RemoveComponent_WhenNotExists_ShouldReturnFalse()
        {
            // Act
            var removed = _entity.RemoveComponent<PositionComponent>();

            // Assert
            Assert.IsFalse(removed, "존재하지 않는 컴포넌트 제거 시 false를 반환해야 합니다");
        }

        [Test]
        public void AddComponent_SameTwice_ShouldReplaceComponent()
        {
            // Arrange
            var firstHealth = new HealthComponent(100);
            var secondHealth = new HealthComponent(200);

            // Act
            _entity.AddComponent(firstHealth);
            _entity.AddComponent(secondHealth);

            // Assert
            var retrievedHealth = _entity.GetComponent<HealthComponent>();
            Assert.AreSame(secondHealth, retrievedHealth, "두 번째로 추가된 컴포넌트가 첫 번째를 대체해야 합니다");
            Assert.AreEqual(200, retrievedHealth.MaxHealth, "새로운 컴포넌트의 데이터가 올바르게 설정되어야 합니다");
        }

        [Test]
        public void Entity_CanHaveMultipleComponents()
        {
            // Arrange & Act
            _entity.AddComponent(new PositionComponent(1, 2));
            _entity.AddComponent(new HealthComponent(50));
            _entity.AddComponent(new NameComponent("MultiComponent"));

            // Assert
            Assert.IsTrue(_entity.HasComponent<PositionComponent>(), "Position 컴포넌트가 존재해야 합니다");
            Assert.IsTrue(_entity.HasComponent<HealthComponent>(), "Health 컴포넌트가 존재해야 합니다");
            Assert.IsTrue(_entity.HasComponent<NameComponent>(), "Name 컴포넌트가 존재해야 합니다");

            // 각 컴포넌트의 데이터 검증
            var position = _entity.GetComponent<PositionComponent>();
            var health = _entity.GetComponent<HealthComponent>();
            var name = _entity.GetComponent<NameComponent>();

            Assert.AreEqual(1, position.X, "Position X가 올바르게 설정되어야 합니다");
            Assert.AreEqual(2, position.Y, "Position Y가 올바르게 설정되어야 합니다");
            Assert.AreEqual(50, health.MaxHealth, "Health가 올바르게 설정되어야 합니다");
            Assert.AreEqual("MultiComponent", name.Name, "Name이 올바르게 설정되어야 합니다");
        }

        [Test]
        public void Entity_DifferentIDs_AreDifferent()
        {
            // Arrange & Act
            var entity1 = new Entity(1);
            var entity2 = new Entity(2);

            // Assert
            Assert.AreNotEqual(entity1.ID, entity2.ID, "서로 다른 엔티티는 다른 ID를 가져야 합니다");
        }
    }
}
