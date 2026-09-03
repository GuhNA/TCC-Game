using System.Collections.Generic;

namespace TCCGame.Core.Input
{
    /// <summary>
    /// Registro compartilhado dos paineis que ocupam a atencao do jogador.
    /// UI decorativa, como alertas breves, nao deve ser registrada aqui.
    /// </summary>
    public static class UiMenuState
    {
        private static readonly HashSet<object> openMenus = new();

        public static bool HasOpenMenu => openMenus.Count > 0;

        public static void SetOpen(object owner, bool isOpen)
        {
            if (owner == null)
            {
                return;
            }

            if (isOpen)
            {
                openMenus.Add(owner);
                return;
            }

            openMenus.Remove(owner);
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(
            UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration
        )]
        private static void Reset()
        {
            openMenus.Clear();
        }
    }
}
