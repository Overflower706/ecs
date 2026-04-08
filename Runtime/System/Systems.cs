using System;
using System.Collections.Generic;

namespace OVFL.ECS
{
    public class Systems
    {
        private readonly Context context;
        private readonly List<ISystem> allSystems = new();
        private readonly List<ISetupSystem> setupSystems = new();
        private readonly List<ITickSystem> tickSystems = new();
        private readonly List<ICleanupSystem> cleanupSystems = new();
        private readonly List<IFixedTickSystem> fixedTickSystems = new();
        private readonly List<IFixedCleanupSystem> fixedCleanupSystems = new();
        private readonly List<ITeardownSystem> teardownSystems = new();

        public Systems(Context context)
        {
            this.context = context;
        }

        public Systems AddSystem(ISystem system)
        {
            allSystems.Add(system);

            system.Context = context;

            if (system is ISetupSystem setupSystem)
                setupSystems.Add(setupSystem);

            if (system is ITickSystem tickSystem)
                tickSystems.Add(tickSystem);

            if (system is ICleanupSystem cleanupSystem)
                cleanupSystems.Add(cleanupSystem);

            if (system is IFixedTickSystem fixedTickSystem)
                fixedTickSystems.Add(fixedTickSystem);

            if (system is IFixedCleanupSystem fixedCleanupSystem)
                fixedCleanupSystems.Add(fixedCleanupSystem);

            if (system is ITeardownSystem teardownSystem)
                teardownSystems.Add(teardownSystem);

            return this;
        }

        public Systems AddSystem<T>() where T : ISystem, new()
        {
            var system = new T();
            return AddSystem(system);
        }

        public Systems RemoveSystem(ISystem system)
        {
            allSystems.Remove(system);

            if (system is ISetupSystem setupSystem)
                setupSystems.Remove(setupSystem);

            if (system is ITickSystem tickSystem)
                tickSystems.Remove(tickSystem);

            if (system is ICleanupSystem cleanupSystem)
                cleanupSystems.Remove(cleanupSystem);

            if (system is IFixedTickSystem fixedTickSystem)
                fixedTickSystems.Remove(fixedTickSystem);

            if (system is IFixedCleanupSystem fixedCleanupSystem)
                fixedCleanupSystems.Remove(fixedCleanupSystem);

            if (system is ITeardownSystem teardownSystem)
                teardownSystems.Remove(teardownSystem);

            return this;
        }

        public void RemoveAllSystems()
        {
            allSystems.Clear();
            setupSystems.Clear();
            tickSystems.Clear();
            cleanupSystems.Clear();
            fixedTickSystems.Clear();
            fixedCleanupSystems.Clear();
            teardownSystems.Clear();
        }

        /// <summary>
        /// 모든 Setup System을 실행합니다 (초기화 시 한 번)
        /// </summary>
        public void Setup()
        {
            foreach (var system in setupSystems)
            {
                try { system.Setup(); }
                catch (Exception e) { UnityEngine.Debug.LogException(e); }
            }
        }

        /// <summary>
        /// 모든 Tick System을 실행합니다.
        /// </summary>
        public void Tick()
        {
            foreach (var system in tickSystems)
            {
                try { system.Tick(); }
                catch (Exception e) { UnityEngine.Debug.LogException(e); }
            }
        }

        /// <summary>
        /// 모든 Cleanup System을 실행합니다. Tick() 이후 직접 호출해야 합니다.
        /// </summary>
        public void Cleanup()
        {
            context?.FlushDestroyQueue();
            foreach (var system in cleanupSystems)
            {
                try { system.Cleanup(); }
                catch (Exception e) { UnityEngine.Debug.LogException(e); }
            }
            context?.FlushDestroyQueue();
        }

        /// <summary>
        /// 모든 FixedTick System을 실행합니다.
        /// </summary>
        public void FixedTick()
        {
            foreach (var system in fixedTickSystems)
            {
                try { system.FixedTick(); }
                catch (Exception e) { UnityEngine.Debug.LogException(e); }
            }
        }

        /// <summary>
        /// 모든 FixedCleanup System을 실행합니다. FixedTick() 이후 직접 호출해야 합니다.
        /// </summary>
        public void FixedCleanup()
        {
            context?.FlushDestroyQueue();
            foreach (var system in fixedCleanupSystems)
            {
                try { system.FixedCleanup(); }
                catch (Exception e) { UnityEngine.Debug.LogException(e); }
            }
            context?.FlushDestroyQueue();
        }

        /// <summary>
        /// 모든 Teardown System을 실행합니다 (마무리 시 한 번)
        /// 실행 완료 후 모든 시스템이 자동으로 해제됩니다.
        /// </summary>
        public void Teardown()
        {
            foreach (var system in teardownSystems)
            {
                try { system.Teardown(); }
                catch (Exception e) { UnityEngine.Debug.LogException(e); }
            }
            context?.FlushDestroyQueue();
            RemoveAllSystems();
        }
    }
}
