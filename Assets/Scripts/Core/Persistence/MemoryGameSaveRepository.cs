namespace TCCGame.Core.Persistence
{
    /// <summary>Transient session storage for tests that must not change player saves.</summary>
    public sealed class MemoryGameSaveRepository : IGameSaveRepository
    {
        private GameSaveData data;
        public MemoryGameSaveRepository(GameSaveData seed) => data = seed.Clone();
        public GameSaveData Load() => data.Clone();
        public void Save(GameSaveData value) => data = value.Clone();
    }
}
