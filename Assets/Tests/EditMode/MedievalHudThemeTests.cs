using System.Linq;
using NUnit.Framework;
using TMPro;
using TCCGame.UI;
using UnityEngine;

namespace TCCGame.Tests.EditMode
{
    public sealed class MedievalHudThemeTests
    {
        [Test]
        public void RecruitmentPrefab_UsesCompleteSharedTheme()
        {
            MedievalHudTheme theme = Resources.Load<MedievalHudTheme>(
                "UI/Theme/MedievalPixelHudTheme");
            GameObject prefab = Resources.Load<GameObject>("UI/RecruitmentCanvas");

            Assert.That(theme, Is.Not.Null, "Shared HUD theme was not generated.");
            Assert.That(theme.IsComplete, Is.True,
                "Shared HUD theme is missing one or more required assets.");
            Assert.That(prefab, Is.Not.Null, "Recruitment prefab was not generated.");

            TMP_Text[] labels = prefab.GetComponentsInChildren<TMP_Text>(true);
            Assert.That(labels, Is.Not.Empty);
            Assert.That(labels.All(label => label.font == theme.Font), Is.True,
                "Recruitment labels must use the shared HUD font.");
        }
    }
}
