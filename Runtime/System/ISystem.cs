namespace OVFL.ECS
{
    public interface ISystem
    {
        Context Context { get; set; }
    }

    public interface ISetupSystem : ISystem
    {
        void Setup(Context context);
    }

    public interface ITickSystem : ISystem
    {
        void Tick(Context context);
    }
    public interface ICleanupSystem : ISystem
    {
        void Cleanup(Context context);
    }

    public interface ITeardownSystem : ISystem
    {
        void Teardown(Context context);
    }
}
