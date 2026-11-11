using System;
using System.Collections.Generic;
using TMPro;
using TCCGame.Core;
using TCCGame.Core.Input;
using TCCGame.Core.Persistence;
using TCCGame.Core.Time;
using TCCGame.UI;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TCCGame.Tests.Visual
{
    public sealed class TccEvidenceCaptureScenario : IVisualReviewScenario
    {
        private const long StartTime = 1_700_000_000;
        private TMP_Text goldText;
        private int initialBalance;
        private int initialTaxes;
        private int reopenedBalance;
        private int reopenedTaxes;
        private int finalTaxes;
        private TaxCollectionResult collection;

        public string Id => "tcc-retomada-2026-11-10";
        public string DisplayName => "TCC — retomada e coleta com relógio controlado";
        public string ScenePath => "Assets/Scenes/Centro.unity";
        public string ReferenceImagePath => string.Empty;

        public IReadOnlyList<VisualReviewCase> Cases { get; } = new[]
        {
            new VisualReviewCase("01-antes", "Estado no salvamento", 1600, 900),
            new VisualReviewCase("02-retomada", "Retomada após seis minutos simulados", 1600, 900),
            new VisualReviewCase("03-coleta", "Saldo após a coleta", 1600, 900)
        };

        public VisualReviewContext PrepareScene()
        {
            CalculateStates();
            foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                canvas.enabled = false;
            }

            var taxHud = Object.FindFirstObjectByType<CityTaxHud>(FindObjectsInactive.Include);
            GameObject root = taxHud.gameObject;
            var gold = Object.FindFirstObjectByType<GoldBalancePresenter>(FindObjectsInactive.Include);
            var goldCopy = Object.Instantiate(gold.PixelGoldText.transform.parent.gameObject, root.transform, false);
            goldCopy.SetActive(true);
            goldText = goldCopy.GetComponentInChildren<TMP_Text>(true);

            var collector = Object.FindFirstObjectByType<TaxCollectorNpc>();
            var player = Object.FindFirstObjectByType<PlayerInteractor>();
            Vector3 position = collector.transform.position;
            player.transform.position = position + new Vector3(-0.8f, -0.3f, 0f);
            Camera camera = Camera.main;
            ProductionWorldCapture.PlaceCamera(camera, position + new Vector3(-0.7f, 1.4f, 0f), 5.5f);
            return new VisualReviewContext(camera, root.GetComponent<Canvas>(), root);
        }

        public void ConfigureCase(VisualReviewContext context, VisualReviewCase reviewCase)
        {
            bool before = reviewCase.Id == "01-antes";
            bool collected = reviewCase.Id == "03-coleta";
            int available = before ? initialTaxes : collected ? finalTaxes : reopenedTaxes;
            int balance = before ? initialBalance : collected ? collection.TotalMoney : reopenedBalance;
            var view = context.TargetRoot.GetComponentInChildren<CityTaxHudView>(true);
            view.Present(3, available, 20, collected);
            if (collected)
            {
                view.PresentResult(collection);
            }
            view.SetVisibility(true, collected);
            goldText.text = CityTaxHudView.Money(balance);
        }

        private void CalculateStates()
        {
            var market = AssetDatabase.LoadAssetAtPath<BuildingDefinition>("Assets/Scriptable/Buildings/LocalMarket.asset");
            string prefix = $"tcc.evidence.{Guid.NewGuid():N}.";
            try
            {
                GameSaveData initial = GameSaveData.CreateDefault();
                initial.money = 1000;
                initial.SetBuildingLevel(BuildingType.LocalMarket, 3);
                new GameSaveRepository(prefix).Save(initial);

                var before = new GameSession(new GameSaveRepository(prefix), new FixedClock(StartTime));
                before.Buildings.RegisterDefinition(market);
                before.Taxes.SaveCurrentAccrual();
                initialBalance = before.Economy.Money;
                initialTaxes = before.Taxes.GetCollectibleAmount();

                var reopened = new GameSession(new GameSaveRepository(prefix), new FixedClock(StartTime + 360));
                reopened.Buildings.RegisterDefinition(market);
                reopenedBalance = reopened.Economy.Money;
                reopenedTaxes = reopened.Taxes.GetCollectibleAmount();
                collection = reopened.Taxes.CollectTaxes();
                finalTaxes = reopened.Taxes.GetCollectibleAmount();

                if (initialTaxes != 0 || reopenedTaxes != 120 || collection.Amount != 120 || collection.TotalMoney != 1120 || finalTaxes != 0)
                {
                    throw new InvalidOperationException("Valores da sequência diferentes do cenário documentado.");
                }
            }
            finally
            {
                PlayerPrefs.DeleteKey(prefix + GameSaveRepository.SaveKey);
                PlayerPrefs.DeleteKey(prefix + GameSaveRepository.CorruptSaveKey);
                PlayerPrefs.DeleteKey(prefix + GameSaveRepository.LegacyCitySaveKey);
                PlayerPrefs.Save();
            }
        }

        private sealed class FixedClock : IClock
        {
            public FixedClock(long unixSeconds) => UnixSeconds = unixSeconds;
            public long UnixSeconds { get; }
        }
    }
}
