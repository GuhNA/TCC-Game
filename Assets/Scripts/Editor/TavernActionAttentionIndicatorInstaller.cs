using TCCGame.Gameplay.Exploration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TavernActionAttentionIndicatorInstaller
{
    [MenuItem("TCC Game/Tavern/Connect Attention Indicator")]
    public static void Connect()
    {
        GameObject visual = GameObject.Find("AttentionExclamation");
        if (visual == null)
        {
            Debug.LogError("Could not find AttentionExclamation in the open scene.");
            return;
        }

        GameObject host = visual.transform.parent != null
            ? visual.transform.parent.gameObject
            : visual;
        TavernActionAttentionIndicator indicator = host.GetComponent<
            TavernActionAttentionIndicator>();
        if (indicator == null)
        {
            indicator = host.AddComponent<TavernActionAttentionIndicator>();
        }

        indicator.Configure(
            visual,
            Resources.Load<ExplorationEncounterCatalog>(
                ExplorationEncounterCatalog.ResourcesPath
            )
        );
        EditorUtility.SetDirty(indicator);
        EditorSceneManager.MarkSceneDirty(host.scene);
        Selection.activeGameObject = host;
        Debug.Log("Tavern attention indicator connected. Its default loot interval is 300 seconds (5 minutes).", host);
    }
}
