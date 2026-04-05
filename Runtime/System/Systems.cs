using System;
using System.Collections.Generic;

namespace OVFL.ECS
{
    public class Systems
    {
        private Context context;
        private readonly List<ISystem> allSystems = new();
        private readonly List<ISetupSystem> setupSystems = new();
        private readonly List<ITickSystem> tickSystems = new();
        private readonly List<ICleanupSystem> cleanupSystems = new();
        private readonly List<IFixedTickSystem> fixedTickSystems = new();
        private readonly List<ITeardownSystem> teardownSystems = new();

        [Obsolete("기본 생성자는 더 이상 사용되지 않습니다. 대신 Context를 전달하는 생성자를 사용하세요.")]
        public Systems()
        {

        }

        public Systems(Context context)
        {
            this.context = context;
        }

        // TODO : v1.6.0에서 제거
        [Obsolete("SetContext는 더 이상 사용되지 않습니다. 대신 Systems 생성자로 Context를 전달하세요.")]
        public void SetContext(Context context)
        {
            this.context = context;

            // 기존 시스템들에 Context 할당
            foreach (var system in allSystems)
            {
                system.Context = context;
            }
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

            if (system is ITeardownSystem teardownSystem)
                teardownSystems.Add(teardownSystem);

            return this;
        }

        public Systems AddSystem<T>() where T : ISystem, new()
        {
            var system = new T();
            return AddSystem(system);
        }

        public Systems UnregisterSystem(ISystem system)
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

            if (system is ITeardownSystem teardownSystem)
                teardownSystems.Remove(teardownSystem);

            return this;
        }

        public void UnregisterAll()
        {
            allSystems.Clear();
            setupSystems.Clear();
            tickSystems.Clear();
            cleanupSystems.Clear();
            fixedTickSystems.Clear();
            teardownSystems.Clear();
        }

        /// <summary>
        /// 모든 Setup System을 실행합니다 (초기화 시 한 번)
        /// </summary>
        public void Setup()
        {
            foreach (var system in setupSystems)
            {
                system.Setup();
            }
        }

        /// <summary>
        /// 모든 Tick System을 실행합니다 (매 프레임)
        /// </summary>
        public void Tick()
        {
            foreach (var system in tickSystems)
            {
                system.Tick();
            }

            context?.FlushDestroyQueue();
        }

        /// <summary>
        /// 모든 Cleanup System을 실행합니다 (Tick 이후)
        /// </summary>
        public void Cleanup()
        {
            foreach (var system in cleanupSystems)
            {
                system.Cleanup();
            }
        }

        /// <summary>
        /// 모든 FixedTick System을 실행합니다 (고정 시간)
        /// </summary>
        public void FixedTick()
        {
            foreach (var system in fixedTickSystems)
            {
                system.FixedTick();
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
                system.Teardown();
            }

            UnregisterAll();
        }
    }
}
