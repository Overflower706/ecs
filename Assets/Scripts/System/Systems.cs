using System.Collections.Generic;

namespace OVFL.ECS
{
    public class Systems
    {
        private readonly List<ISystem> _allSystems = new();
        private readonly List<ISetupSystem> _setupSystems = new();
        private readonly List<ITickSystem> _tickSystems = new();
        private readonly List<ICleanupSystem> _cleanupSystems = new();
        private readonly List<ITeardownSystem> _teardownSystems = new();

        /// <summary>
        /// System을 추가합니다
        /// </summary>
        public Systems Add(ISystem system)
        {
            _allSystems.Add(system);

            if (system is ISetupSystem setupSystem)
                _setupSystems.Add(setupSystem);

            if (system is ITickSystem tickSystem)
                _tickSystems.Add(tickSystem);

            if (system is ICleanupSystem cleanupSystem)
                _cleanupSystems.Add(cleanupSystem);

            if (system is ITeardownSystem teardownSystem)
                _teardownSystems.Add(teardownSystem);

            return this;
        }

        /// <summary>
        /// 모든 Setup System을 실행합니다 (초기화 시 한 번)
        /// </summary>
        public void Setup(Context context)
        {
            foreach (var system in _setupSystems)
            {
                system.Setup(context);
            }
        }

        /// <summary>
        /// 모든 Tick System을 실행합니다 (매 프레임)
        /// </summary>
        public void Tick(Context context)
        {
            foreach (var system in _tickSystems)
            {
                system.Tick(context);
            }
        }

        /// <summary>
        /// 모든 Cleanup System을 실행합니다 (Tick 이후)
        /// </summary>
        public void Cleanup(Context context)
        {
            foreach (var system in _cleanupSystems)
            {
                system.Cleanup(context);
            }
        }

        /// <summary>
        /// 모든 Teardown System을 실행합니다 (마무리 시 한 번)
        /// </summary>
        public void Teardown(Context context)
        {
            foreach (var system in _teardownSystems)
            {
                system.Teardown(context);
            }
        }
    }
}