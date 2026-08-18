using System.Collections;
using System.Linq;
using NUnit.Framework;
using TCCGame.Core;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Recruitment;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TCCGame.Tests.PlayMode
{
    public class SceneIntegrationTests
    {
        private bool hadLegacySave;
        private string legacySave;
        private bool hadVersionTwoSave;
        private string versionTwoSave;

        [SetUp]
        public void SetUp()
        {
            hadLegacySave = PlayerPrefs.HasKey(
                GameSaveRepository.LegacyCitySaveKey
            );
            legacySave = hadLegacySave
                ? PlayerPrefs.GetString(
                    GameSaveRepository.LegacyCitySaveKey
                )
                : null;
            hadVersionTwoSave = PlayerPrefs.HasKey(
                GameSaveRepository.SaveKey
            );
            versionTwoSave = hadVersionTwoSave
                ? PlayerPrefs.GetString(GameSaveRepository.SaveKey)
                : null;

            PlayerPrefs.DeleteKey(GameSaveRepository.LegacyCitySaveKey);
            PlayerPrefs.DeleteKey(GameSaveRepository.SaveKey);
            PlayerPrefs.Save();
            GameSessionProvider.Reset();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Scene activeScene = SceneManager.GetActiveScene();

            if (activeScene.IsValid() &&
                activeScene.isLoaded &&
                (activeScene.name == "Centro" ||
                 activeScene.name == "Portão"))
            {
                Scene cleanupScene = SceneManager.CreateScene(
                    "Scene integration cleanup"
                );
                SceneManager.SetActiveScene(cleanupScene);
                AsyncOperation unload = SceneManager.UnloadSceneAsync(
                    activeScene
                );

                if (unload != null)
                {
                    while (!unload.isDone)
                    {
                        yield return null;
                    }
                }
            }

            GameSessionProvider.Reset();

            if (hadLegacySave)
            {
                PlayerPrefs.SetString(
                    GameSaveRepository.LegacyCitySaveKey,
                    legacySave
                );
            }
            else
            {
                PlayerPrefs.DeleteKey(
                    GameSaveRepository.LegacyCitySaveKey
                );
            }

            if (hadVersionTwoSave)
            {
                PlayerPrefs.SetString(
                    GameSaveRepository.SaveKey,
                    versionTwoSave
                );
            }
            else
            {
                PlayerPrefs.DeleteKey(GameSaveRepository.SaveKey);
            }

            PlayerPrefs.Save();
            GameSessionProvider.Reset();
        }

        [UnityTest]
        public IEnumerator CentroAndPortao_ReadTheSameMoneyAndBuildingState()
        {
            GameSaveData data = GameSaveData.CreateDefault();
            data.money = 777;
            data.SetBuildingLevel(BuildingType.ArcherStand, 2);
            data.recruitment.initialized = true;
            data.recruitment.availableArchers = 500;
            data.recruitment.nextRestockUnixSeconds = 0;
            new GameSaveRepository().Save(data);
            GameSessionProvider.Reset();

            yield return SceneManager.LoadSceneAsync(
                "Centro",
                LoadSceneMode.Single
            );
            yield return null;

            BuildingBehaviour archerStand = Object
                .FindObjectsByType<BuildingBehaviour>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                .Single(
                    building => building.Definition.type ==
                        BuildingType.ArcherStand
                );
            GoldBalancePresenter centroGoldPresenter =
                Object.FindFirstObjectByType<GoldBalancePresenter>();

            Assert.That(centroGoldPresenter, Is.Not.Null);
            Assert.That(
                centroGoldPresenter.GoldText.text,
                Is.EqualTo("Ouro: 777")
            );
            Assert.That(
                GameSessionProvider.Current.Economy.Money,
                Is.EqualTo(777)
            );
            Assert.That(archerStand.State.level, Is.EqualTo(2));

            GameSessionProvider.Current.Economy.AddMoney(1);

            Assert.That(
                centroGoldPresenter.GoldText.text,
                Is.EqualTo("Ouro: 778")
            );

            yield return SceneManager.LoadSceneAsync(
                "Portão",
                LoadSceneMode.Single
            );
            yield return null;

            GoldBalancePresenter goldPresenter =
                Object.FindFirstObjectByType<GoldBalancePresenter>();
            NPCVenda vendor = Object.FindFirstObjectByType<NPCVenda>();

            vendor.SendMessage(
                "abrir_popup",
                SendMessageOptions.RequireReceiver
            );

            Assert.That(goldPresenter, Is.Not.Null);
            Assert.That(goldPresenter.GoldText.text, Is.EqualTo("Ouro: 778"));
            Assert.That(
                vendor.arqueiro_texto.text,
                Is.EqualTo("Arqueiros: 16")
            );
            Assert.That(
                GameSessionProvider.Current.Economy.Money,
                Is.EqualTo(778)
            );

            Button archerPlusOne = Object
                .FindObjectsByType<Button>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                .Single(
                    button => button.name == "+1" &&
                        HasAncestorContaining(button.transform, "arqueiro")
                );

            archerPlusOne.onClick.Invoke();

            Assert.That(
                GameSessionProvider.Current.Economy.Money,
                Is.EqualTo(773)
            );
            Assert.That(goldPresenter.GoldText.text, Is.EqualTo("Ouro: 773"));
            Assert.That(
                vendor.arqueiro_texto.text,
                Is.EqualTo("Arqueiros: 15")
            );
            Assert.That(
                GameSessionProvider.Current.Recruitment
                    .GetOwned(UnitType.Archer),
                Is.EqualTo(1)
            );
        }

        private static bool HasAncestorContaining(
            Transform transform,
            string text
        )
        {
            Transform current = transform;

            while (current != null)
            {
                if (current.name.ToLowerInvariant().Contains(text))
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }
    }
}
