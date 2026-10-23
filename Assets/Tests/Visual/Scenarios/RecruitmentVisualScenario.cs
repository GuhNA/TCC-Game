using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TCCGame.Core;
using UnityEngine;

namespace TCCGame.Tests.Visual
{
    public sealed class RecruitmentVisualScenario : IVisualReviewScenario
    {
        private static readonly IReadOnlyList<VisualReviewCase> ReviewCases =
            new List<VisualReviewCase>
            {
                new("normal", "Normal", 1920, 1080),
                new("normal-compact", "Normal compact", 1366, 768),
                new("out-of-stock", "Out of stock", 1920, 1080),
                new("insufficient-gold", "Insufficient gold", 1920, 1080),
                new("purchase-success", "Purchase success", 1920, 1080)
            };

        public string Id => "recruitment";
        public string DisplayName => "Gate recruitment HUD";
        public string ScenePath => "Assets/Scenes/Portão.unity";
        public string ReferenceImagePath =>
            "Design/HUD/RecruitmentHudVisualDirection.png";
        public IReadOnlyList<VisualReviewCase> Cases => ReviewCases;

        public VisualReviewContext PrepareScene()
        {
            NPCVenda vendor = UnityEngine.Object.FindFirstObjectByType<NPCVenda>(
                FindObjectsInactive.Include
            );
            Camera camera = UnityEngine.Object.FindFirstObjectByType<Camera>(
                FindObjectsInactive.Include
            );

            if (vendor == null || vendor.popup == null || camera == null)
            {
                throw new InvalidOperationException(
                    "Recruitment visual scenario could not find its vendor, popup, or camera."
                );
            }

            GameObject root = vendor.popup;
            root.SetActive(true);
            Canvas canvas = root.GetComponent<Canvas>();
            if (canvas == null)
            {
                throw new InvalidOperationException(
                    "Recruitment popup does not have a root Canvas."
                );
            }

            Canvas[] canvases = UnityEngine.Object.FindObjectsByType<Canvas>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );
            foreach (Canvas other in canvases)
            {
                bool belongsToTarget = other == canvas ||
                    other.transform.IsChildOf(root.transform);
                if (!belongsToTarget)
                {
                    other.enabled = false;
                }
            }

            camera.gameObject.SetActive(true);
            camera.enabled = true;
            return new VisualReviewContext(camera, canvas, root);
        }

        public void ConfigureCase(
            VisualReviewContext context,
            VisualReviewCase reviewCase
        )
        {
            int money = 1000;
            string feedback = "Selecione a quantidade e confirme a contratação.";
            UnitState warrior = new UnitState(12, 8, 3);
            UnitState archer = new UnitState(15, 10, 5);
            UnitState mage = new UnitState(8, 5, 8);

            switch (reviewCase.Id)
            {
                case "out-of-stock":
                    warrior = new UnitState(0, 50, 3);
                    archer = new UnitState(0, 50, 5);
                    mage = new UnitState(0, 40, 8);
                    money = 145319;
                    feedback = "Novos recrutas chegarão no próximo reestoque.";
                    break;

                case "insufficient-gold":
                    money = 2;
                    feedback = "Ouro insuficiente para concluir a contratação.";
                    break;

                case "purchase-success":
                    mage = new UnitState(7, 6, 8);
                    money = 992;
                    feedback = "Compra realizada, magos disponíveis: 7";
                    break;
            }

            RecruitmentRowView[] rows = context.TargetRoot
                .GetComponentsInChildren<RecruitmentRowView>(true);
            Apply(rows, UnitType.Warrior, warrior, money);
            Apply(rows, UnitType.Archer, archer, money);
            Apply(rows, UnitType.Mage, mage, money);

            FindText(context.TargetRoot, "Gold").text = $"Ouro: {money}";
            FindText(context.TargetRoot, "Timer").text =
                "Novos recrutas em 05:30:00";
            FindText(context.TargetRoot, "Feedback").text = feedback;

            context.TargetRoot.SetActive(true);
            Canvas.ForceUpdateCanvases();
        }

        private static void Apply(
            IEnumerable<RecruitmentRowView> rows,
            UnitType type,
            UnitState state,
            int money
        )
        {
            RecruitmentRowView row = rows.Single(item => item.UnitType == type);
            row.Refresh(state.Available, state.Owned, state.Price, money);
            row.ResetSelection();
        }

        private static TMP_Text FindText(GameObject root, string name)
        {
            return root.GetComponentsInChildren<TMP_Text>(true)
                .Single(text => text.name == name);
        }

        private readonly struct UnitState
        {
            public UnitState(int available, int owned, int price)
            {
                Available = available;
                Owned = owned;
                Price = price;
            }

            public int Available { get; }
            public int Owned { get; }
            public int Price { get; }
        }
    }
}
