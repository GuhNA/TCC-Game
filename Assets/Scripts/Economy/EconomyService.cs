using System;
using TCCGame.Core.Persistence;

namespace TCCGame.Gameplay.Economy
{
    public sealed class EconomyService
    {
        private readonly GameSaveData state;
        private readonly Action commit;

        public int Money => state.money;

        public event Action<int> MoneyChanged;

        public EconomyService(GameSaveData state, Action commit)
        {
            this.state = state ??
                throw new ArgumentNullException(nameof(state));
            this.commit = commit ??
                throw new ArgumentNullException(nameof(commit));
        }

        public bool CanAfford(int amount)
        {
            return amount >= 0 && state.money >= amount;
        }

        public void AddMoney(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            int previousMoney = state.money;

            try
            {
                state.money = checked(state.money + amount);
                commit();
            }
            catch
            {
                state.money = previousMoney;
                throw;
            }

            PublishChange();
        }

        internal void DebitWithoutCommit(int amount)
        {
            if (!CanAfford(amount))
            {
                throw new InvalidOperationException(
                    "Cannot debit more money than is available."
                );
            }

            state.money -= amount;
        }

        internal void RestoreMoney(int money)
        {
            state.money = money;
        }

        internal void PublishChange()
        {
            MoneyChanged?.Invoke(state.money);
        }
    }
}
