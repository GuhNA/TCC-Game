// Copied into a small isolated Unity project for validation; not a gameplay script.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TCCGame.Gameplay.Exploration;
using UnityEditor;
using UnityEngine;

public static class UnityResourceAudit
{
    [Serializable] public class Manifest { public Item[] items; public Region[] regions; public Sheet[] sheets; }
    [Serializable] public class Item { public string id; public string name; public string persistenceId; public string sheet; public int row; public int column; }
    [Serializable] public class Region { public string asset; public string name; public int environment; public string[] items; public bool includeExistingWood; }
    [Serializable] public class Sheet { public string id; public int columns; public int rows; }
    [Serializable] public class Report
    {
        public bool passed;
        public int resources;
        public int sprites;
        public int regions;
        public int rewardEntries;
        public string unityVersion;
        public string[] errors;
    }

    public static void Run()
    {
        var report = new Report { unityVersion = Application.unityVersion };
        var errors = new List<string>();
        try
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Argument("-resourceManifest")));
            var byId = new Dictionary<string, ExplorationResourceDefinition>();
            var identities = new HashSet<string>();
            foreach (var sheet in manifest.sheets)
            {
                string path = "Assets/Images/Exploration/Items/" + sheet.id + ".png";
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Require(texture != null, sheet.id + ": texture missing", errors);
                Require(importer != null && importer.spriteImportMode == SpriteImportMode.Multiple,
                    sheet.id + ": not imported as multiple sprites", errors);
                if (importer != null)
                {
                    Require(importer.filterMode == FilterMode.Point, sheet.id + ": filtering", errors);
                    Require(!importer.mipmapEnabled, sheet.id + ": mipmaps", errors);
                    Require(importer.textureCompression == TextureImporterCompression.Uncompressed,
                        sheet.id + ": compressed", errors);
                }
                var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
                report.sprites += sprites.Length;
                Require(sprites.Length == sheet.rows * sheet.columns, sheet.id + ": sprite count", errors);
            }
            foreach (var item in manifest.items)
            {
                string path = "Assets/Scriptable/Exploration/Resources/" + item.id + ".asset";
                var resource = AssetDatabase.LoadAssetAtPath<ExplorationResourceDefinition>(path);
                Require(resource != null, item.id + ": asset missing or wrong script", errors);
                if (resource == null) continue;
                report.resources++;
                byId[item.id] = resource;
                Require(resource.DisplayName == item.name, item.id + ": display name mismatch", errors);
                Require(resource.PersistenceId == item.persistenceId, item.id + ": persistence ID mismatch", errors);
                Require(identities.Add(resource.PersistenceId), item.id + ": duplicate persistence ID", errors);
                Require(resource.Icon != null, item.id + ": missing sprite reference", errors);
                if (resource.Icon == null) continue;
                var sprite = resource.Icon;
                Require(sprite.name == item.id, item.id + ": wrong sprite name " + sprite.name, errors);
                Require(AssetDatabase.GetAssetPath(sprite).EndsWith("/" + item.sheet + ".png"),
                    item.id + ": wrong atlas", errors);
                var sheet = manifest.sheets.First(s => s.id == item.sheet);
                double cw = (double)sprite.texture.width / sheet.columns;
                double ch = (double)sprite.texture.height / sheet.rows;
                int left = (int)Math.Round((item.column - 1) * cw);
                int right = (int)Math.Round(item.column * cw);
                int top = (int)Math.Round((item.row - 1) * ch);
                int bottom = (int)Math.Round(item.row * ch);
                var size = new Vector2(right - left, bottom - top);
                var expected = new Rect(left, sprite.texture.height - bottom, size.x, size.y);
                Require(sprite.rect == expected, item.id + ": wrong grid rectangle " + sprite.rect, errors);
                Require(Vector2.Distance(sprite.pivot, size * 0.5f) < 0.01f, item.id + ": pivot mismatch", errors);
            }
            foreach (var region in manifest.regions)
            {
                var encounter = AssetDatabase.LoadAssetAtPath<ExplorationEncounterDefinition>(
                    "Assets/Scriptable/Exploration/Encounters/" + region.asset + ".asset");
                Require(encounter != null, region.name + ": encounter missing", errors);
                if (encounter == null) continue;
                report.regions++;
                Require((int)encounter.Environment == region.environment, region.name + ": environment changed", errors);
                var seen = new HashSet<string>();
                foreach (var entry in encounter.PossibleRewards)
                {
                    report.rewardEntries++;
                    Require(entry != null && entry.Resource != null, region.name + ": missing reward resource", errors);
                    if (entry == null || entry.Resource == null) continue;
                    Require(seen.Add(entry.Resource.PersistenceId), region.name + ": duplicate reward", errors);
                    Require(entry.Resource.Icon != null, region.name + ": reward without icon", errors);
                    Require(entry.DropChance > 0f && entry.DropChance <= 1f, region.name + ": invalid chance", errors);
                    Require(entry.MinimumQuantity >= 1 && entry.MaximumQuantity >= entry.MinimumQuantity,
                        region.name + ": invalid quantity", errors);
                    var random = new System.Random(713);
                    for (int sample = 0; sample < 100; sample++)
                    {
                        int value = entry.Roll(random);
                        Require(value == 0 || (value >= entry.MinimumQuantity && value <= entry.MaximumQuantity),
                            region.name + ": reward roll outside configured range", errors);
                    }
                }
                foreach (var id in region.items)
                    Require(byId.ContainsKey(id) && encounter.PossibleRewards.Any(e => e != null && e.Resource == byId[id]),
                        region.name + ": missing requested resource " + id, errors);
                if (region.includeExistingWood)
                    Require(encounter.PossibleRewards.Any(e => e != null && e.Resource != null && e.Resource.PersistenceId == "resource.wood"),
                        region.name + ": existing wood missing", errors);
            }
        }
        catch (Exception exception)
        {
            errors.Add(exception.ToString());
        }
        report.errors = errors.ToArray();
        report.passed = errors.Count == 0;
        File.WriteAllText(Argument("-resourceReport"), JsonUtility.ToJson(report, true));
        Debug.Log("ExplorationResourceAudit: " + (report.passed ? "PASS" : "FAIL"));
        EditorApplication.Exit(report.passed ? 0 : 1);
    }

    private static void Require(bool condition, string message, List<string> errors)
    {
        if (!condition) errors.Add(message);
    }

    private static string Argument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, name);
        if (index < 0 || index + 1 >= args.Length) throw new ArgumentException(name);
        return args[index + 1];
    }
}
