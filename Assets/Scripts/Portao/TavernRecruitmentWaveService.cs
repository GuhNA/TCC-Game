using System;
using System.Collections.Generic;
using System.Linq;
using TCCGame.Core.Persistence;
using TCCGame.Core.Time;

namespace TCCGame.Gameplay.Recruitment
{
    /// <summary>
    /// Controla a onda persistente de aventureiros da taverna. Uma onda nova
    /// só é criada quando a cena da taverna solicita sua abertura e a anterior
    /// já expirou. Portanto, vencer o prazo dentro da cena nunca atualiza os
    /// NPCs imediatamente: é necessário sair e entrar novamente.
    /// </summary>
    public sealed class TavernRecruitmentWaveService
    {
        private readonly GameSaveData state;
        private readonly IClock clock;
        private readonly Action commit;

        public TavernRecruitmentWaveService(
            GameSaveData state,
            IClock clock,
            Action commit
        )
        {
            this.state = state ??
                throw new ArgumentNullException(nameof(state));
            this.clock = clock ??
                throw new ArgumentNullException(nameof(clock));
            this.commit = commit ??
                throw new ArgumentNullException(nameof(commit));
            state.tavernRecruitmentWave ??=
                new TavernRecruitmentWaveSaveData();
            state.tavernRecruitmentWave.adventurerIds ??= new List<string>();
            state.playerInventory ??= new PlayerInventorySaveData();
            state.playerInventory.recruitedAdventurerIds ??=
                new List<string>();
        }

        public bool IsCurrentWaveExpired =>
            state.tavernRecruitmentWave.expiresUnixSeconds > 0 &&
            clock.UnixSeconds >=
                state.tavernRecruitmentWave.expiresUnixSeconds;

        public long RemainingSeconds => Math.Max(
            0,
            state.tavernRecruitmentWave.expiresUnixSeconds -
                clock.UnixSeconds
        );

        /// <summary>
        /// Retorna a onda em andamento ou cria uma nova. O prazo começa aqui,
        /// que corresponde ao momento em que a cena foi carregada e a onda se
        /// tornou visível ao jogador.
        /// </summary>
        public IReadOnlyList<string> EnterTavern(
            IEnumerable<string> eligibleAdventurerIds,
            int offerCount,
            int durationSeconds
        )
        {
            if (offerCount <= 0)
            {
                return Array.Empty<string>();
            }

            TavernRecruitmentWaveSaveData wave =
                state.tavernRecruitmentWave;
            if (!IsCurrentWaveExpired &&
                wave.expiresUnixSeconds > clock.UnixSeconds &&
                wave.adventurerIds.Count > 0)
            {
                return wave.adventurerIds.AsReadOnly();
            }

            List<string> candidates = eligibleAdventurerIds?
                .Select(id => id?.Trim())
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .Where(id => !state.playerInventory.recruitedAdventurerIds
                    .Contains(id))
                .ToList() ?? new List<string>();

            wave.sequence = checked(wave.sequence + 1);
            wave.adventurerIds.Clear();
            int count = Math.Clamp(offerCount, 0, candidates.Count);
            var random = new Random(unchecked(
                wave.sequence * 486187739 ^ clock.UnixSeconds.GetHashCode()
            ));
            for (int index = candidates.Count - 1; index > 0; index--)
            {
                int other = random.Next(index + 1);
                (candidates[index], candidates[other]) =
                    (candidates[other], candidates[index]);
            }

            wave.adventurerIds.AddRange(candidates.Take(count));
            wave.expiresUnixSeconds = clock.UnixSeconds +
                Math.Max(1, durationSeconds);
            commit();
            return wave.adventurerIds.AsReadOnly();
        }
    }
}
