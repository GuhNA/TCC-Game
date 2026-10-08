using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TCCGame.Gameplay.Exploration
{
    /// <summary>
    /// Visual representation of one recruited adventurer inside the Scroll View.
    /// Create one prefab with this component and wire its portrait, labels, button,
    /// and optional highlight. The controller instantiates and binds it at runtime.
    /// </summary>
    public sealed class AdventurerSelectionCardView : MonoBehaviour
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text statsText;
        [SerializeField] private Button selectionButton;
        [SerializeField] private TMP_Text selectionButtonText;
        [SerializeField] private GameObject selectedHighlight;
        [Tooltip("Persistent tint used while this adventurer belongs to the party.")]
        [SerializeField] private Color selectedButtonColor =
            new(0.75f, 0.15f, 0.15f, 1f);

        private Char_base definition;
        private Action<Char_base> selectionRequested;
        private bool isSelected;
        private bool buttonColorsCached;
        private ColorBlock defaultButtonColors;

        public bool IsSelected => isSelected;

        public void Bind(
            Char_base adventurer,
            bool selected,
            Action<Char_base> onSelectionRequested
        )
        {
            definition = adventurer;
            selectionRequested = onSelectionRequested;
            if (selectionButton != null)
            {
                selectionButton.gameObject.SetActive(true);
                selectionButton.interactable = true;
            }
            AdventurerStats stats = AdventurerStatsCalculator.Calculate(
                adventurer
            );

            if (portraitImage != null)
            {
                portraitImage.sprite = adventurer.sprite;
                portraitImage.preserveAspect = true;
            }

            if (nameText != null)
            {
                nameText.text = adventurer.name;
            }

            if (statsText != null)
            {
                statsText.text =
                    $"Vida: {stats.MaxHealth}\n" +
                    $"Dano: {stats.MinimumDamage}-{stats.MaximumDamage}\n" +
                    $"Velocidade: {stats.Speed}\n" +
                    $"Armadura: {adventurer.armadura}\n" +
                    $"Res. mágica: {adventurer.resistência_magica}\n" +
                    $"Tipo: {GetDamageTypeName(stats.DamageType)}";
            }

            selectionButton?.onClick.RemoveListener(NotifySelection);
            selectionButton?.onClick.AddListener(NotifySelection);

            if (selectionButton != null && !buttonColorsCached)
            {
                defaultButtonColors = selectionButton.colors;
                buttonColorsCached = true;

                // The button must be the last child inside the card panel so its
                // background is drawn above decorative images and labels.
                selectionButton.transform.SetAsLastSibling();
            }

            SetSelected(selected);
        }

        /// <summary>
        /// Reuses the same card layout as a read-only post-battle report.
        /// The saved snapshot is used so the displayed health is exactly the
        /// value left by that battle, even if the asset is later rebalanced.
        /// </summary>
        public void BindReport(
            Char_base adventurer,
            ExplorationCombatantSaveData snapshot
        )
        {
            definition = adventurer;
            selectionRequested = null;
            if (portraitImage != null)
            {
                portraitImage.sprite = adventurer != null
                    ? adventurer.sprite
                    : null;
                portraitImage.preserveAspect = true;
            }

            if (nameText != null)
            {
                nameText.text = string.IsNullOrWhiteSpace(snapshot?.displayName)
                    ? adventurer != null ? adventurer.name : "Aventureiro"
                    : snapshot.displayName;
            }

            if (statsText != null && snapshot != null)
            {
                statsText.text =
                    $"Vida restante: {snapshot.currentHealth}/{snapshot.maxHealth}\n" +
                    $"Dano: {snapshot.minimumDamage}-{snapshot.maximumDamage}\n" +
                    $"Velocidade: {snapshot.speed}\n" +
                    $"Armadura: {snapshot.armor}\n" +
                    $"Res. mágica: {snapshot.magicResistance}\n" +
                    $"Estado: {(snapshot.currentHealth > 0 ? "Retornou" : "Caído")}";
            }

            selectedHighlight?.SetActive(false);
            if (selectionButton != null)
            {
                selectionButton.onClick.RemoveListener(NotifySelection);
                selectionButton.interactable = false;
                selectionButton.gameObject.SetActive(false);
            }
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            if (selectedHighlight != null)
            {
                selectedHighlight.SetActive(selected);

                // UI is rendered in sibling order. Keeping the medal last makes
                // it appear above the portrait, panel and other card elements.
                if (selected)
                {
                    selectedHighlight.transform.SetAsLastSibling();
                }
            }

            if (selectionButtonText != null)
            {
                selectionButtonText.text = selected
                    ? "Remover"
                    : "Selecionar";
            }

            UpdateButtonColors(selected);
        }

        private void OnDestroy()
        {
            selectionButton?.onClick.RemoveListener(NotifySelection);
        }

        private void NotifySelection()
        {
            selectionRequested?.Invoke(definition);
        }

        private void UpdateButtonColors(bool selected)
        {
            if (selectionButton == null || !buttonColorsCached)
            {
                return;
            }

            ColorBlock colors = defaultButtonColors;
            if (selected)
            {
                colors.normalColor = selectedButtonColor;
                colors.selectedColor = selectedButtonColor;
                colors.highlightedColor = Color.Lerp(
                    selectedButtonColor,
                    Color.white,
                    0.2f
                );
            }

            selectionButton.colors = colors;
            selectionButton.targetGraphic?.CrossFadeColor(
                selected ? selectedButtonColor : colors.normalColor,
                0f,
                true,
                true
            );
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (selectedHighlight != null &&
                !selectedHighlight.transform.IsChildOf(transform))
            {
                Debug.LogError(
                    "Selected Highlight must be a child of the same adventurer " +
                    "card prefab. External scene references are not cloned with " +
                    "each card.",
                    this
                );
            }
        }
#endif

        private static string GetDamageTypeName(DamageType type)
        {
            return type == DamageType.Magical ? "Mágico" : "Físico";
        }
    }
}
