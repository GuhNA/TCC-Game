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
                wave.expiresUnixSeconds > clock.UnixSeconds)
            {
                return wave.adventurerIds.AsReadOnly();
            }

            List<string> candidates = eligibleAdventurerIds?
                .Select(id => id?.Trim())
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList() ?? new List<string>();

            wave.sequence = checked(wave.sequence + 1);
            wave.adventurerIds.Clear();
            int count = candidates.Count == 0
                ? 0
                : Math.Max(0, offerCount);
            var random = new Random(unchecked(
                wave.sequence * 486187739 ^ clock.UnixSeconds.GetHashCode()
            ));

            // Trabalhamos em ciclos embaralhados. Enquanto houver vagas, todas
            // as classes aparecem uma vez antes de uma classe poder se repetir.
            // Isso permite ondas maiores que o catálogo sem excluir classes já
            // recrutadas pelo jogador.
            var shuffledCycle = new List<string>(candidates);
            while (wave.adventurerIds.Count < count)
            {
                for (int index = shuffledCycle.Count - 1; index > 0; index--)
                {
                    int other = random.Next(index + 1);
                    (shuffledCycle[index], shuffledCycle[other]) =
                        (shuffledCycle[other], shuffledCycle[index]);
                }

                int remaining = count - wave.adventurerIds.Count;
                wave.adventurerIds.AddRange(
                    shuffledCycle.Take(Math.Min(
                        remaining,
                        shuffledCycle.Count
                    ))
                );
            }

            wave.expiresUnixSeconds = clock.UnixSeconds +
                Math.Max(1, durationSeconds);
            commit();
            return wave.adventurerIds.AsReadOnly();
        }

        /// <summary>
        /// Consome somente a vaga comprada. Outras ofertas da mesma classe
        /// continuam independentes e a onda vazia não é renovada antes do prazo.
        /// </summary>
        public void ClaimOffer(string adventurerDefinitionId)
        {
            string normalizedId = adventurerDefinitionId?.Trim();
            if (string.IsNullOrEmpty(normalizedId))
            {
                return;
            }

            int index = state.tavernRecruitmentWave.adventurerIds.IndexOf(
                normalizedId
            );
            if (index < 0)
            {
                return;
            }

            state.tavernRecruitmentWave.adventurerIds.RemoveAt(index);
            commit();
        }
    }
}
