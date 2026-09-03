using System;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Campaign;

namespace TCCGame.Gameplay.Navigation
{
    public sealed class NavigationService
    {
        private readonly GameSaveData state;
        private readonly Action commit;

        public NavigationSnapshot Current => state.navigation.ToSnapshot();

        public event Action<NavigationSnapshot> Changed;

        public NavigationService(GameSaveData state, Action commit)
        {
            this.state = state ??
                throw new ArgumentNullException(nameof(state));
            this.commit = commit ??
                throw new ArgumentNullException(nameof(commit));

            NavigationSaveDataUtility.Normalize(state);
        }

        public bool EnterGate(GateId gateId)
        {
            ValidateGate(gateId);

            NavigationSaveData navigation = state.navigation;

            if (navigation.currentArea == CityArea.Gate &&
                navigation.activeGateId == gateId)
            {
                return false;
            }

            navigation.currentArea = CityArea.Gate;
            navigation.activeGateId = gateId;
            PersistAndPublish();
            return true;
        }

        public bool ReturnToCenter()
        {
            NavigationSaveData navigation = state.navigation;

            if (navigation.currentArea != CityArea.Gate)
            {
                return false;
            }

            navigation.currentArea = CityArea.Center;
            navigation.hasCenterEntryGate = true;
            navigation.centerEntryGate = navigation.activeGateId;
            PersistAndPublish();
            return true;
        }

        public bool RecoverToCenter()
        {
            NavigationSaveData navigation = state.navigation;

            if (navigation.currentArea == CityArea.Center &&
                !navigation.hasCenterEntryGate)
            {
                return false;
            }

            navigation.currentArea = CityArea.Center;
            navigation.hasCenterEntryGate = false;
            PersistAndPublish();
            return true;
        }

        private void PersistAndPublish()
        {
            commit();
            Changed?.Invoke(Current);
        }

        private static void ValidateGate(GateId gateId)
        {
            if (!Enum.IsDefined(typeof(GateId), gateId))
            {
                throw new ArgumentOutOfRangeException(nameof(gateId));
            }
        }
    }
}
