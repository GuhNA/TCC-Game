using System;
using TCCGame.Gameplay.Campaign;

namespace TCCGame.Gameplay.Navigation
{
    public enum CityArea
    {
        Center,
        Gate
    }

    public enum TravelDestinationKind
    {
        Gate,
        Center
    }

    public readonly struct NavigationSnapshot
    {
        public CityArea CurrentArea { get; }
        public GateId ActiveGateId { get; }
        public bool HasCenterEntryGate { get; }
        public GateId CenterEntryGate { get; }

        public bool IsAtGate => CurrentArea == CityArea.Gate;

        public NavigationSnapshot(
            CityArea currentArea,
            GateId activeGateId,
            bool hasCenterEntryGate,
            GateId centerEntryGate
        )
        {
            CurrentArea = currentArea;
            ActiveGateId = activeGateId;
            HasCenterEntryGate = hasCenterEntryGate;
            CenterEntryGate = centerEntryGate;
        }
    }

    public readonly struct TravelDestination
    {
        public TravelDestinationKind Kind { get; }
        public GateId GateId { get; }

        public TravelDestination(
            TravelDestinationKind kind,
            GateId gateId
        )
        {
            Kind = kind;
            GateId = gateId;
        }
    }

    public static class GateDisplayNames
    {
        public static string Get(GateId gateId)
        {
            return gateId switch
            {
                GateId.North => "Norte",
                GateId.East => "Leste",
                GateId.South => "Sul",
                GateId.West => "Oeste",
                _ => throw new ArgumentOutOfRangeException(nameof(gateId))
            };
        }

        public static string GetUppercase(GateId gateId)
        {
            return Get(gateId).ToUpperInvariant();
        }
    }
}
