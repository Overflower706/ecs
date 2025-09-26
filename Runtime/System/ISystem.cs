namespace OVFL.ECS
{
    public interface ISystem
    {
        Context Context { get; set; }
    }

    public interface ISetupSystem : ISystem
    {
        void Setup();
    }

    public interface ITickSystem : ISystem
    {
        void Tick();
    }
    public interface ICleanupSystem : ISystem
    {
        void Cleanup();
    }

    public interface IFixedTickSystem : ISystem
    {
        void FixedTick();
    }

    public interface ITeardownSystem : ISystem
    {
        void Teardown();
    }
}
