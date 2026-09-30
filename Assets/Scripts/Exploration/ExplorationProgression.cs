using System;
using System.Collections.Generic;

namespace TCCGame.Gameplay.Exploration
{
    /// <summary>
    /// Contains the world-map route graph. An empty prerequisite list means the
    /// destination is directly reachable from Porto Azul at the start of a save.
    /// Changing this table changes progression without coupling it to Canvas code.
    /// </summary>
    public static class ExplorationProgression
    {
        private static readonly Dictionary<
            ExplorationEnvironment,
            ExplorationEnvironment[]
        > Prerequisites = new()
        {
            { ExplorationEnvironment.ColinasDeVento, Array.Empty<ExplorationEnvironment>() },
            { ExplorationEnvironment.FlorestaDasSombras, Array.Empty<ExplorationEnvironment>() },
            { ExplorationEnvironment.CavernaDeGelo, Array.Empty<ExplorationEnvironment>() },
            { ExplorationEnvironment.TorreAntiga, new[] { ExplorationEnvironment.ColinasDeVento } },
            { ExplorationEnvironment.VilaDoMar, new[] { ExplorationEnvironment.ColinasDeVento } },
            { ExplorationEnvironment.FlorestaEsmeralda, new[] { ExplorationEnvironment.FlorestaDasSombras } },
            { ExplorationEnvironment.MontanhasGelidas, new[] { ExplorationEnvironment.CavernaDeGelo } },
            { ExplorationEnvironment.ReinoReal, new[] { ExplorationEnvironment.FlorestaEsmeralda, ExplorationEnvironment.MontanhasGelidas } },
            { ExplorationEnvironment.IlhaVerde, new[] { ExplorationEnvironment.ReinoReal } },
            { ExplorationEnvironment.CavernaSombria, new[] { ExplorationEnvironment.VilaDoMar } },
            { ExplorationEnvironment.MontanhaDeFogo, new[] { ExplorationEnvironment.CavernaSombria } },
            { ExplorationEnvironment.DesertoDosVentos, new[] { ExplorationEnvironment.MontanhaDeFogo } },
            { ExplorationEnvironment.Santuario, new[] { ExplorationEnvironment.DesertoDosVentos, ExplorationEnvironment.TorreAntiga } },
            { ExplorationEnvironment.IlhasCoral, new[] { ExplorationEnvironment.VilaDoMar, ExplorationEnvironment.Santuario } }
        };

        public static bool IsUnlocked(
            ExplorationEnvironment environment,
            ICollection<ExplorationEnvironment> completed
        )
        {
            if (!Prerequisites.TryGetValue(environment, out var required))
            {
                return false;
            }

            foreach (ExplorationEnvironment prerequisite in required)
            {
                if (!completed.Contains(prerequisite))
                {
                    return false;
                }
            }

            return true;
        }

        public static string GetDisplayName(ExplorationEnvironment environment)
        {
            return environment switch
            {
                ExplorationEnvironment.ColinasDeVento => "Colinas do Vento",
                ExplorationEnvironment.FlorestaDasSombras => "Floresta das Sombras",
                ExplorationEnvironment.CavernaDeGelo => "Caverna do Gelo",
                ExplorationEnvironment.TorreAntiga => "Torre Antiga",
                ExplorationEnvironment.VilaDoMar => "Vila do Mar",
                ExplorationEnvironment.FlorestaEsmeralda => "Floresta Esmeralda",
                ExplorationEnvironment.MontanhasGelidas => "Montanhas Gélidas",
                ExplorationEnvironment.ReinoReal => "Reino Real",
                ExplorationEnvironment.IlhaVerde => "Ilha Verde",
                ExplorationEnvironment.CavernaSombria => "Caverna Sombria",
                ExplorationEnvironment.MontanhaDeFogo => "Montanha de Fogo",
                ExplorationEnvironment.DesertoDosVentos => "Deserto dos Ventos",
                ExplorationEnvironment.Santuario => "Santuário",
                ExplorationEnvironment.IlhasCoral => "Ilhas Coral",
                _ => environment.ToString()
            };
        }
    }
}
