using System.Collections.Generic;
using TCCGame.Prototypes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TCCGame.Tests.Visual
{
    public abstract class CharacterDepthVisualScenario : IVisualReviewScenario
    {
        protected WorldPrototypeDepthSort Player;
        protected WorldPrototypeDepthSort Npc;
        public abstract string Id { get; }
        public abstract string DisplayName { get; }
        public abstract string ScenePath { get; }
        public string ReferenceImagePath => "";
        public IReadOnlyList<VisualReviewCase> Cases { get; } = new[]
        {
            new VisualReviewCase("player-behind", "Jogador acima dos pés do NPC — atrás", 1366, 768),
            new VisualReviewCase("player-in-front", "Jogador abaixo dos pés do NPC — na frente", 1366, 768)
        };

        protected abstract GameObject FindNpc();
        protected virtual Bounds? BackdropBounds => null;

        public VisualReviewContext PrepareScene()
        {
            foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                canvas.enabled = false;
            Player = Object.FindFirstObjectByType<PlayerInteractor>().GetComponent<WorldPrototypeDepthSort>();
            Npc = FindNpc().GetComponent<WorldPrototypeDepthSort>();
            var root = new GameObject("Character depth review", typeof(Canvas));
            return new VisualReviewContext(Camera.main, root.GetComponent<Canvas>(), root);
        }

        public void ConfigureCase(VisualReviewContext context, VisualReviewCase reviewCase)
        {
            float dy = reviewCase.Id == "player-behind" ? .30f : -.40f;
            Player.transform.position += Npc.GroundPosition + new Vector3(.18f, dy, 0) - Player.GroundPosition;
            Physics2D.SyncTransforms();
            foreach (var depth in Object.FindObjectsByType<WorldPrototypeDepthSort>(FindObjectsSortMode.None)) depth.Refresh();
            foreach (var view in Object.FindObjectsByType<WorldBuildingView>(FindObjectsSortMode.None)) view.Refresh();
            foreach (var prop in Object.FindObjectsByType<WorldPropOcclusion>(FindObjectsSortMode.None)) prop.Refresh();
            Vector3 center = Npc.GroundPosition + Vector3.up * .7f;
            if (BackdropBounds is Bounds area)
            {
                float halfWidth = 2.7f * reviewCase.Width / reviewCase.Height;
                center.x = Mathf.Clamp(center.x, area.min.x + halfWidth, area.max.x - halfWidth);
                center.y = Mathf.Clamp(center.y, area.min.y + 2.7f, area.max.y - 2.7f);
            }
            ProductionWorldCapture.PlaceCamera(context.Camera, center, 2.7f);
        }
    }

    public sealed class CentroCharacterDepthVisualScenario : CharacterDepthVisualScenario
    {
        public override string Id => "character-depth-centro";
        public override string DisplayName => "Profundidade — Centro";
        public override string ScenePath => "Assets/Scenes/Centro.unity";
        protected override GameObject FindNpc() => Object.FindFirstObjectByType<TaxCollectorNpc>().gameObject;
    }

    public sealed class GateCharacterDepthVisualScenario : CharacterDepthVisualScenario
    {
        public override string Id => "character-depth-gate";
        public override string DisplayName => "Profundidade — Portão";
        public override string ScenePath => "Assets/Scenes/Portão.unity";
        protected override Bounds? BackdropBounds => GameObject.Find("portao_0").GetComponent<SpriteRenderer>().bounds;
        protected override GameObject FindNpc() => Object.FindFirstObjectByType<NPCVenda>().gameObject;
    }

    public sealed class TavernCharacterDepthVisualScenario : CharacterDepthVisualScenario
    {
        public override string Id => "character-depth-tavern";
        public override string DisplayName => "Profundidade — Taverna";
        public override string ScenePath => "Assets/Scenes/Taverna.unity";
        protected override GameObject FindNpc()
        {
            // Edit-mode capture uses the same prefab instantiated by the runtime spawner.
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Aventureiros/Contratação/Arqueiro.prefab");
            var npc = Object.Instantiate(prefab);
            var adventurer = npc.GetComponent<Aventureiro>();
            adventurer.Configure(adventurer.charBase);
            var floor = GameObject.Find("taverna_0").GetComponent<SpriteRenderer>();
            npc.transform.position += floor.bounds.center - npc.GetComponent<WorldPrototypeDepthSort>().GroundPosition;
            return npc;
        }
    }
}
