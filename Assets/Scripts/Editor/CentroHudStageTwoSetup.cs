#if UNITY_EDITOR
using System.Linq;
using TMPro;
using TCCGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace TCCGame.Editor
{
    public static class CentroHudStageTwoSetup
    {
        [MenuItem("TCC Game/UI/Apply Centro HUD Stage 2")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Centro.unity", OpenSceneMode.Single);
            var collector = Object.FindFirstObjectByType<TaxCollectorNpc>();
            var previous = Object.FindFirstObjectByType<CityTaxHud>(FindObjectsInactive.Include);
            if (previous != null) Object.DestroyImmediate(previous.gameObject);
            var root = new GameObject("City Tax Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            root.GetComponent<Canvas>().sortingOrder = 20;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            var prototype = AssetDatabase.LoadAssetAtPath<GameObject>(CentroHudProposalSetup.PrefabPath);
            var content = Object.Instantiate(prototype.transform.Find("City Proposal").gameObject, root.transform, false);
            content.name = "Tax Content";
            Object.DestroyImmediate(content.transform.Find("City Gold").gameObject);
            Object.DestroyImmediate(content.transform.Find("City Prompt").gameObject);
            var view = content.AddComponent<CityTaxHudView>();
            var data = new SerializedObject(view);
            Bind(data, "widget", content.transform.Find("Treasury Widget").gameObject);
            Bind(data, "toast", content.transform.Find("Tax Toast").gameObject);
            Bind(data, "amount", Text(content, "Treasury Amount"));
            Bind(data, "rate", Text(content, "Treasury Rate"));
            Bind(data, "message", Text(content, "Toast Message"));
            Bind(data, "statusIcon", content.GetComponentsInChildren<Image>(true).Single(x => x.name == "Toast Check"));
            // Resolve the same theme used by the approved modal, independent of folder naming.
            data.FindProperty("theme").objectReferenceValue = AssetDatabase.LoadAssetAtPath<MedievalHudTheme>(TCCGame.Editor.UI.MedievalHudKit.ThemeAssetPath);
            data.ApplyModifiedPropertiesWithoutUndo();
            foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
            view.SetVisibility(false, false);
            var hud = root.AddComponent<CityTaxHud>();
            var controller = new SerializedObject(hud);
            Bind(controller, "view", view);
            controller.ApplyModifiedPropertiesWithoutUndo();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Resources/UI/CityTaxCanvas.prefab");
            Object.DestroyImmediate(root);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            // Scene references cannot be stored inside a prefab asset.
            var instanceData = new SerializedObject(instance.GetComponent<CityTaxHud>());
            Bind(instanceData, "collector", collector);
            instanceData.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.RecordPrefabInstancePropertyModifications(instance.GetComponent<CityTaxHud>());
            var npcData = new SerializedObject(collector);
            var oldAlert = npcData.FindProperty("alert").objectReferenceValue as AlertPopup;
            Bind(npcData, "hud", instance.GetComponent<CityTaxHud>());
            Bind(npcData, "alert", null);
            npcData.ApplyModifiedPropertiesWithoutUndo();
            if (oldAlert != null) Object.DestroyImmediate(oldAlert.gameObject);
            var gold = Object.FindFirstObjectByType<GoldBalancePresenter>(FindObjectsInactive.Include);
            gold.transform.SetParent(null, false);
            gold.transform.localScale = Vector3.one;
            // Restore Centro's zero-scaled nested canvas; display only its bound visual.
            Transform boundGold = gold.PixelGoldText.transform.parent;
            foreach (Transform child in gold.transform) child.gameObject.SetActive(child == boundGold);
            PrefabUtility.RecordPrefabInstancePropertyModifications(gold.transform);
            foreach (Transform child in gold.transform) PrefabUtility.RecordPrefabInstancePropertyModifications(child.gameObject);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Centro stage 2 applied: contextual taxes, readable feedback and prompts.");
        }

        private static TMP_Text Text(GameObject root, string name) => root.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == name);
        private static void Bind(SerializedObject data, string field, Object value) => data.FindProperty(field).objectReferenceValue = value;
    }
}
#endif
