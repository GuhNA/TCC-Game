using System;
using UnityEngine;
using UnityEngine.UI;

public readonly struct AdventurerRecruitmentViewData
{
    public int MaxHealth { get; }
    public int Dexterity { get; }
    public int Intelligence { get; }
    public int Strength { get; }
    public int Price { get; }
    public Sprite Portrait { get; }

    public AdventurerRecruitmentViewData(
        int maxHealth,
        int dexterity,
        int intelligence,
        int strength,
        int price,
        Sprite portrait
    )
    {
        MaxHealth = maxHealth;
        Dexterity = dexterity;
        Intelligence = intelligence;
        Strength = strength;
        Price = price;
        Portrait = portrait;
    }
}

public sealed class AdventurerRecruitmentPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject panelRoot;

    [SerializeField]
    private Text healthText;

    [SerializeField]
    private Text dexterityText;

    [SerializeField]
    private Text intelligenceText;

    [SerializeField]
    private Text strengthText;

    [SerializeField]
    private Text priceText;

    [SerializeField]
    private Image portraitImage;

    [SerializeField]
    private Button purchaseButton;

    public event Action PurchaseRequested;

    private void Awake()
    {
        panelRoot ??= gameObject;
    }

    private void OnEnable()
    {
        purchaseButton?.onClick.AddListener(NotifyPurchaseRequested);
    }

    private void OnDisable()
    {
        purchaseButton?.onClick.RemoveListener(NotifyPurchaseRequested);
    }

    public void Show(AdventurerRecruitmentViewData data)
    {
        SetText(healthText, data.MaxHealth);
        SetText(dexterityText, data.Dexterity);
        SetText(intelligenceText, data.Intelligence);
        SetText(strengthText, data.Strength);
        SetText(priceText, data.Price);

        if (portraitImage != null)
        {
            portraitImage.sprite = data.Portrait;
        }

        (panelRoot ?? gameObject).SetActive(true);
    }

    public void Hide()
    {
        ClearText(healthText);
        ClearText(dexterityText);
        ClearText(intelligenceText);
        ClearText(strengthText);
        ClearText(priceText);

        if (portraitImage != null)
        {
            portraitImage.sprite = null;
        }

        (panelRoot ?? gameObject).SetActive(false);
    }

    private void NotifyPurchaseRequested()
    {
        PurchaseRequested?.Invoke();
    }

    private static void SetText(Text text, int value)
    {
        if (text != null)
        {
            text.text = value.ToString();
        }
    }

    private static void ClearText(Text text)
    {
        if (text != null)
        {
            text.text = string.Empty;
        }
    }
}
