using TCCGame.Core;
using TCCGame.Core.Input;
using TCCGame.UI;
using UnityEngine;

/// <summary>Contextual tax preview; feedback never takes a modal or movement lock.</summary>
public sealed class CityTaxHud : MonoBehaviour
{
    [SerializeField] private TaxCollectorNpc collector;
    [SerializeField] private CityTaxHudView view;
    [SerializeField, Min(0.1f)] private float visibleDurationSeconds = 3.5f;
    private PlayerInteractor player;
    private float remaining;
    private bool collected;

    private void Awake() => view.SetVisibility(false, false);

    private void Update()
    {
        if (player == null) player = FindFirstObjectByType<PlayerInteractor>();
        bool blocked = UiMenuState.HasOpenMenu || (player != null && player.HasModalOpen);
        bool context = collector != null && collector.CanInteract && player != null &&
            ReferenceEquals(player.CurrentInteractable, collector);
        bool feedback = remaining > 0;
        view.SetVisibility(!blocked && (context || feedback), !blocked && feedback);
        if (blocked) return;
        if (context || feedback)
        {
            var session = GameSessionProvider.Current;
            int level = session.Buildings.GetLevel(BuildingType.LocalMarket);
            view.Present(level, session.Taxes.GetCollectibleAmount(),
                session.Buildings.GetProduction(BuildingType.LocalMarket), feedback && collected);
        }
        if (feedback) remaining = Mathf.Max(0, remaining - Time.unscaledDeltaTime);
    }

    public void ShowResult(TaxCollectionResult result)
    {
        view.PresentResult(result);
        collected = result.Status == TaxCollectionStatus.Collected;
        remaining = visibleDurationSeconds;
    }

    private void OnDisable()
    {
        remaining = 0;
        if (view != null) view.SetVisibility(false, false);
    }
}
