using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DnD;
using DnD.Model;
using DnD.Model.Inventory;
using DnD.UI;
using DnD.UI.Tabs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

static class Program
{
    private static int passed;
    private static readonly string TestRoot = Path.Combine(Path.GetTempPath(), "DnD-Regression-" + Guid.NewGuid().ToString("N"));
    static void Main()
    {
        Directory.CreateDirectory(TestRoot);
        try
        {
            Test("XP thresholds match all twenty D&D 5e levels", () =>
            {
                var thresholds = new[]
                {
                    0, 300, 900, 2700, 6500, 14000, 23000, 34000, 48000, 64000,
                    85000, 100000, 120000, 140000, 165000, 195000, 225000, 265000, 305000, 355000
                };
                for (var index = 0; index < thresholds.Length; index++)
                {
                    Equal(index + 1, CharacterData.GetLevelForExperience(thresholds[index]));
                    if (index > 0)
                        Equal(index, CharacterData.GetLevelForExperience(thresholds[index] - 1));
                }
                Equal(1, CharacterData.GetLevelForExperience(int.MinValue));
                Equal(20, CharacterData.GetLevelForExperience(int.MaxValue));
            });
            Test("Changing XP updates level in both directions and clamps negative XP", () =>
            {
                var hero = new CharacterData { experience = 6500 };
                hero.Recalculate(); Equal(5, hero.level);
                hero.experience = 300; hero.Recalculate(); Equal(2, hero.level);
                hero.experience = -10; hero.Recalculate(); Equal(1, hero.level); Equal(0, hero.experience);
                hero.experience = int.MaxValue; hero.Recalculate(); Equal(20, hero.level);
            });
            Test("Saved and imported levels are recalculated from XP", () =>
            {
                SetSaveFolder("level-from-xp");
                var hero = new CharacterData { level = 20, experience = 900 };
                Check(hero.Save()); Equal(3, hero.level); Equal(3, CharacterData.Load(hero.id).level);
                var oldJson = JObject.Parse(JsonStorage.Serialize(hero));
                oldJson.Remove("dataVersion"); oldJson["level"] = 18; oldJson["experience"] = 14000;
                var imported = JsonStorage.Deserialize<CharacterData>(oldJson.ToString());
                Equal(6, imported.level); Equal(14000, imported.experience);
            });
            Test("Self-sync preserves effects and props", () =>
            {
                var item = MakeItem();
                item.SyncModelFrom(item);
                Equal(1, item.effects.Count); Equal(1, item.props.Count);
            });
            Test("Library copy starts at one and owns its lists", () =>
            {
                var item = MakeItem(); item.count = 12;
                var copy = item.Clone(); copy.effects.Add("B"); copy.props[0].name = "changed";
                Equal(1, copy.count); Equal(12, item.count); Equal(1, item.effects.Count); Equal("A", item.props[0].name);
            });
            Test("Synchronization preserves each inventory quantity", () =>
            {
                var item = MakeItem(); var copy = item.Clone(); copy.count = 8;
                item.effects[0] = "updated"; item.SyncModelFrom(item.Clone()); copy.SyncModelFrom(item);
                Equal(8, copy.count); Equal("updated", copy.effects[0]);
                item.effects.Clear(); Equal(1, copy.effects.Count);
            });
            Test("Editing replaces effects and respects removals", () =>
            {
                var popup = new ManageItemPopup();
                var effects = new RectTransform();
                var effect = new DnD.UI.Inventory.EffectItem(); effect.Initialize("A"); effect.transform.SetParent(effects);
                Set(popup, "effectsContainer", effects); Set(popup, "effectField", new TMPro.TMP_InputField());
                var item = MakeItem(); Call(popup, "FillEffects", item); Call(popup, "FillEffects", item);
                Equal(1, item.effects.Count);
                effect.gameObject.SetActive(false); Call(popup, "FillEffects", item);
                Equal(0, item.effects.Count);
                Get<TMPro.TMP_InputField>(popup, "effectField").text = "B"; Call(popup, "FillEffects", item);
                Equal("B", item.effects.Single());
            });
            Test("Template editor hides quantity and ignores its input", () =>
            {
                var popup = new ManageItemPopup(); Set(popup, "templateMode", true);
                foreach (var field in typeof(ManageItemPopup).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (field.FieldType == typeof(TMPro.TMP_InputField)) field.SetValue(popup, new TMPro.TMP_InputField());
                    if (field.FieldType == typeof(TMPro.TMP_Dropdown)) field.SetValue(popup, new TMPro.TMP_Dropdown());
                }
                Set(popup, "countContainer", new GameObject()); Set(popup, "effectsContainer", new RectTransform());
                Get<TMPro.TMP_Dropdown>(popup, "typeDropdown").value = (int)EItemType.Consumable;
                Get<TMPro.TMP_InputField>(popup, "countField").text = "999";
                popup.OnTypeDropdownSelected((int)EItemType.Consumable);
                Equal(false, Get<GameObject>(popup, "countContainer").activeSelf);
                Equal(1, ((Item)Call(popup, "CreateItem")).count);
                var item = MakeItem(); item.count = 8; Call(popup, "UpdateItem", item); Equal(1, item.count);
            });
            Test("Skills use independent proficiency and bonuses", () =>
            {
                var hero = new CharacterData { masteryBonus = 3 };
                hero.dexterity.baseValue = 14;
                hero.throws.dexterity.master = true; hero.throws.dexterity.modifier = 7;
                hero.skills.attributes["Акробатика"].modifier = 1;
                hero.Recalculate();
                Equal(3, hero.skills.attributes["Акробатика"].baseValue);
                Equal(false, hero.skills.attributes["Акробатика"].master);
                Equal(12, hero.throws.dexterity.baseValue);
                hero.skills.attributes["Акробатика"].master = true; hero.Recalculate();
                Equal(6, hero.skills.attributes["Акробатика"].baseValue);
            });
            Test("Selected abilities highlight exactly their related skills", () =>
            {
                var hero = new CharacterData();
                var groups = new (ThrowAttribute savingThrow, string[] skills)[]
                {
                    (hero.throws.strength, new[] { "Атлетика" }),
                    (hero.throws.dexterity, new[] { "Акробатика", "Ловкость рук", "Скрытность" }),
                    (hero.throws.constitution, Array.Empty<string>()),
                    (hero.throws.intelligence, new[] { "Анализ", "История", "Магия", "Природа", "Религия" }),
                    (hero.throws.wisdom, new[] { "Внимательность", "Выживание", "Медицина", "Проницательность", "Уход за животными" }),
                    (hero.throws.charisma, new[] { "Выступление", "Запугивание", "Обман", "Убеждение" })
                };
                foreach (var group in groups)
                {
                    group.savingThrow.master = true;
                    hero.Recalculate();
                    foreach (var skill in hero.skills.attributes)
                    {
                        Equal(group.skills.Contains(skill.Key), SkillAttributes.IsRelatedAbilitySelected(hero, skill.Key));
                        Equal(false, skill.Value.master);
                        Equal(0, skill.Value.baseValue);
                    }
                    group.savingThrow.master = false;
                }
                Equal(false, SkillAttributes.IsRelatedAbilitySelected(hero, "Unknown skill"));
                Equal(false, SkillAttributes.IsRelatedAbilitySelected(null, "Убеждение"));
            });
            Test("Skill border follows selected ability, independent proficiency and character changes", () =>
            {
                var hero = new CharacterData(); hero.throws.charisma.master = true;
                var skill = hero.skills.attributes["Убеждение"];
                var ui = new SkillItem(); var border = new GameObject();
                Set(ui, "border", border); Set(ui, "valueText", new TMPro.TMP_Text()); Set(ui, "labelText", new TMPro.TMP_Text());
                ui.Invalidate("Убеждение", skill, hero); Check(border.activeSelf);
                Equal(false, skill.master);
                hero.throws.charisma.master = false;
                ui.Invalidate("Убеждение", skill, hero); Equal(false, border.activeSelf);
                skill.master = true;
                ui.Invalidate("Убеждение", skill, hero); Check(border.activeSelf);
                var other = new CharacterData();
                ui.Invalidate("Убеждение", other.skills.attributes["Убеждение"], other);
                Equal(false, border.activeSelf);
            });
            Test("Ability bonus propagates and manual modifier remains supported", () =>
            {
                var hero = new CharacterData(); hero.dexterity.baseValue = 14; hero.dexterity.bonus = 2;
                hero.Recalculate(); Equal(4, hero.skills.attributes["Акробатика"].baseValue);
                Equal(4, hero.throws.dexterity.baseValue);
                hero.dexterity.auto = false; hero.dexterity.modifier = 5; hero.Recalculate();
                Equal(7, hero.skills.attributes["Скрытность"].baseValue);
            });
            Test("Old profile migration preserves totals and runs once", () =>
            {
                var hero = new CharacterData { masteryBonus = 2 };
                hero.dexterity.baseValue = 14; hero.throws.dexterity.master = true; hero.throws.dexterity.modifier = 3;
                var skill = hero.skills.attributes["Акробатика"]; skill.master = true; skill.modifier = 1;
                var old = JObject.Parse(JsonStorage.Serialize(hero)); old.Remove("dataVersion");
                var migrated = JsonStorage.Deserialize<CharacterData>(old.ToString());
                Equal(8, migrated.skills.attributes["Акробатика"].baseValue);
                Equal(4, migrated.skills.attributes["Акробатика"].modifier);
                var again = JsonStorage.Deserialize<CharacterData>(JsonStorage.Serialize(migrated));
                Equal(8, again.skills.attributes["Акробатика"].baseValue);
                Equal(4, again.skills.attributes["Акробатика"].modifier);
            });
            Test("Round trip preserves every tab subtype, skill and quantity", () =>
            {
                var hero = new CharacterData(); hero.items.Add(MakeItem()); hero.items[0].count = 9;
                hero.skills.attributes["Магия"].modifier = 4; hero.dexterity.bonus = 2;
                hero.tabs.Add(new TabData { name = "Tab", items = new()
                {
                    new TitleContent { title = "T" },
                    new DescriptionContent { title = "D", description = "Description" },
                    new IconTooltipContent { title = "I", tooltip = "Tooltip", icon_group = 2, icon_id = 3 },
                    new SpellCellsContent { level = 3, maxSlots = 4, usedSlots = 2 }
                }});
                var json = JsonStorage.Serialize(new ExportEnvelope { items = new() { hero } });
                var copy = JsonStorage.Deserialize<ExportEnvelope>(json).items.Single();
                Equal("Description", ((DescriptionContent)copy.tabs[0].items[1]).description);
                Equal("Tooltip", ((IconTooltipContent)copy.tabs[0].items[2]).tooltip);
                Equal("T", ((TitleContent)copy.tabs[0].items[0]).title);
                Equal(2, ((SpellCellsContent)copy.tabs[0].items[3]).usedSlots);
                Equal(4, copy.skills.attributes["Магия"].modifier); Equal(9, copy.items[0].count); Equal(2, copy.dexterity.bonus);
            });
            Test("Missing and null optional collections are normalized", () =>
            {
                var json = JObject.Parse(JsonStorage.Serialize(new CharacterData()));
                json["items"] = null; json["skills"] = null; json["tabs"] = null;
                var copy = JsonStorage.Deserialize<CharacterData>(json.ToString());
                Equal(18, copy.skills.attributes.Count); Equal(0, copy.items.Count); Equal(0, copy.tabs.Count);
            });
            Test("Clipboard export and import use the full profile model", () =>
            {
                SetSaveFolder("clipboard"); var lib = new ItemsLibrary(); lib.InitializeSelf(); var manager = MakeManager();
                var hero = manager.CreateCharacter("Exported", "Race", "Class", true, 0);
                hero.skills.attributes["Магия"].modifier = 5;
                hero.tabs.Add(new TabData { name = "Magic", items = new() { new DescriptionContent { description = "Full text" } } });
                var exporter = new ExportPopup(); Set(exporter, "_items", new List<ToggleCharacter> { Toggle(hero) });
                Check((bool)Call(exporter, "Export"));
                var importer = MakeImporter(); importer.ButtonImport();
                var imported = Get<List<ToggleCharacter>>(importer, "_items").Single().Character;
                Equal(5, imported.skills.attributes["Магия"].modifier);
                Equal("Full text", ((DescriptionContent)imported.tabs[0].items[0]).description);
                importer.ButtonImport(); // A second paste must replace rather than append.
                Equal(1, Get<List<ToggleCharacter>>(importer, "_items").Count);
                hero.name = "Local changes";
                Check((bool)Call(importer, "SaveItems"));
                Equal("Exported", manager.GetCharacterById(hero.id).name);
                GUIUtility.systemCopyBuffer = "{broken"; importer.ButtonImport();
                Check(Get<GameObject>(importer, "errorText").activeSelf);
                Equal(false, Get<List<ToggleCharacter>>(importer, "_items")[0].gameObject.activeSelf);
            });
            Test("Partial import reloads successful writes before reporting failure", () =>
            {
                SetSaveFolder("partial-import"); var lib = new ItemsLibrary(); lib.InitializeSelf(); var manager = MakeManager();
                var current = manager.CreateCharacter("Old", "Race", "Class", true, 0);
                manager.SetActiveCharacter(current);
                var imported = JsonStorage.Deserialize<CharacterData>(JsonStorage.Serialize(current)); imported.name = "Imported";
                var blocked = new CharacterData(); Directory.CreateDirectory(HeroPath(blocked));
                var importer = MakeImporter(); Set(importer, "_items", new List<ToggleCharacter> { Toggle(imported), Toggle(blocked) });
                Equal(false, (bool)Call(importer, "SaveItems"));
                Equal("Imported", manager.ActiveCharacter.name);
                manager.SaveAll(); Equal("Imported", CharacterData.Load(current.id).name);
            });
            Test("Invalid IDs and unknown content fail before writing", () =>
            {
                var json = JObject.Parse(JsonStorage.Serialize(new CharacterData())); json["id"] = "../bad";
                Throws(() => JsonStorage.Deserialize<CharacterData>(json.ToString()));
                Throws(() => JsonStorage.Deserialize<DnD.Model.TabContent>("{\"contentType\":42}"));
                Throws(() => JsonStorage.Deserialize<DnD.Model.TabContent>("{}"));
            });
            Test("Atomic writes rotate a readable backup", () =>
            {
                var file = Path.Combine(TestRoot, "atomic.json");
                Check(JsonStorage.Write(file, new Payload { value = "old" }));
                Check(JsonStorage.Write(file, new Payload { value = "new" }));
                Equal("new", JsonStorage.Deserialize<Payload>(File.ReadAllText(file)).value);
                Equal("old", JsonStorage.Deserialize<Payload>(File.ReadAllText(file + ".bak")).value);
                Equal(false, File.Exists(file + ".tmp"));
            });
            Test("Failed write reports failure without replacing existing data", () =>
            {
                var file = Path.Combine(TestRoot, "locked.json");
                JsonStorage.Write(file, new Payload { value = "original" });
                using (var locked = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None))
                    Equal(false, JsonStorage.Write(file, new Payload { value = "replacement" }));
                Equal("original", JsonStorage.Deserialize<Payload>(File.ReadAllText(file)).value);
            });
            Test("Corrupt main recovers backup and preserves corrupt bytes", () =>
            {
                var file = Path.Combine(TestRoot, "recovery.json");
                JsonStorage.Write(file, new Payload { value = "backup" });
                JsonStorage.Write(file, new Payload { value = "latest" });
                File.WriteAllText(file, "{broken");
                var copy = JsonStorage.Read(file, JsonStorage.Deserialize<Payload>);
                Equal("backup", copy.value); Equal(false, File.Exists(file));
                Equal("{broken", File.ReadAllText(Directory.GetFiles(TestRoot, "recovery.json.corrupt_*").Single()));
                Check(JsonStorage.Write(file, copy));
                Equal("backup", JsonStorage.Deserialize<Payload>(File.ReadAllText(file + ".bak")).value);
            });
            Test("Damaged profile cannot block other profiles", () =>
            {
                SetSaveFolder("profiles");
                var good = new CharacterData { name = "Good" }; Check(good.Save());
                File.WriteAllText(Path.Combine(Application.persistentDataPath, "characters", "hero_" + Guid.NewGuid() + ".json"), "{broken");
                Equal("Good", CharacterData.LoadAll().Single().name);
            });
            Test("Backup-only profile loads and deletion cannot resurrect it", () =>
            {
                SetSaveFolder("backup-only"); var hero = new CharacterData(); hero.Save(); hero.Save();
                var file = HeroPath(hero); File.Delete(file);
                Equal(hero.id, CharacterData.LoadAll().Single().id);
                Check(hero.Delete()); Equal(0, CharacterData.LoadAll().Count);
            });
            Test("Library mutations persist immediately and templates have quantity one", () =>
            {
                SetSaveFolder("library"); var lib = new ItemsLibrary(); lib.InitializeSelf();
                var manager = MakeManager(); var hero = manager.CreateCharacter("Hero", "Race", "Class", true, 0);
                var template = MakeItem(); template.count = 20; lib.AddItem(template);
                var copy = template.Clone(); copy.count = 7; hero.items.Add(copy);
                Equal(1, template.count);
                Equal(1, JsonStorage.Deserialize<List<Item>>(File.ReadAllText(Path.Combine(Application.persistentDataPath, "items.json"))).Single().count);
                template.effects[0] = "edited"; lib.UpdateModel(template);
                Equal(1, template.effects.Count); Equal("edited", copy.effects[0]); Equal(7, copy.count);
                copy.effects[0] = "from inventory"; lib.UpdateModel(copy);
                Equal("from inventory", template.effects[0]); Equal(7, copy.count);
                lib.DeleteItem(template);
                Equal(0, JsonStorage.Deserialize<List<Item>>(File.ReadAllText(Path.Combine(Application.persistentDataPath, "items.json"))).Count);
            });
            Test("Deleting last selected character publishes null and clears preference", () =>
            {
                SetSaveFolder("delete-last"); var lib = new ItemsLibrary(); lib.InitializeSelf(); var manager = MakeManager();
                var hero = manager.CreateCharacter("Hero", "Race", "Class", true, 0); manager.SetActiveCharacter(hero);
                var other = manager.CreateCharacter("Other", "Race", "Class", true, 0); other.isSelected = false; other.Save();
                var called = false; manager.OnSetActiveCharacter += character => { called = true; Equal<CharacterData>(null, character); };
                manager.Delete(hero);
                Check(called); Equal<CharacterData>(null, manager.ActiveCharacter); Equal(0, manager.AmountOfActiveCharacters);
                Equal("", PlayerPrefs.GetString("ActiveCharacterId", "missing")); Equal(1, manager.GetCharacters().Count);
            });
            Test("Fresh startup selects an available character", () =>
            {
                SetSaveFolder("startup"); var hero = new CharacterData { isSelected = true }; hero.Save();
                PlayerPrefs.SetString("ActiveCharacterId", "missing");
                var manager = MakeManager(); Equal(hero.id, manager.ActiveCharacter.id);
            });
            Test("Inventory capacity includes last item despite seven internal holes", () =>
            {
                SetSaveFolder("inventory"); var hero = new CharacterData();
                for (var i = 0; i < 29; i++) hero.items.Add(i < 7 ? null : MakeItem());
                var lastId = hero.items[28].ID;
                var inventory = new DnD.UI.Inventory.Inventory();
                Set(inventory, "itemPrefab", new DnD.UI.Inventory.InventoryItem());
                inventory.Invalidate(hero); inventory.Save();
                Check(hero.items.Any(item => item?.ID == lastId));
                Check(CharacterData.Load(hero.id).items.Any(item => item?.ID == lastId));
                Set(inventory, "cols", 5); Set(inventory, "minRows", 1);
                var rows = (int)Call(inventory, "GetNeededRows", hero.items);
                Check(rows * 5 >= 29);
            });
            Test("Rebuilt dynamic tabs select a real panel and preserve edit mode", () =>
            {
                SetSaveFolder("tabs"); var group = MakeTabGroup();
                var hero = new CharacterData(); hero.tabs.Add(new TabData { name = "First", index = 3 });
                Call(group, "Invalidate", hero); group.SetEditable(true);
                var buttons = Get<List<TitleButton>>(group, "tabButtons");
                group.SelectTab(buttons[0].GetComponent<TabButton>());
                var oldPanel = Get<List<CharacterTabContent>>(group, "tabContents")[0];
                Call(group, "Invalidate", hero);
                var samePanel = Get<List<CharacterTabContent>>(group, "tabContents")[0];
                Check(!ReferenceEquals(oldPanel, samePanel)); Check(samePanel.gameObject.activeSelf); Check(samePanel.Editable);
                var other = new CharacterData(); other.tabs.Add(new TabData { name = "Other", index = 3 });
                Call(group, "Invalidate", other);
                Check(Get<TabButton>(group, "defaultSelectedTab").Selected);
                Check(Get<List<CharacterTabContent>>(group, "tabContents")[0].Editable);
                Call(group, "CreateTab", "Saved immediately");
                Equal(2, CharacterData.Load(other.id).tabs.Count);
                Check(Get<List<CharacterTabContent>>(group, "tabContents")[1].gameObject.activeSelf);
            });
            Console.WriteLine($"PASS: {passed} regression scenarios.");
        }
        finally
        {
            // Only delete the explicitly created unique test directory.
            if (TestRoot.StartsWith(Path.Combine(Path.GetTempPath(), "DnD-Regression-"), StringComparison.OrdinalIgnoreCase))
                Directory.Delete(TestRoot, true);
        }
    }
    private static void Test(string name, Action action) { action(); passed++; Console.WriteLine("PASS " + name); }
    private static void Check(bool value) { if (!value) throw new Exception("Assertion failed"); }
    private static void Equal<T>(T expected, T actual) { if (!Equals(expected, actual)) throw new Exception($"Expected {expected}, got {actual}"); }
    private static void Throws(Action action) { try { action(); } catch { return; } throw new Exception("Expected an exception"); }
    private static Item MakeItem()
    {
        var item = Item.CreateDummy(); item.type = EItemType.Consumable; item.effects.Add("A"); item.props.Add(new ItemProp { name = "A" }); return item;
    }
    private static void SetSaveFolder(string name) { Application.persistentDataPath = Path.Combine(TestRoot, name); Directory.CreateDirectory(Application.persistentDataPath); }
    private static string HeroPath(CharacterData hero) => Path.Combine(Application.persistentDataPath, "characters", "hero_" + hero.id + ".json");
    private static CharacterManager MakeManager()
    {
        var manager = new CharacterManager(); Set(manager, "leftButton", new Button()); Set(manager, "rightButton", new Button()); manager.InitializeSelf(); return manager;
    }
    private static ToggleCharacter Toggle(CharacterData hero) { var toggle = new ToggleCharacter(); toggle.Init(hero); toggle.SetToggle(true); return toggle; }
    private static ImportPopup MakeImporter()
    {
        var popup = new ImportPopup(); Set(popup, "togglePrefab", new ToggleCharacter()); Set(popup, "content", new RectTransform());
        Set(popup, "errorText", new GameObject()); Set(popup, "tooltipText", new GameObject()); return popup;
    }
    private static CharacterTabGroup MakeTabGroup()
    {
        var group = new CharacterTabGroup(); var container = new RectTransform(); container.SetParent(group.transform);
        Set(group, "content", container); Set(group, "contentContainer", new RectTransform());
        var defaultButton = new TabButton(); group.Register(defaultButton); defaultButton.transform.SetParent(container);
        var panel = new GameObject();
        Set(group, "defaultSelectedTab", defaultButton);
        Set(group, "contents", new List<DnD.UI.TabContent> { new() { Button = defaultButton, Content = panel } });
        Set(group, "tabButtonPrefab", new TitleButton()); Set(group, "tabContentPrefab", new CharacterTabContent());
        var add = new RectTransform(); add.SetParent(container); Set(group, "buttonAdd", add);
        var manage = new RectTransform(); manage.SetParent(container); Set(group, "buttonManage", manage);
        return group;
    }
    private static FieldInfo Field(object target, string name)
    {
        for (var type = target.GetType(); type != null; type = type.BaseType)
            if (type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance) is FieldInfo field) return field;
        throw new MissingFieldException(name);
    }
    private static void Set(object target, string name, object value) => Field(target, name).SetValue(target, value);
    private static T Get<T>(object target, string name) => (T)Field(target, name).GetValue(target);
    private static object Call(object target, string name, params object[] values) => target.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, values);
    private class Payload { public string value; }
    private class ExportEnvelope { public List<CharacterData> items; }
}
