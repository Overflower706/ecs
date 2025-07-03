using NUnit.Framework;
using OVFL.ECS;

namespace Test.OVFL.ECS
{
    /// <summary>
    /// IComponent 인터페이스에 대한 테스트
    /// </summary>
    [TestFixture]
    public class IComponentTests
    {
        // 테스트용 컴포넌트들
        private class TestComponent : IComponent
        {
            public string Data { get; set; } = "Test";
        }

        private class AnotherTestComponent : IComponent
        {
            public int Value { get; set; } = 42;
        }

        [Test]
        public void TestComponent_ImplementsIComponent()
        {
            // Arrange & Act
            var component = new TestComponent();

            // Assert
            Assert.IsTrue(component is IComponent, "TestComponent는 IComponent를 구현해야 합니다");
        }

        [Test]
        public void TestComponent_CanStoreData()
        {
            // Arrange & Act
            var component = new TestComponent { Data = "Custom Data" };

            // Assert
            Assert.AreEqual("Custom Data", component.Data, "컴포넌트는 데이터를 저장할 수 있어야 합니다");
        }

        [Test]
        public void DifferentComponents_AreDifferentTypes()
        {
            // Arrange
            var component1 = new TestComponent();
            var component2 = new AnotherTestComponent();

            // Act & Assert
            Assert.AreNotEqual(component1.GetType(), component2.GetType(), "서로 다른 컴포넌트는 다른 타입이어야 합니다");
            Assert.IsTrue(component1 is IComponent, "TestComponent는 IComponent여야 합니다");
            Assert.IsTrue(component2 is IComponent, "AnotherTestComponent는 IComponent여야 합니다");
        }

        [Test]
        public void Component_CanBeCastToInterface()
        {
            // Arrange
            var component = new TestComponent();

            // Act
            IComponent interfaceComponent = component;

            // Assert
            Assert.IsNotNull(interfaceComponent, "컴포넌트는 IComponent로 캐스트할 수 있어야 합니다");
            Assert.AreSame(component, interfaceComponent, "캐스트된 인터페이스는 원본과 같은 객체여야 합니다");
        }
    }
}
