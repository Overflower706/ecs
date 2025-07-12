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
        private readonly List<ITeardownSystem> teardownSystems = new();

        /// <summary>
        /// Context를 설정합니다
        /// </summary>
        public void SetContext(Context context)
        {
            this.context = context;

            // 기존 시스템들에 Context 할당
            foreach (var system in allSystems)
            {
                system.Context = context;
            }
        }

        /// <summary>
        /// System을 추가합니다
        /// </summary>
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

            if (system is ITeardownSystem teardownSystem)
                teardownSystems.Add(teardownSystem);

            return this;
        }

        /// <summary>
        /// System을 제네릭으로 추가합니다
        /// </summary>
        public Systems AddSystem<T>() where T : ISystem, new()
        {
            var system = new T();
            return AddSystem(system);
        }

        /// <summary>
        /// 모든 Setup System을 실행합니다 (초기화 시 한 번)
        /// </summary>
        public void Setup(Context context)
        {
            foreach (var system in setupSystems)
            {
                system.Setup(context);
            }
        }

        /// <summary>
        /// 모든 Tick System을 실행합니다 (매 프레임)
        /// </summary>
        public void Tick(Context context)
        {
            foreach (var system in tickSystems)
            {
                system.Tick(context);
            }
        }

        /// <summary>
        /// 모든 Cleanup System을 실행합니다 (Tick 이후)
        /// </summary>
        public void Cleanup(Context context)
        {
            foreach (var system in cleanupSystems)
            {
                system.Cleanup(context);
            }
        }

        /// <summary>
        /// 모든 Teardown System을 실행합니다 (마무리 시 한 번)
        /// </summary>
        public void Teardown(Context context)
        {
            foreach (var system in teardownSystems)
            {
                system.Teardown(context);
            }
        }
    }
}
