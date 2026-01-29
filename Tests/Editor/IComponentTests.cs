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
