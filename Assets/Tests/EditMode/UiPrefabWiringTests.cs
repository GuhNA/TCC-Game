using System.IO;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TCCGame.Tests.EditMode
{
    public sealed class UiPrefabWiringTests
    {
        [TestCase("UI/InteractionPrompt")]
        [TestCase("UI/WaveAlertCanvas")]
        [TestCase("UI/WaveAlertIndicator")]
        [TestCase("UI/GateContextCanvas")]
        [TestCase("UI/TravelConversationCanvas")]
        [TestCase("UI/GarrisonManagementCanvas")]
        [TestCase("UI/RecruitmentCanvas")]
        public void UiPrefab_ContainsCanvasAndText(string resourcePath)
        {
            GameObject prefab = Resources.Load<GameObject>(resourcePath);

            Assert.That(prefab, Is.Not.Null, resourcePath);
            Assert.That(prefab.GetComponentInChildren<Canvas>(true), Is.Not.Null);
            Assert.That(prefab.GetComponentInChildren<TMP_Text>(true), Is.Not.Null);
        }

        [Test]
        public void TravelPrefab_ContainsRequiredControls()
        {
            AssertNamedChildren(
                "UI/TravelConversationCanvas",
                "Travel Dialog", "Title", "Body", "Confirm", "Cancel"
            );
        }

        [Test]
        public void GarrisonPrefab_ContainsRequiredControls()
        {
            AssertNamedChildren(
                "UI/GarrisonManagementCanvas",
                "Garrison Dialog", "Title", "Battle Report", "Summary",
                "Destination", "Status", "Acknowledge Report",
                "Change Destination", "Close"
            );
            GameObject prefab = Resources.Load<GameObject>(
                "UI/GarrisonManagementCanvas"
            );
            Assert.That(
                prefab.GetComponentsInChildren<Button>(true).Length,
                Is.EqualTo(15)
            );
        }

        [Test]
        public void RecruitmentPrefab_ContainsThreeFunctionalCards()
        {
            AssertNamedChildren(
                "UI/RecruitmentCanvas",
                "Recruitment Dialog", "Prototype Board", "Title", "Subtitle",
                "Gold", "Timer", "Feedback", "Close", "GUERREIRO Card",
                "ARQUEIRO Card", "MAGO Card"
            );
            GameObject prefab = Resources.Load<GameObject>(
                "UI/RecruitmentCanvas"
            );
            Assert.That(
                prefab.GetComponentsInChildren<RecruitmentRowView>(true),
                Has.Length.EqualTo(3)
            );
            Assert.That(
                prefab.GetComponentsInChildren<Button>(true),
                Has.Length.EqualTo(19)
            );
        }

        [Test]
        public void SceneFadePrefab_CoversScreenAtHighestUiOrder()
        {
            GameObject prefab = Resources.Load<GameObject>("UI/SceneFadeCanvas");
            Assert.That(prefab, Is.Not.Null);
            Canvas canvas = prefab.GetComponent<Canvas>();
            Assert.That(canvas, Is.Not.Null);
            Assert.That(canvas.sortingOrder, Is.EqualTo(1000));

            Transform fade = prefab.GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Fade Image");
            RectTransform rect = fade.GetComponent<RectTransform>();
            Assert.That(rect.anchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(rect.anchorMax, Is.EqualTo(Vector2.one));
            Assert.That(rect.offsetMin, Is.EqualTo(Vector2.zero));
            Assert.That(rect.offsetMax, Is.EqualTo(Vector2.zero));
            Assert.That(fade.GetComponent<Image>(), Is.Not.Null);
        }

        [TestCase("UI/InteractionPrompt")]
        [TestCase("UI/WaveAlertCanvas")]
        [TestCase("UI/WaveAlertIndicator")]
        [TestCase("UI/GateContextCanvas")]
        [TestCase("UI/TravelConversationCanvas")]
        [TestCase("UI/GarrisonManagementCanvas")]
        [TestCase("UI/RecruitmentCanvas")]
        public void EditableUiPrefab_HasNoEmptyTextPlaceholders(
            string resourcePath
        )
        {
            GameObject prefab = Resources.Load<GameObject>(resourcePath);
            Assert.That(prefab, Is.Not.Null);
            TMP_Text[] texts = prefab.GetComponentsInChildren<TMP_Text>(true);
            Assert.That(texts, Is.Not.Empty);
            foreach (TMP_Text text in texts)
            {
                Assert.That(
                    text.text,
                    Is.Not.Null.And.Not.Empty,
                    $"{resourcePath}/{text.name} needs an editor placeholder."
                );
            }
        }

        [TestCase("UI/TravelConversationCanvas", "Travel Dialog")]
        [TestCase("UI/GarrisonManagementCanvas", "Garrison Dialog")]
        public void ModalPrefab_IsVisibleInPrefabMode(
            string resourcePath,
            string rootName
        )
        {
            GameObject prefab = Resources.Load<GameObject>(resourcePath);
            Transform root = prefab.GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == rootName);
            Assert.That(root.gameObject.activeSelf, Is.True);
        }

        [Test]
        public void RuntimeUiControllers_DoNotBuildVisualHierarchy()
        {
            string[] relativePaths =
            {
                "Scripts/Campaign/GlobalThreatHud.cs",
                "Scripts/Campaign/GarrisonManagementPanel.cs",
                "Scripts/Core/Input/InteractionPromptPresenter.cs",
                "Scripts/Navigation/GateContextPresenter.cs",
                "Scripts/Navigation/TravelConversationPanel.cs",
                "Scripts/Navigation/SceneTransitionController.cs"
            };
            string[] forbidden =
            {
                "AddComponent<Canvas>",
                "typeof(Canvas)",
                "AddComponent<Image>",
                "typeof(Image)",
                "AddComponent<TextMeshProUGUI>",
                "typeof(TextMeshProUGUI)",
                "CreateFadeOverlay",
                "CreateButton",
                "CreateText"
            };

            foreach (string relativePath in relativePaths)
            {
                string source = File.ReadAllText(
                    Path.Combine(Application.dataPath, relativePath)
                );
                foreach (string token in forbidden)
                {
                    Assert.That(source, Does.Not.Contain(token),
                        $"{relativePath} still contains {token}.");
                }
            }
        }

        private static void AssertNamedChildren(
            string resourcePath,
            params string[] names
        )
        {
            GameObject prefab = Resources.Load<GameObject>(resourcePath);
            Assert.That(prefab, Is.Not.Null);
            string[] actual = prefab.GetComponentsInChildren<Transform>(true)
                .Select(item => item.name)
                .ToArray();
            foreach (string name in names)
            {
                Assert.That(actual, Does.Contain(name), resourcePath);
            }
        }
    }
}
