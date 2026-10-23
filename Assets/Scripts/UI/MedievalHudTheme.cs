using TMPro;
using UnityEngine;

namespace TCCGame.UI
{
    [CreateAssetMenu(
        fileName = "MedievalPixelHudTheme",
        menuName = "TCC Game/UI/Medieval Pixel HUD Theme"
    )]
    public sealed class MedievalHudTheme : ScriptableObject
    {
        [Header("Typography")]
        [SerializeField] private TMP_FontAsset font;

        [Header("Surfaces")]
        [SerializeField] private Sprite woodPanel;
        [SerializeField] private Sprite parchmentPanel;
        [SerializeField] private Sprite metalPanel;

        [Header("Buttons")]
        [SerializeField] private Sprite goldButton;
        [SerializeField] private Sprite bronzeButton;
        [SerializeField] private Sprite redButton;

        [Header("Class Banners")]
        [SerializeField] private Sprite warriorBanner;
        [SerializeField] private Sprite archerBanner;
        [SerializeField] private Sprite mageBanner;

        [Header("Icons")]
        [SerializeField] private Sprite coinIcon;
        [SerializeField] private Sprite hourglassIcon;
        [SerializeField] private Sprite checkIcon;
        [SerializeField] private Sprite helmetIcon;
        [SerializeField] private Sprite headerDrape;

        [Header("Palette")]
        [SerializeField] private Color modalOverlay =
            new Color(0.025f, 0.035f, 0.05f, 0.76f);
        [SerializeField] private Color title =
            new Color(1f, 0.77f, 0.28f, 1f);
        [SerializeField] private Color subtitle =
            new Color(0.86f, 0.78f, 0.62f, 1f);
        [SerializeField] private Color goldText =
            new Color(1f, 0.82f, 0.33f, 1f);
        [SerializeField] private Color parchmentText =
            new Color(0.22f, 0.16f, 0.10f, 1f);
        [SerializeField] private Color priceText =
            new Color(0.53f, 0.31f, 0.07f, 1f);
        [SerializeField] private Color success =
            new Color(0.57f, 0.9f, 0.49f, 1f);
        [SerializeField] private Color goldButtonLabel =
            new Color(0.20f, 0.11f, 0.04f, 1f);
        [SerializeField] private Color textOutline =
            new Color32(43, 25, 14, 230);

        public TMP_FontAsset Font => font;
        public Sprite WoodPanel => woodPanel;
        public Sprite ParchmentPanel => parchmentPanel;
        public Sprite MetalPanel => metalPanel;
        public Sprite GoldButton => goldButton;
        public Sprite BronzeButton => bronzeButton;
        public Sprite RedButton => redButton;
        public Sprite WarriorBanner => warriorBanner;
        public Sprite ArcherBanner => archerBanner;
        public Sprite MageBanner => mageBanner;
        public Sprite CoinIcon => coinIcon;
        public Sprite HourglassIcon => hourglassIcon;
        public Sprite CheckIcon => checkIcon;
        public Sprite HelmetIcon => helmetIcon;
        public Sprite HeaderDrape => headerDrape;
        public Color ModalOverlay => modalOverlay;
        public Color Title => title;
        public Color Subtitle => subtitle;
        public Color GoldText => goldText;
        public Color ParchmentText => parchmentText;
        public Color PriceText => priceText;
        public Color Success => success;
        public Color GoldButtonLabel => goldButtonLabel;
        public Color TextOutline => textOutline;

        public bool IsComplete =>
            font != null &&
            woodPanel != null &&
            parchmentPanel != null &&
            metalPanel != null &&
            goldButton != null &&
            bronzeButton != null &&
            redButton != null &&
            warriorBanner != null &&
            archerBanner != null &&
            mageBanner != null &&
            coinIcon != null &&
            hourglassIcon != null &&
            checkIcon != null &&
            helmetIcon != null &&
            headerDrape != null;
    }
}
