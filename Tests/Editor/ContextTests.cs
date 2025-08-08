using NUnit.Framework;
using OVFL.ECS;
using System.Linq;

namespace Test.OVFL.ECS
{
    /// <summary>
    /// Context 클래스에 대한 테스트
    /// </summary>
    [TestFixture]
    public class ContextTests
    {
        // 테스트용 컴포넌트
        private class TestComponent : IComponent
        {
            public string Data { get; set; }

            public TestComponent(string data = "Test")
            {
                Data = data;
            }
        }

        private Context _context;

        [SetUp]
        public void SetUp()
        {
            _context = new Context();
        }

        [Test]
        public void CreateEntity_ShouldCreateNewEntity()
        {
            // Act
            var entity = _context.CreateEntity();

            // Assert
            Assert.IsNotNull(entity, "생성된 엔티티는 null이 아니어야 합니다");
            Assert.Greater(entity.ID, 0, "엔티티 ID는 0보다 커야 합니다");
        }

        [Test]
        public void CreateEntity_ShouldAssignUniqueIDs()
        {
            // Act
            var entity1 = _context.CreateEntity();
            var entity2 = _context.CreateEntity();
            var entity3 = _context.CreateEntity();

            // Assert
            Assert.AreNotEqual(entity1.ID, entity2.ID, "각 엔티티는 고유한 ID를 가져야 합니다");
            Assert.AreNotEqual(entity2.ID, entity3.ID, "각 엔티티는 고유한 ID를 가져야 합니다");
            Assert.AreNotEqual(entity1.ID, entity3.ID, "각 엔티티는 고유한 ID를 가져야 합니다");
        }

        [Test]
        public void CreateEntity_ShouldAssignIncrementalIDs()
        {
            // Act
            var entity1 = _context.CreateEntity();
            var entity2 = _context.CreateEntity();

            // Assert
            Assert.AreEqual(entity1.ID + 1, entity2.ID, "엔티티 ID는 순차적으로 증가해야 합니다");
        }

        [Test]
        public void CreateEntity_ShouldAddToEntityList()
        {
            // Act
            var entity = _context.CreateEntity();

            // Assert
            var entities = _context.GetEntities();
            Assert.Contains(entity, entities.ToList(), "생성된 엔티티는 엔티티 목록에 포함되어야 합니다");
        }

        [Test]
        public void GetEntities_InitiallyEmpty()
        {
            // Act
            var entities = _context.GetEntities();

            // Assert
            Assert.IsNotNull(entities, "엔티티 목록은 null이 아니어야 합니다");
            Assert.AreEqual(0, entities.Count, "초기 엔티티 목록은 비어있어야 합니다");
        }

        [Test]
        public void GetEntities_ShouldReturnReadOnlyList()
        {
            // Act
            var entities = _context.GetEntities();

            // Assert
            Assert.IsInstanceOf<System.Collections.Generic.IReadOnlyList<Entity>>(entities, "엔티티 목록은 읽기 전용이어야 합니다");
        }

        [Test]
        public void GetEntities_ShouldContainAllCreatedEntities()
        {
            // Arrange
            var entity1 = _context.CreateEntity();
            var entity2 = _context.CreateEntity();
            var entity3 = _context.CreateEntity();

            // Act
            var entities = _context.GetEntities();

            // Assert
            Assert.AreEqual(3, entities.Count, "생성된 모든 엔티티가 목록에 포함되어야 합니다");
            Assert.Contains(entity1, entities.ToList(), "첫 번째 엔티티가 목록에 포함되어야 합니다");
            Assert.Contains(entity2, entities.ToList(), "두 번째 엔티티가 목록에 포함되어야 합니다");
            Assert.Contains(entity3, entities.ToList(), "세 번째 엔티티가 목록에 포함되어야 합니다");
        }

        [Test]
        public void DestroyEntity_WhenExists_ShouldReturnTrue()
        {
            // Arrange
            var entity = _context.CreateEntity();

            // Act
            var destroyed = _context.DestroyEntity(entity);

            // Assert
            Assert.IsTrue(destroyed, "존재하는 엔티티 제거 시 true를 반환해야 합니다");
        }

        [Test]
        public void DestroyEntity_WhenExists_ShouldRemoveFromList()
        {
            // Arrange
            var entity1 = _context.CreateEntity();
            var entity2 = _context.CreateEntity();

            // Act
            _context.DestroyEntity(entity1);

            // Assert
            var entities = _context.GetEntities();
            Assert.AreEqual(1, entities.Count, "엔티티가 목록에서 제거되어야 합니다");
            Assert.IsFalse(entities.Contains(entity1), "제거된 엔티티는 목록에 포함되지 않아야 합니다");
            Assert.IsTrue(entities.Contains(entity2), "제거되지 않은 엔티티는 목록에 남아있어야 합니다");
        }

        [Test]
        public void DestroyEntity_WhenNotExists_ShouldReturnFalse()
        {
            // Arrange
            var externalEntity = new Entity(999); // Context가 생성하지 않은 엔티티

            // Act
            var destroyed = _context.DestroyEntity(externalEntity);

            // Assert
            Assert.IsFalse(destroyed, "존재하지 않는 엔티티 제거 시 false를 반환해야 합니다");
        }

        [Test]
        public void DestroyEntity_MultipleEntities_ShouldWorkCorrectly()
        {
            // Arrange
            var entity1 = _context.CreateEntity();
            var entity2 = _context.CreateEntity();
            var entity3 = _context.CreateEntity();

            // Act
            var destroyed1 = _context.DestroyEntity(entity1);
            var destroyed3 = _context.DestroyEntity(entity3);

            // Assert
            Assert.IsTrue(destroyed1, "첫 번째 엔티티 제거가 성공해야 합니다");
            Assert.IsTrue(destroyed3, "세 번째 엔티티 제거가 성공해야 합니다");

            var entities = _context.GetEntities();
            Assert.AreEqual(1, entities.Count, "하나의 엔티티만 남아있어야 합니다");
            Assert.IsTrue(entities.Contains(entity2), "두 번째 엔티티만 남아있어야 합니다");
        }

        [Test]
        public void Context_EntityLifecycle_Integration()
        {
            // Arrange & Act
            var entity = _context.CreateEntity();
            entity.AddComponent(new TestComponent("ContextTest"));

            // 엔티티가 컨텍스트와 독립적으로 작동하는지 확인
            var component = entity.GetComponent<TestComponent>();

            // Assert
            Assert.IsNotNull(component, "엔티티의 컴포넌트가 정상적으로 작동해야 합니다");
            Assert.AreEqual("ContextTest", component.Data, "컴포넌트 데이터가 올바르게 보존되어야 합니다");

            // 엔티티 제거 후에도 엔티티 자체는 여전히 유효함
            _context.DestroyEntity(entity);
            Assert.IsNotNull(entity.GetComponent<TestComponent>(), "제거된 엔티티의 컴포넌트는 여전히 접근 가능해야 합니다");
        }

        [Test]
        public void Context_MultipleContexts_ShouldBeIndependent()
        {
            // Arrange
            var context2 = new Context();

            // Act
            var entity1 = _context.CreateEntity();
            var entity2 = context2.CreateEntity();

            // Assert
            Assert.AreEqual(1, _context.GetEntities().Count, "첫 번째 컨텍스트는 하나의 엔티티를 가져야 합니다");
            Assert.AreEqual(1, context2.GetEntities().Count, "두 번째 컨텍스트는 하나의 엔티티를 가져야 합니다");

            // 서로 다른 컨텍스트의 엔티티는 서로 영향을 주지 않음
            Assert.IsFalse(_context.GetEntities().Contains(entity2), "다른 컨텍스트의 엔티티는 포함되지 않아야 합니다");
            Assert.IsFalse(context2.GetEntities().Contains(entity1), "다른 컨텍스트의 엔티티는 포함되지 않아야 합니다");
        }

        [Test]
        public void ComponentCaching_PerformanceTest_ShouldBeFasterThanLinearSearch()
        {
            // Arrange - 많은 엔티티 생성
            const int entityCount = 1000;
            var entities = new Entity[entityCount];

            for (int i = 0; i < entityCount; i++)
            {
                entities[i] = _context.CreateEntity();
                if (i % 10 == 0) // 10개 중 1개만 TestComponent 추가
                {
                    entities[i].AddComponent(new TestComponent($"Entity{i}"));
                }
            }

            // Act - 여러 번 조회해서 캐시 성능 확인
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < 100; i++) // 100번 조회
            {
                var result = _context.GetEntitiesWithComponent<TestComponent>();
                Assert.AreEqual(100, result.Count, "매번 동일한 결과를 반환해야 합니다");
            }

            stopwatch.Stop();

            // Assert - 성능 검증 (구체적인 시간은 환경에 따라 다르므로 로그만 출력)
            UnityEngine.Debug.Log($"1000개 엔티티 중 100번 조회 시간: {stopwatch.ElapsedMilliseconds}ms");
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, "캐시를 사용하면 100ms 이내에 완료되어야 합니다");
        }
    }
}
