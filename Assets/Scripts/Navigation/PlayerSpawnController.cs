using TCCGame.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TCCGame.Gameplay.Navigation
{
    [DefaultExecutionOrder(-8500)]
    public sealed class PlayerSpawnController : MonoBehaviour
    {
        private const string CenterDefaultAnchor = "CenterDefaultSpawn";
        private const string GateEntranceAnchor = "GateEntranceSpawn";

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad
        )]
        private static void PlaceInitialPlayer()
        {
            PlacePlayerInActiveScene();
        }

        public static void PlacePlayerInActiveScene()
        {
            PlayerInteractor player = Object.FindFirstObjectByType<
                PlayerInteractor
            >();

            if (player == null)
            {
                Debug.LogError(
                    "Navigation scene requires exactly one PlayerInteractor."
                );
                return;
            }

            string anchorName = GetAnchorName();
            GameObject anchor = GameObject.Find(anchorName);

            if (anchor == null)
            {
                Debug.LogError(
                    $"Navigation spawn anchor '{anchorName}' is missing."
                );
                return;
            }

            player.transform.position = anchor.transform.position;
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();

            if (body != null)
            {
                body.position = anchor.transform.position;
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
            }
        }

        private static string GetAnchorName()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            if (sceneName == SceneCatalog.Gate)
            {
                return GateEntranceAnchor;
            }

            NavigationSnapshot navigation =
                GameSessionProvider.Current.Navigation.Current;

            if (navigation.HasCenterEntryGate)
            {
                return $"CenterFrom{navigation.CenterEntryGate}Spawn";
            }

            return CenterDefaultAnchor;
        }
    }
}
