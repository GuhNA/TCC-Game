using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TCCGame.Core;
using TCCGame.Core.Persistence;
using TCCGame.Core.Time;
using TCCGame.Prototypes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TCCGame.Tests.PlayMode
{
    public sealed class CharacterDepthSmokeTests
    {
        private static readonly FieldInfo SessionField = typeof(GameSessionProvider)
            .GetField("current", BindingFlags.Static | BindingFlags.NonPublic);
        private object previousSession;

        [SetUp]
        public void SetUp()
        {
            previousSession = SessionField.GetValue(null);
            var save = GameSaveData.CreateDefault();
            save.money = 1000;
            save.SetBuildingLevel(BuildingType.Tavern, 1);
            SessionField.SetValue(null, new GameSession(new MemoryGameSaveRepository(save), new SystemClock()));
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.IsValid() && new[] { "Centro", "Portão", "Taverna" }.Contains(scene.name))
            {
                SceneManager.SetActiveScene(SceneManager.CreateScene("Character depth cleanup"));
                yield return SceneManager.UnloadSceneAsync(scene);
            }
            SessionField.SetValue(null, previousSession);
        }

        [UnityTest]
        public IEnumerator ProductionCharacters_SortAboveBelowAndAtEqualFeetHeight()
        {
            foreach (string scene in new[] { "Centro", "Portão", "Taverna" })
            {
                yield return SceneManager.LoadSceneAsync(scene);
                yield return null;
                yield return null; // The tavern spawner creates its offers in Start.
                var player = Object.FindFirstObjectByType<PlayerInteractor>();
                player.GetComponent<moviment>().enabled = false;
                player.GetComponent<Rigidbody2D>().simulated = false;
                var depth = player.GetComponent<WorldPrototypeDepthSort>();
                Assert.That(depth, Is.Not.Null, scene);
                var npcs = Object.FindObjectsByType<WorldPrototypeDepthSort>(FindObjectsSortMode.None)
                    .Where(item => item.UseFeetCenter && item != depth).ToArray();
                Assert.That(npcs.Length, Is.GreaterThanOrEqualTo(scene == "Centro" ? 5 : scene == "Portão" ? 3 : 1));
                if (scene == "Taverna")
                    foreach (var spawned in Object.FindObjectsByType<Aventureiro>(FindObjectsSortMode.None))
                        Assert.That(spawned.GetComponent<WorldPrototypeDepthSort>(), Is.Not.Null, spawned.name);

                foreach (var npc in npcs)
                {
                    Assert.That(npc.Feet.isTrigger, Is.False, npc.name);
                    Assert.That(npc.Target.sortingLayerID, Is.EqualTo(depth.Target.sortingLayerID), npc.name);
                    foreach (float y in new[] { .15f, -.15f, 0f })
                    {
                        player.transform.position += npc.GroundPosition + Vector3.up * y - depth.GroundPosition;
                        Physics2D.SyncTransforms();
                        for (int frame = 0; frame < 3; frame++)
                        {
                            yield return null;
                            if (y > 0)
                                Assert.That(depth.Target.sortingOrder, Is.LessThan(npc.Target.sortingOrder), $"{scene}/{npc.name}: above");
                            else
                                Assert.That(depth.Target.sortingOrder, Is.GreaterThan(npc.Target.sortingOrder), $"{scene}/{npc.name}: below or tied");
                        }
                    }
                }
            }
        }

        [UnityTest]
        public IEnumerator Vendor_ResistsPlayerContactFromEverySideAndStillOpens()
        {
            yield return SceneManager.LoadSceneAsync("Portão");
            yield return null;
            var vendor = Object.FindFirstObjectByType<NPCVenda>();
            var vendorBody = vendor.GetComponent<Rigidbody2D>();
            var feet = vendor.GetComponent<CircleCollider2D>();
            var player = Object.FindFirstObjectByType<PlayerInteractor>();
            player.GetComponent<moviment>().enabled = false;
            var playerBody = player.GetComponent<Rigidbody2D>();
            var playerFeet = player.GetComponents<Collider2D>().First(item => !item.isTrigger);
            Vector3 position = vendor.transform.position;
            Quaternion rotation = vendor.transform.rotation;
            Assert.That(vendorBody.bodyType, Is.EqualTo(RigidbodyType2D.Static));
            Assert.That(feet.isTrigger, Is.False);
            foreach (Vector2 side in new[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right })
            {
                Vector3 offset = WorldPrototypeDepthSort.GroundCenter(playerFeet) - player.transform.position;
                float distance = feet.bounds.extents.magnitude + playerFeet.bounds.extents.magnitude + .12f;
                playerBody.position = (Vector2)(WorldPrototypeDepthSort.GroundCenter(feet) - offset) + side * distance;
                Physics2D.SyncTransforms();
                for (int frame = 0; frame < 25; frame++)
                {
                    playerBody.linearVelocity = -side * 4;
                    yield return new WaitForFixedUpdate();
                    Assert.That(Vector3.Distance(vendor.transform.position, position), Is.LessThan(.0001f));
                    Assert.That(vendor.transform.rotation, Is.EqualTo(rotation));
                }
                Assert.That(playerFeet.Distance(feet).distance, Is.GreaterThan(-.05f), "Player must be blocked by the feet circle.");
            }
            playerBody.linearVelocity = Vector2.zero;
            yield return null;
            Assert.That(player.CurrentInteractable, Is.SameAs(vendor));
            vendor.Interact(player.gameObject);
            Assert.That(vendor.popup.activeSelf, Is.True);
            Assert.That(player.HasModalOpen, Is.True);
            vendor.CloseModal();
            Assert.That(player.HasModalOpen, Is.False);
        }
    }
}
