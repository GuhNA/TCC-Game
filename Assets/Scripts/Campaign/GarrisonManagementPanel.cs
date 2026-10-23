using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TCCGame.Core;
using TCCGame.Core.Input;
using TCCGame.Gameplay.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TCCGame.UI;
using TCCGame.Core.Persistence;

namespace TCCGame.Gameplay.Campaign
{
    [DefaultExecutionOrder(-7800)]
    public sealed class GarrisonManagementPanel : MonoBehaviour
    {
        private static readonly UnitType[] UnitTypes =
        {
            UnitType.Warrior,
            UnitType.Archer,
            UnitType.Mage
        };

        private static GarrisonManagementPanel instance;

        private readonly List<TroopLocation> sendDestinations = new();
        private readonly List<Button> commandButtons = new();
        private GameObject panelRoot;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI summaryText;
        private TextMeshProUGUI reportText;
        private TextMeshProUGUI destinationText;
        private TextMeshProUGUI statusText;
        private Button acknowledgeButton;
        private PlayerInteractor lockedInteractor;
        private GateId currentGate;
        private int destinationIndex;
        private float refreshAt;
        private bool visible;
        private GarrisonHudView themedView;

        [SerializeField]
        private GameObject interfacePrefab;

        public static bool IsVisible => instance != null && instance.visible;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            EnsureInstance();
        }

        public static void Show(GameObject player)
        {
            if (player == null || !EnsureInstance())
            {
                return;
            }

            instance.Open(player);
        }

        private static bool EnsureInstance()
        {
            if (instance != null)
            {
                return true;
            }

            instance = FindFirstObjectByType<GarrisonManagementPanel>();
            if (instance != null)
            {
                return true;
            }

            GameObject panelObject = new GameObject("Garrison Management Panel");
            instance = panelObject.AddComponent<GarrisonManagementPanel>();
            DontDestroyOnLoad(panelObject);
            return true;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadInterface();
        }

        private void Update()
        {
            if (!visible)
            {
                return;
            }

            if (GameInput.WasCancelPressedThisFrame())
            {
                Close();
                return;
            }

            if (Time.unscaledTime >= refreshAt)
            {
                Refresh();
                refreshAt = Time.unscaledTime + 0.25f;
            }
        }

        private void OnDestroy()
        {
            UiMenuState.SetOpen(this, false);
            ReleasePlayerLock();
            if (instance == this)
            {
                instance = null;
            }
        }

        private void Open(GameObject player)
        {
            NavigationSnapshot navigation =
                GameSessionProvider.Current.Navigation.Current;
            if (visible || UiMenuState.HasOpenMenu || !navigation.IsAtGate)
            {
                return;
            }

            lockedInteractor = player.GetComponent<PlayerInteractor>();
            if (lockedInteractor == null)
            {
                return;
            }

            currentGate = navigation.ActiveGateId;
            sendDestinations.Clear();
            sendDestinations.Add(TroopLocation.Reserve);
            foreach (GateId gateId in Enum.GetValues(typeof(GateId)))
            {
                if (gateId != currentGate)
                {
                    sendDestinations.Add(TroopLocation.Gate(gateId));
                }
            }

            destinationIndex = 0;
            statusText.text = string.Empty;
            visible = true;
            panelRoot.SetActive(true);
            UiMenuState.SetOpen(this, true);
            // Strategic time keeps advancing while the captain panel is open,
            // so transfers and wave countdowns remain observable in real time.
            lockedInteractor.AcquireModalLock(this);
            Refresh();
            EventSystem.current?.SetSelectedGameObject(null);
        }

        private void Close()
        {
            if (!visible)
            {
                return;
            }

            visible = false;
            UiMenuState.SetOpen(this, false);
            panelRoot.SetActive(false);
            ReleasePlayerLock();
        }

        private void ReleasePlayerLock()
        {
            if (lockedInteractor != null)
            {
                lockedInteractor.ReleaseModalLock(this);
                lockedInteractor = null;
            }
        }

        private void CycleDestination()
        {
            if (sendDestinations.Count == 0)
            {
                return;
            }

            destinationIndex = (destinationIndex + 1) % sendDestinations.Count;
            RefreshDestination();
        }

        private void AcknowledgeReport()
        {
            BattleReportSnapshot? report = GameSessionProvider.Current
                .Campaign.GetLatestBattleReport(
                    currentGate,
                    onlyUnacknowledged: true
                );
            if (!report.HasValue)
            {
                return;
            }

            GameSessionProvider.Current.Battles.AcknowledgeReport(
                report.Value.WaveSequence
            );
            statusText.text = "Relatório confirmado.";
            Refresh();
        }

        private void Receive(UnitType unitType, int quantity)
        {
            Dispatch(
                TroopLocation.Reserve,
                TroopLocation.Gate(currentGate),
                unitType,
                quantity
            );
        }

        private void Send(UnitType unitType, int quantity)
        {
            Dispatch(
                TroopLocation.Gate(currentGate),
                sendDestinations[destinationIndex],
                unitType,
                quantity
            );
        }

        private void Dispatch(
            TroopLocation source,
            TroopLocation destination,
            UnitType unitType,
            int quantity
        )
        {
            if (GameSessionProvider.Current.Campaign.Outcome !=
                CampaignOutcome.Ongoing)
            {
                statusText.text = "A campanha foi encerrada.";
                return;
            }

            TroopDispatchResult result =
                GameSessionProvider.Current.TroopDeployment.Dispatch(
                    source,
                    destination,
                    unitType,
                    quantity
                );

            statusText.text = result.Status switch
            {
                TroopDispatchStatus.Dispatched =>
                    $"{quantity} {GetUnitName(unitType, quantity)} em marcha.",
                TroopDispatchStatus.InsufficientTroops =>
                    "Tropas insuficientes na origem.",
                TroopDispatchStatus.InvalidQuantity =>
                    "Quantidade inválida.",
                TroopDispatchStatus.CampaignEnded =>
                    "A campanha foi encerrada.",
                _ => "Este deslocamento não é permitido."
            };
            Refresh();
        }

        private void Refresh()
        {
            TroopDeploymentService deployment =
                GameSessionProvider.Current.TroopDeployment;
            titleText.text =
                $"CAPITÃO — PORTÃO {GateDisplayNames.GetUppercase(currentGate)}";
            RefreshReport();
            summaryText.text = themedView != null ? BuildTransitSummary(deployment) :
                $"RESERVA   G {deployment.GetReserveCount(UnitType.Warrior)}  " +
                $"A {deployment.GetReserveCount(UnitType.Archer)}  " +
                $"M {deployment.GetReserveCount(UnitType.Mage)}\n" +
                $"GUARNIÇÃO  G {deployment.GetGateCount(currentGate, UnitType.Warrior)}  " +
                $"A {deployment.GetGateCount(currentGate, UnitType.Archer)}  " +
                $"M {deployment.GetGateCount(currentGate, UnitType.Mage)}\n" +
                BuildTransitSummary(deployment);
            bool campaignActive = GameSessionProvider.Current.Campaign.Outcome ==
                CampaignOutcome.Ongoing;
            foreach (Button button in commandButtons)
            {
                button.interactable = campaignActive;
            }
            if (themedView != null)
            {
                var session = GameSessionProvider.Current;
                var gate = session.Campaign.GetGate(currentGate);
                var threat = session.Threats.BuildGateCard(currentGate);
                themedView.SetStrategicState(gate.CurrentHealth, GateSaveData.DefaultHealth,
                    threat.Threat, threat.Countdown);
                for (int i = 0; i < UnitTypes.Length; i++)
                    themedView.SetUnit(i, deployment.GetGateCount(currentGate, UnitTypes[i]),
                        deployment.GetReserveCount(UnitTypes[i]), campaignActive);
            }
            RefreshDestination();
        }

        private void RefreshReport()
        {
            CampaignService campaign = GameSessionProvider.Current.Campaign;
            BattleReportSnapshot? report = campaign.GetLatestBattleReport(
                currentGate
            );
            if (!report.HasValue)
            {
                reportText.text = "Nenhum relatório de batalha neste portão.";
                acknowledgeButton.gameObject.SetActive(false);
                return;
            }

            BattleReportSnapshot value = report.Value;
            WaveSnapshot? wave = campaign.GetWave(value.WaveSequence);
            string advantage = string.Empty;
            if (wave.HasValue)
            {
                var initialGate = new GateStateSnapshot(
                    value.GateId,
                    value.InitialGateHealth,
                    value.InitialWarriors,
                    value.InitialArchers,
                    value.InitialMages,
                    0
                );
                advantage = "\n" +
                    ThreatForecastService.BuildAdvantageExplanation(
                        initialGate,
                        wave.Value
                    );
            }

            string outcome = value.Outcome == BattleOutcome.Breached
                ? "PORTÃO ROMPIDO"
                : "ONDA REPELIDA";
            reportText.text =
                $"ONDA {value.WaveSequence} — {outcome}   " +
                $"Defesa {value.EffectiveDefense:0.#} / Ameaça {value.ThreatPower}\n" +
                $"G {value.InitialWarriors}→{value.SurvivingWarriors} (-{value.LostWarriors})   " +
                $"A {value.InitialArchers}→{value.SurvivingArchers} (-{value.LostArchers})   " +
                $"M {value.InitialMages}→{value.SurvivingMages} (-{value.LostMages})\n" +
                $"Portão {value.InitialGateHealth}→{value.FinalGateHealth} " +
                $"(-{value.GateDamage})   Espólio +{value.Spoils} ouro" +
                advantage;
            acknowledgeButton.gameObject.SetActive(
                !value.ReportAcknowledged
            );
        }

        private string BuildTransitSummary(TroopDeploymentService deployment)
        {
            TroopTransferSnapshot[] relevant = deployment.InTransit
                .Where(transfer =>
                    transfer.Source.Kind == TroopLocationKind.Gate &&
                    transfer.Source.GateId == currentGate ||
                    transfer.Destination.Kind == TroopLocationKind.Gate &&
                    transfer.Destination.GateId == currentGate)
                .ToArray();
            if (relevant.Length == 0)
            {
                return "EM TRÂNSITO  nenhum";
            }

            double elapsed = GameSessionProvider.Current.Campaign
                .ElapsedStrategicSeconds;
            return "EM TRÂNSITO  " + string.Join(", ", relevant.Select(
                transfer =>
                    $"{transfer.Quantity} {GetUnitAbbreviation(transfer.UnitType)} " +
                    $"{GetLocationName(transfer.Source)} → " +
                    $"{GetLocationName(transfer.Destination)} " +
                    $"({Math.Max(0d, transfer.ArrivalStrategicSeconds - elapsed):0.0}s)"
            ));
        }

        private static string GetLocationName(TroopLocation location)
        {
            return location.Kind == TroopLocationKind.Reserve
                ? "Reserva"
                : GateDisplayNames.Get(location.GateId);
        }

        private void RefreshDestination()
        {
            if (sendDestinations.Count == 0)
            {
                return;
            }

            TroopLocation destination = sendDestinations[destinationIndex];
            destinationText.text = destination.Kind == TroopLocationKind.Reserve
                ? "Destino dos envios: Reserva"
                : $"Destino dos envios: Portão {GateDisplayNames.Get(destination.GateId)}";
        }

        private void LoadInterface()
        {
            interfacePrefab ??= Resources.Load<GameObject>(
                "UI/GarrisonManagementCanvas"
            );
            if (interfacePrefab == null)
            {
                Debug.LogError(
                    "GarrisonManagementPanel requires " +
                    "Resources/UI/GarrisonManagementCanvas.prefab."
                );
                enabled = false;
                return;
            }

            GameObject instance = Instantiate(interfacePrefab, transform);
            themedView = instance.GetComponentInChildren<GarrisonHudView>(true);
            panelRoot = FindChild(instance, "Garrison Dialog");
            titleText = FindText(instance, "Title");
            reportText = FindText(instance, "Battle Report");
            summaryText = FindText(instance, "Summary");
            destinationText = FindText(instance, "Destination");
            statusText = FindText(instance, "Status");
            acknowledgeButton = FindButton(instance, "Acknowledge Report");
            Button changeDestination = FindButton(instance, "Change Destination");
            Button closeButton = FindButton(instance, "Close");

            if (panelRoot == null || titleText == null || reportText == null ||
                summaryText == null || destinationText == null ||
                statusText == null || acknowledgeButton == null ||
                changeDestination == null || closeButton == null)
            {
                Debug.LogError("GarrisonManagementCanvas prefab is incomplete.");
                enabled = false;
                return;
            }

            acknowledgeButton.onClick.AddListener(AcknowledgeReport);
            changeDestination.onClick.AddListener(CycleDestination);
            closeButton.onClick.AddListener(Close);
            commandButtons.Clear();
            commandButtons.Add(changeDestination);

            foreach (UnitType unitType in UnitTypes)
            {
                AddCommand(instance, $"Receive 1 {unitType}", () => Receive(unitType, 1));
                AddCommand(instance, $"Receive 10 {unitType}", () => Receive(unitType, 10));
                AddCommand(instance, $"Send 1 {unitType}", () => Send(unitType, 1));
                AddCommand(instance, $"Send 10 {unitType}", () => Send(unitType, 10));
            }

            panelRoot.SetActive(false);
        }

        private void AddCommand(
            GameObject root,
            string name,
            UnityEngine.Events.UnityAction action
        )
        {
            Button button = FindButton(root, name);
            if (button == null)
            {
                Debug.LogError($"Missing garrison button: {name}.");
                return;
            }

            button.onClick.AddListener(action);
            commandButtons.Add(button);
        }

        private static TextMeshProUGUI FindText(GameObject root, string name)
        {
            GameObject child = FindChild(root, name);
            return child == null ? null : child.GetComponent<TextMeshProUGUI>();
        }

        private static Button FindButton(GameObject root, string name)
        {
            GameObject child = FindChild(root, name);
            return child == null ? null : child.GetComponent<Button>();
        }

        private static GameObject FindChild(GameObject root, string name)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        private static string GetUnitName(UnitType unitType, int quantity)
        {
            return unitType switch
            {
                UnitType.Warrior => quantity == 1 ? "guerreiro" : "guerreiros",
                UnitType.Archer => quantity == 1 ? "arqueiro" : "arqueiros",
                UnitType.Mage => quantity == 1 ? "mago" : "magos",
                _ => "tropas"
            };
        }

        private static string GetUnitAbbreviation(UnitType unitType) =>
            unitType switch
            {
                UnitType.Warrior => "G",
                UnitType.Archer => "A",
                UnitType.Mage => "M",
                _ => "?"
            };
    }
}
