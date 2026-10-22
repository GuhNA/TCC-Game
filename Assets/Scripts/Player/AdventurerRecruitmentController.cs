using TCCGame.Core;
using TCCGame.Gameplay.Player;
using UnityEngine;

[RequireComponent(typeof(PlayerInteractor))]
public sealed class AdventurerRecruitmentController : MonoBehaviour,
    IModalInteractionOwner
{
    [SerializeField]
    private AdventurerRecruitmentPanel panel;

    [SerializeField]
    private AlertPopup alert;

    private PlayerInteractor interactor;
    private Aventureiro selectedAdventurer;
    private bool ownsModalLock;

    public Aventureiro SelectedAdventurer => selectedAdventurer;

    private void Awake()
    {
        interactor = GetComponent<PlayerInteractor>();
    }

    private void OnEnable()
    {
        if (panel != null)
        {
            panel.PurchaseRequested += PurchaseSelected;
        }
    }

    private void OnDisable()
    {
        if (panel != null)
        {
            panel.PurchaseRequested -= PurchaseSelected;
        }

        CloseCurrent();
    }

    public void Open(Aventureiro target)
    {
        if (target == null || target.charBase == null || panel == null)
        {
            return;
        }

        if (ownsModalLock && selectedAdventurer == target)
        {
            Close(target);
            return;
        }

        selectedAdventurer?.SetBeingInspected(false);
        selectedAdventurer = target;
        selectedAdventurer.SetBeingInspected(true);
        string suggestedName = GameSessionProvider.Current.Inventory
            .GetNextDefaultAdventurerName(
                target.charBase.PersistenceId,
                target.charBase.name
            );
        panel.Show(CreateViewData(target, suggestedName));

        if (!ownsModalLock)
        {
            interactor.AcquireModalLock(this);
            ownsModalLock = true;
        }
    }

    public void Close(Aventureiro target)
    {
        if (selectedAdventurer == target)
        {
            CloseCurrent();
        }
    }

    public void CloseModal()
    {
        CloseCurrent();
    }

    public void PurchaseSelected()
    {
        if (selectedAdventurer == null ||
            selectedAdventurer.charBase == null)
        {
            CloseCurrent();
            alert?.Show("Aventureiro inválido");
            return;
        }

        Aventureiro purchasedOffer = selectedAdventurer;
        Char_base definition = purchasedOffer.charBase;
        AdventurerRecruitmentStatus result =
            GameSessionProvider.Current.Inventory.TryRecruitAdventurer(
                definition.PersistenceId,
                definition.name,
                panel?.AdventurerName,
                definition.preco,
                out _
            );

        if (result == AdventurerRecruitmentStatus.Recruited)
        {
            GameSessionProvider.Current.TavernRecruitment.ClaimOffer(
                definition.PersistenceId
            );
        }

        CloseCurrent();
        if (result == AdventurerRecruitmentStatus.Recruited &&
            purchasedOffer != null)
        {
            purchasedOffer.gameObject.SetActive(false);
        }
        alert?.Show(MessageFor(result));
    }

    private void CloseCurrent()
    {
        panel?.Hide();
        selectedAdventurer?.SetBeingInspected(false);
        selectedAdventurer = null;

        if (!ownsModalLock)
        {
            return;
        }

        interactor?.ReleaseModalLock(this);
        ownsModalLock = false;
    }

    private static AdventurerRecruitmentViewData CreateViewData(
        Aventureiro adventurer,
        string suggestedName
    )
    {
        Char_base definition = adventurer.charBase;

        return new AdventurerRecruitmentViewData(
            adventurer.Stats.MaxHealth,
            definition.destreza,
            definition.inteligencia,
            definition.forca,
            definition.preco,
            definition.sprite,
            suggestedName
        );
    }

    private static string MessageFor(AdventurerRecruitmentStatus status)
    {
        return status switch
        {
            AdventurerRecruitmentStatus.Recruited =>
                "Mercenário contratado",
            AdventurerRecruitmentStatus.AlreadyRecruited =>
                "Personagem já contratado!!!",
            AdventurerRecruitmentStatus.InsufficientMoney =>
                "Dinheiro insuficiente",
            _ => "Aventureiro inválido"
        };
    }
}
