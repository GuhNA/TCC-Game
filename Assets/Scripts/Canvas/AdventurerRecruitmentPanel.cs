using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TCCGame.Core.Input;
using TMPro;

public readonly struct AdventurerRecruitmentViewData
{
    public int MaxHealth { get; }
    public int Dexterity { get; }
    public int Intelligence { get; }
    public int Strength { get; }
    public int Price { get; }
    public Sprite Portrait { get; }
    public string SuggestedName { get; }

    public AdventurerRecruitmentViewData(
        int maxHealth,
        int dexterity,
        int intelligence,
        int strength,
        int price,
        Sprite portrait,
        string suggestedName
    )
    {
        MaxHealth = maxHealth;
        Dexterity = dexterity;
        Intelligence = intelligence;
        Strength = strength;
        Price = price;
        Portrait = portrait;
        SuggestedName = suggestedName;
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

    [Header("Nome do aventureiro")]
    [Tooltip("Opcional. Se vazio, um campo é criado automaticamente no painel.")]
    [SerializeField]
    private TMP_InputField adventurerNameInput;

    [Header("Destaque do campo de nome")]
    [Tooltip("Quantidade de piscadas ao abrir o painel.")]
    [SerializeField, Min(0)]
    private int nameHintBlinkCount = 3;

    [Tooltip("Tempo para escurecer e para voltar à cor original.")]
    [SerializeField, Min(0.01f)]
    private float nameHintHalfCycleSeconds = 0.14f;

    [Tooltip("Multiplicador RGB da fase escura. Valores menores escurecem mais.")]
    [SerializeField, Range(0f, 1f)]
    private float nameHintDarkness = 0.45f;

    private Coroutine nameHintRoutine;
    private Graphic nameHintGraphic;
    private Color nameHintOriginalColor;
    private bool hasNameHintOriginalColor;

    public string AdventurerName => adventurerNameInput == null
        ? string.Empty
        : ConvertLineBreaksToSpaces(adventurerNameInput.text);

    public event Action PurchaseRequested;

    private void Awake()
    {
        panelRoot ??= gameObject;
    }

    private void OnEnable()
    {
        purchaseButton?.onClick.AddListener(NotifyPurchaseRequested);
        BindNameInputEvents();
    }

    private void OnDisable()
    {
        StopNameHintAnimation();
        UiMenuState.SetOpen(this, false);
        purchaseButton?.onClick.RemoveListener(NotifyPurchaseRequested);
        UnbindNameInputEvents();
    }

    public void Show(AdventurerRecruitmentViewData data)
    {
        // TMP_InputField reconstrói seu label em OnEnable. Por isso o painel
        // deve estar ativo antes de atribuirmos e forçarmos o nome sugerido.
        (panelRoot ?? gameObject).SetActive(true);
        EnsureNameInput();
        SetText(healthText, data.MaxHealth);
        SetText(dexterityText, data.Dexterity);
        SetText(intelligenceText, data.Intelligence);
        SetText(strengthText, data.Strength);
        SetText(priceText, data.Price);

        if (portraitImage != null)
        {
            portraitImage.sprite = data.Portrait;
        }

        if (adventurerNameInput != null)
        {
            adventurerNameInput.characterLimit =
                TCCGame.Gameplay.Player.PlayerInventoryService
                    .MaximumAdventurerNameLength;
            ApplySuggestedName(data.SuggestedName);
            StartNameHintAnimation(data.SuggestedName);
        }

        UiMenuState.SetOpen(this, true);
    }

    public void Hide()
    {
        StopNameHintAnimation();
        UiMenuState.SetOpen(this, false);
        ClearText(healthText);
        ClearText(dexterityText);
        ClearText(intelligenceText);
        ClearText(strengthText);
        ClearText(priceText);

        adventurerNameInput?.SetTextWithoutNotify(string.Empty);

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

    private void ApplySuggestedName(string suggestedName)
    {
        if (adventurerNameInput == null)
        {
            return;
        }

        // O nome sugerido vem do sistema e deve manter sua formatação original.
        // Assim "Arqueiro 2" continua em uma linha. Somente um espaço digitado
        // pelo jogador é transformado em quebra de linha.
        string value = suggestedName ?? string.Empty;
        adventurerNameInput.SetTextWithoutNotify(value);

        // SetTextWithoutNotify atualiza o valor lógico, mas um TMP_InputField
        // recém-ativado pode ainda não ter reconstruído o componente visual.
        // A atribuição direta e ForceLabelUpdate sincronizam os dois estados.
        if (adventurerNameInput.textComponent != null)
        {
            adventurerNameInput.textComponent.text = value;
            adventurerNameInput.textComponent.gameObject.SetActive(true);
        }

        adventurerNameInput.ForceLabelUpdate();
    }

    private void StartNameHintAnimation(string suggestedName)
    {
        StopNameHintAnimation();
        nameHintRoutine = StartCoroutine(
            ReapplyNameAndBlink(suggestedName)
        );
    }

    private IEnumerator ReapplyNameAndBlink(string suggestedName)
    {
        // Aguarda o Canvas e o TMP_InputField concluírem o primeiro rebuild.
        yield return null;
        ApplySuggestedName(suggestedName);
        Canvas.ForceUpdateCanvases();

        nameHintGraphic = adventurerNameInput != null
            ? adventurerNameInput.targetGraphic
            : null;
        if (nameHintGraphic == null || nameHintBlinkCount <= 0)
        {
            nameHintRoutine = null;
            yield break;
        }

        nameHintOriginalColor = nameHintGraphic.color;
        hasNameHintOriginalColor = true;
        Color darkColor = new Color(
            nameHintOriginalColor.r * nameHintDarkness,
            nameHintOriginalColor.g * nameHintDarkness,
            nameHintOriginalColor.b * nameHintDarkness,
            nameHintOriginalColor.a
        );

        for (int index = 0; index < nameHintBlinkCount; index++)
        {
            yield return AnimateNameHintColor(
                nameHintOriginalColor,
                darkColor
            );
            yield return AnimateNameHintColor(
                darkColor,
                nameHintOriginalColor
            );
        }

        RestoreNameHintColor();
        nameHintRoutine = null;
    }

    private IEnumerator AnimateNameHintColor(Color from, Color to)
    {
        float duration = Mathf.Max(0.01f, nameHintHalfCycleSeconds);
        float elapsed = 0f;
        while (elapsed < duration && nameHintGraphic != null)
        {
            elapsed += Time.unscaledDeltaTime;
            nameHintGraphic.color = Color.Lerp(
                from,
                to,
                Mathf.Clamp01(elapsed / duration)
            );
            yield return null;
        }

        if (nameHintGraphic != null)
        {
            nameHintGraphic.color = to;
        }
    }

    private void StopNameHintAnimation()
    {
        if (nameHintRoutine != null)
        {
            StopCoroutine(nameHintRoutine);
            nameHintRoutine = null;
        }

        RestoreNameHintColor();
    }

    private void RestoreNameHintColor()
    {
        if (nameHintGraphic != null && hasNameHintOriginalColor)
        {
            nameHintGraphic.color = nameHintOriginalColor;
        }

        nameHintGraphic = null;
        hasNameHintOriginalColor = false;
    }

    private void EnsureNameInput()
    {
        if (adventurerNameInput != null)
        {
            ConfigureNameInputVisuals();
            return;
        }

        Transform parent = (panelRoot ?? gameObject).transform;
        var inputObject = new GameObject(
            "Nome do aventureiro",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(TMP_InputField)
        );
        inputObject.layer = parent.gameObject.layer;
        inputObject.transform.SetParent(parent, false);
        RectTransform inputRect = inputObject.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.62f, 0.82f);
        inputRect.anchorMax = new Vector2(0.95f, 0.89f);
        inputRect.offsetMin = Vector2.zero;
        inputRect.offsetMax = Vector2.zero;

        Image background = inputObject.GetComponent<Image>();
        background.color = new Color(0.08f, 0.08f, 0.08f, 0.86f);

        var viewportObject = new GameObject(
            "Text Area",
            typeof(RectTransform),
            typeof(RectMask2D)
        );
        viewportObject.layer = inputObject.layer;
        viewportObject.transform.SetParent(inputObject.transform, false);
        RectTransform viewport = viewportObject.GetComponent<RectTransform>();
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(12f, 4f);
        viewport.offsetMax = new Vector2(-12f, -4f);

        TMP_Text placeholder = CreateInputText(
            viewport,
            "Placeholder",
            new Color(1f, 1f, 1f, 0.45f),
            "Nome do aventureiro"
        );
        placeholder.fontStyle = FontStyles.Italic;
        TMP_Text text = CreateInputText(
            viewport,
            "Texto do nome",
            Color.white,
            string.Empty
        );

        adventurerNameInput = inputObject.GetComponent<TMP_InputField>();
        adventurerNameInput.textViewport = viewport;
        adventurerNameInput.textComponent = text;
        adventurerNameInput.placeholder = placeholder;
        adventurerNameInput.targetGraphic = background;
        adventurerNameInput.lineType = TMP_InputField.LineType.SingleLine;
        adventurerNameInput.characterLimit =
            TCCGame.Gameplay.Player.PlayerInventoryService
                .MaximumAdventurerNameLength;
        BindNameInputEvents();
        ConfigureNameInputVisuals();
        inputObject.transform.SetAsLastSibling();
    }

    private void ConfigureNameInputVisuals()
    {
        if (adventurerNameInput == null)
        {
            return;
        }

        adventurerNameInput.lineType =
            TMP_InputField.LineType.MultiLineNewline;

        ConfigureResponsiveTmpText(adventurerNameInput.textComponent);
        ConfigureResponsiveTmpText(
            adventurerNameInput.placeholder as TMP_Text
        );
    }

    private static void ConfigureResponsiveTmpText(TMP_Text text)
    {
        if (text == null)
        {
            return;
        }

        // O Input Name da cena usa escala reduzida para encaixar a arte.
        // Auto-size mantém as letras legíveis sem alterar posição ou tamanho
        // do campo que foi organizado manualmente no Canvas.
        text.enableAutoSizing = true;
        text.fontSizeMin = 24f;
        text.fontSizeMax = 200f;
        // Centraliza tanto nomes de uma linha quanto nomes compostos empilhados.
        text.alignment = TextAlignmentOptions.Center;
        text.overflowMode = TextOverflowModes.Truncate;
    }

    private void BindNameInputEvents()
    {
        if (adventurerNameInput == null)
        {
            return;
        }

        adventurerNameInput.onValueChanged.RemoveListener(
            FormatNameAsStackedWords
        );
        adventurerNameInput.onValueChanged.AddListener(
            FormatNameAsStackedWords
        );
        adventurerNameInput.onValidateInput -= ValidateNameCharacter;
        adventurerNameInput.onValidateInput += ValidateNameCharacter;
    }

    private void UnbindNameInputEvents()
    {
        if (adventurerNameInput == null)
        {
            return;
        }

        adventurerNameInput.onValueChanged.RemoveListener(
            FormatNameAsStackedWords
        );
        adventurerNameInput.onValidateInput -= ValidateNameCharacter;
    }

    /// <summary>
    /// Transforma o primeiro espaço digitado em Enter. Enquanto a quebra existir,
    /// outros espaços/Enters são recusados. Ao apagar a quebra e voltar para uma
    /// linha, a própria verificação do texto libera um novo espaço imediatamente.
    /// </summary>
    private char ValidateNameCharacter(
        string currentText,
        int characterIndex,
        char addedCharacter
    )
    {
        bool isRequestedSeparator = addedCharacter == ' ' ||
            addedCharacter == '\n' ||
            addedCharacter == '\r';
        if (!isRequestedSeparator)
        {
            return addedCharacter;
        }

        return ContainsLineBreak(currentText) ? '\0' : '\n';
    }

    private void FormatNameAsStackedWords(string value)
    {
        if (adventurerNameInput == null || string.IsNullOrEmpty(value))
        {
            return;
        }

        int caretPosition = adventurerNameInput.stringPosition;
        string formatted = KeepOnlyFirstPlayerLineBreak(value);
        if (string.Equals(value, formatted, StringComparison.Ordinal))
        {
            return;
        }

        adventurerNameInput.SetTextWithoutNotify(formatted);
        adventurerNameInput.stringPosition = Mathf.Clamp(
            caretPosition,
            0,
            formatted.Length
        );
        adventurerNameInput.ForceLabelUpdate();
    }

    /// <summary>
    /// Também protege contra colagem de textos com vários espaços ou Enters.
    /// A primeira separação entre palavras vira uma quebra; as demais são
    /// removidas para manter o nome limitado a duas linhas.
    /// </summary>
    private static string KeepOnlyFirstPlayerLineBreak(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var result = new System.Text.StringBuilder(value.Length);
        bool alreadyHasLineBreak = false;
        for (int index = 0; index < value.Length; index++)
        {
            char character = value[index];
            bool isLineBreak = character == '\n' || character == '\r';
            bool isCompoundNameSpace = character == ' ' &&
                index + 1 < value.Length &&
                char.IsLetter(value[index + 1]);

            if (!isLineBreak && !isCompoundNameSpace)
            {
                result.Append(character);
                continue;
            }

            // Trata CRLF como uma única quebra.
            if (character == '\r' && index + 1 < value.Length &&
                value[index + 1] == '\n')
            {
                index++;
            }

            if (!alreadyHasLineBreak)
            {
                result.Append('\n');
                alreadyHasLineBreak = true;
            }
        }

        return result.ToString();
    }

    private static bool ContainsLineBreak(string value)
    {
        return !string.IsNullOrEmpty(value) &&
            (value.IndexOf('\n') >= 0 || value.IndexOf('\r') >= 0);
    }

    private static string ConvertLineBreaksToSpaces(string value)
    {
        return value
            .Replace("\r\n", " ")
            .Replace('\r', ' ')
            .Replace('\n', ' ');
    }

    private static TMP_Text CreateInputText(
        Transform parent,
        string objectName,
        Color color,
        string value
    )
    {
        var textObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        textObject.layer = parent.gameObject.layer;
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TMP_Text text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = 28;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.text = value;
        text.richText = false;
        text.raycastTarget = false;
        return text;
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
