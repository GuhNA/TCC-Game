using TCCGame.Core.Persistence;
using TCCGame.Core.Time;
using TCCGame.Gameplay.City;
using TCCGame.Gameplay.Economy;
using TCCGame.Gameplay.Player;
using TCCGame.Gameplay.Recruitment;
using UnityEngine;

namespace TCCGame.Core
{
    public sealed class GameSession
    {
        private readonly IGameSaveRepository repository;

        internal GameSaveData State { get; }

        public EconomyService Economy { get; }
        public BuildingService Buildings { get; }
        public RecruitmentService Recruitment { get; }
        public PlayerInventoryService Inventory { get; }
        public TaxService Taxes { get; }

        public GameSession(
            IGameSaveRepository repository,
            IClock clock
        )
        {
            this.repository = repository ??
                throw new System.ArgumentNullException(nameof(repository));
            State = repository.Load();
            Economy = new EconomyService(State, Commit);
            Buildings = new BuildingService(State, Economy, Commit);
            Recruitment = new RecruitmentService(
                State,
                Economy,
                Buildings,
                clock,
                Commit
            );
            Inventory = new PlayerInventoryService(
                State,
                Economy,
                Commit
            );
            Taxes = new TaxService(
                State,
                Economy,
                Buildings,
                clock,
                Commit
            );
        }

        private void Commit()
        {
            repository.Save(State);
        }
    }

    public static class GameSessionProvider
    {
        private static GameSession current;

        public static GameSession Current =>
            current ??= new GameSession(
                new GameSaveRepository(),
                new SystemClock()
            );

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration
        )]
        public static void Reset()
        {
            current = null;
        }
    }
}
