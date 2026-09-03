using System;
using TCCGame.Gameplay.Campaign;
using TCCGame.Gameplay.Navigation;

namespace TCCGame.Core.Persistence
{
    [Serializable]
    public sealed class NavigationSaveData
    {
        public bool initialized = true;
        public CityArea currentArea = CityArea.Center;
        public GateId activeGateId = GateId.North;
        public bool hasCenterEntryGate;
        public GateId centerEntryGate = GateId.North;

        public NavigationSnapshot ToSnapshot()
        {
            return new NavigationSnapshot(
                currentArea,
                activeGateId,
                hasCenterEntryGate,
                centerEntryGate
            );
        }
    }

    public static class NavigationSaveDataUtility
    {
        public static bool Normalize(GameSaveData saveData)
        {
            if (saveData == null)
            {
                throw new ArgumentNullException(nameof(saveData));
            }

            bool changed = false;

            if (saveData.navigation == null)
            {
                saveData.navigation = new NavigationSaveData();
                changed = true;
            }

            NavigationSaveData navigation = saveData.navigation;

            if (!navigation.initialized)
            {
                navigation.initialized = true;
                changed = true;
            }

            if (!Enum.IsDefined(typeof(CityArea), navigation.currentArea))
            {
                navigation.currentArea = CityArea.Center;
                changed = true;
            }

            if (!Enum.IsDefined(typeof(GateId), navigation.activeGateId))
            {
                navigation.activeGateId = GateId.North;
                changed = true;
            }

            if (!Enum.IsDefined(typeof(GateId), navigation.centerEntryGate))
            {
                navigation.centerEntryGate = GateId.North;
                navigation.hasCenterEntryGate = false;
                changed = true;
            }

            return changed;
        }
    }
}
