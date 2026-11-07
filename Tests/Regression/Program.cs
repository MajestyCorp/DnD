using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DnD;
using DnD.Model;
using DnD.Model.Inventory;
using DnD.Profiles;
using DnD.Model.Spells;
using DnD.UI.Spells;
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
            var realCatalog = new SpellCatalog(JsonStorage.Deserialize<SpellCatalogData>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "spells.json"))));
            SpellLibrary.Catalog = realCatalog;
            Test("Saved spell order is preserved across different spell levels and reloading", () =>
            {
                SetSaveFolder("spell-order"); var hero = new CharacterData { spellIds = new() { "spell.wish", "spell.fireball", "missing-id" } };
                Check(hero.Save()); Check(CharacterSpellSelection.Move(hero, "spell.fireball", 0));
                Equal("spell.fireball", CharacterData.Load(hero.id).spellIds[0]);
                Check(CharacterSpellSelection.Move(hero, "spell.fireball", 2));
                Equal("spell.wish", SpellListEntry.ForCharacter(hero, realCatalog)[0].Id);
                Equal("spell.fireball", SpellListEntry.ForCharacter(hero, realCatalog)[2].Id);
                Equal(false, CharacterSpellSelection.Move(hero, "not-present", 0));
                var blocked = new CharacterData { spellIds = new() { "spell.wish", "spell.fireball" } }; Directory.CreateDirectory(HeroPath(blocked));
                Equal(false, CharacterSpellSelection.Move(blocked, "spell.fireball", 0)); Equal("spell.wish", blocked.spellIds[0]);
            });
            Test("Drag handle is edit-only and mid-drag cancellation restores the card and scroll", () =>
            {
                SetSaveFolder("spell-drag-cancel"); var hero = new CharacterData { spellIds = new() { "spell.fireball", "spell.cure-wounds", "spell.wish" } };
                var setup = MakeDraggableSpells(hero, realCatalog);
                var card = setup.rows[0]; var drag = card.GetComponent<SpellCardDrag>(); var handle = Get<GameObject>(drag, "dragHandle");
                Equal(false, handle.activeSelf); Equal(false, drag.CanPerformAction(EDropAction.Drag));
                setup.view.SetEditable(true, null); Check(handle.activeSelf);
                var data = new UnityEngine.EventSystems.PointerEventData { position = new Vector2(0, 300) };
                drag.OnBeginDrag(data); Equal(false, setup.scroll.enabled); Check(card.transform.parent != setup.parent);
                setup.view.SetEditable(false, null); Check(card.transform.parent == setup.parent); Check(setup.scroll.enabled);
                Equal(3, setup.parent.childCount); Equal(0, card.transform.GetSiblingIndex()); Equal(false, handle.activeSelf);
            });
            Test("Dragging before a later sibling saves the intended order without off-by-one", () =>
            {
                SetSaveFolder("spell-drag-order"); var hero = new CharacterData { spellIds = new() { "spell.fireball", "spell.cure-wounds", "spell.wish" } };
                hero.Save(); var setup = MakeDraggableSpells(hero, realCatalog); var moved = 0;
                setup.view.SetEditable(true, (id,index) => { moved++; CharacterSpellSelection.Move(hero,id,index); });
                var drag = setup.rows[0].GetComponent<SpellCardDrag>();
                var data = new UnityEngine.EventSystems.PointerEventData { position = new Vector2(0, 300) };
                drag.OnBeginDrag(data); data.position = new Vector2(0, 80); drag.OnDrag(data); drag.OnEndDrag(data);
                Equal(1, moved); Equal("spell.cure-wounds", hero.spellIds[0]); Equal("spell.fireball", hero.spellIds[1]); Equal("spell.wish", hero.spellIds[2]);
                Equal("spell.fireball", CharacterData.Load(hero.id).spellIds[1]);
            });
            Test("DropArea excludes only the dragged character spell and trailing end-drag is harmless", () =>
            {
                SetSaveFolder("spell-drop-exclude"); var hero = new CharacterData { spellIds = new() { "spell.fireball", "spell.cure-wounds", "spell.wish" } }; hero.Save();
                var removed = 0; var setup = MakeDraggableSpells(hero, realCatalog, entry => { if (CharacterSpellSelection.Remove(hero, entry.Id)) removed++; });
                var moved = 0; setup.view.SetEditable(true, (id,index) => moved++);
                var area = new DropArea(); area.gameObject.AddComponent<Button>(); Set(area, "action", EDropAction.Delete); Call(area, "Awake");
                var drag = setup.rows[1].GetComponent<SpellCardDrag>(); var handle = Get<GameObject>(drag, "dragHandle");
                var data = new UnityEngine.EventSystems.PointerEventData { position = new Vector2(0,200), pointerDrag = handle };
                drag.OnBeginDrag(data); Check(area.GetComponent<Button>().interactable); area.OnDrop(data); drag.OnEndDrag(data);
                Equal(1, removed); Equal(0, moved); Equal(2, hero.spellIds.Count); Check(realCatalog.Find("spell.cure-wounds") != null);
                Equal(false, area.GetComponent<Button>().interactable); Check(setup.scroll.enabled); Call(area, "OnDestroy");
            });
            Test("Question tooltip contains the full description and the two prefabs have distinct controls", () =>
            {
                var spell = realCatalog.Find("spell.fireball"); var card = new SpellRowUI(); Set(card, "titleText", new TMPro.TMP_Text());
                Set(card, "detailsButton", new Button()); Set(card, "useTextTooltip", true);
                card.Bind(new SpellListEntry { Id=spell.id, Spell=spell }, null, null); card.ButtonDetails();
                Check(TooltipManager.Instance.Text.Contains(spell.GetText("ru").description));
                var libraryPrefab = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "SpellLibraryCard.prefab"));
                var tabPrefab = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "SpellTabCard.prefab"));
                Check(!libraryPrefab.Contains("SpellCardDrag")); Check(!libraryPrefab.Contains("DnD.UI.Tabs.DragButton")); Check(!libraryPrefab.Contains("m_Name: RemoveButton"));
                Check(tabPrefab.Contains("SpellCardDrag")); Check(tabPrefab.Contains("DnD.UI.Tabs.DragButton")); Check(tabPrefab.Contains("m_Name: QuestionButton"));
                Check(tabPrefab.Contains("actionButton: {fileID: 0}")); Check(!tabPrefab.Contains("m_Name: StarredToggle"));
            });
            Test("Starred draft survives every filter and applies all marked spells together", () =>
            {
                SetSaveFolder("starred-filters"); var manager = MakeManager();
                var hero = manager.CreateCharacter("Mage", "Race", "Wizard", true, 0, "human", "wizard"); manager.SetActiveCharacter(hero);
                var popup = MakeSpellPopup(); popup.Open(hero, null);
                Call(popup, "ToggleSelection", new SpellListEntry { Id = "spell.fireball" });
                var filters = Get<List<string>>(popup, "classIds");
                Get<TMPro.TMP_Dropdown>(popup, "classFilter").value = filters.IndexOf("cleric");
                Get<TMPro.TMP_InputField>(popup, "searchInput").text = "cure wounds"; Call(popup, "Refresh");
                Call(popup, "ToggleSelection", new SpellListEntry { Id = "spell.cure-wounds" });
                Get<TMPro.TMP_InputField>(popup, "searchInput").text = "no matching spell"; Call(popup, "Refresh");
                Equal(0, hero.spellIds.Count); Equal(2, Get<SpellSelectionDraft>(popup, "draft").Count);
                popup.ButtonApplySelection();
                var loaded = CharacterData.Load(hero.id);
                Equal(2, loaded.spellIds.Count); Check(loaded.spellIds.Contains("spell.fireball")); Check(loaded.spellIds.Contains("spell.cure-wounds"));
            });
            Test("Unstarring removes existing spells only on confirmation; closing cancels", () =>
            {
                SetSaveFolder("starred-cancel"); var manager = MakeManager();
                var hero = manager.CreateCharacter("Mage", "Race", "Wizard", true, 0); manager.SetActiveCharacter(hero);
                CharacterSpellSelection.Add(hero, realCatalog, "spell.fireball");
                var popup = MakeSpellPopup(); popup.Open(hero, null);
                Check(Get<SpellSelectionDraft>(popup, "draft").Contains("spell.fireball"));
                Call(popup, "ToggleSelection", new SpellListEntry { Id = "spell.fireball" });
                Equal(1, hero.spellIds.Count); popup.Hide(); Equal(1, hero.spellIds.Count);
                popup.Open(hero, null); Check(Get<SpellSelectionDraft>(popup, "draft").Contains("spell.fireball"));
                Call(popup, "ToggleSelection", new SpellListEntry { Id = "spell.fireball" }); popup.ButtonApplySelection();
                Equal(0, CharacterData.Load(hero.id).spellIds.Count);
            });
            Test("Draft de-duplicates IDs, preserves unavailable spells and rolls back failed saves", () =>
            {
                SetSaveFolder("starred-failure"); var hero = new CharacterData();
                hero.spellIds.Add("missing-id"); hero.spellIds.Add("missing-id");
                var draft = new SpellSelectionDraft(hero, realCatalog); Equal(1, draft.Count);
                draft.Toggle("spell.fireball"); draft.Toggle("spell.fireball"); draft.Toggle("spell.fireball"); Equal(2, draft.Count);
                Directory.CreateDirectory(HeroPath(hero)); Equal(false, draft.Apply()); Check(draft.IsActive); Equal(2, hero.spellIds.Count);
                Directory.Delete(HeroPath(hero)); Check(draft.Apply()); Equal(2, hero.spellIds.Count); Equal(false, draft.Apply());
                Equal(1, hero.spellIds.Count(id => id == "spell.fireball")); Check(hero.spellIds.Contains("missing-id"));
            });
            Test("Details opened from library change the draft without saving the character", () =>
            {
                SetSaveFolder("starred-details"); var manager = MakeManager(); var hero = manager.CreateCharacter("Mage", "Race", "Class", true, 0); manager.SetActiveCharacter(hero);
                var draft = new SpellSelectionDraft(hero, realCatalog); var popup = new SpellDetailsPopup();
                foreach (var field in typeof(SpellDetailsPopup).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                    if (field.FieldType == typeof(TMPro.TMP_Text)) field.SetValue(popup, new TMPro.TMP_Text());
                popup.Open(realCatalog.Find("spell.fireball"), hero, null, draft); popup.ButtonToggleSelection();
                Check(draft.Contains("spell.fireball")); Equal(0, hero.spellIds.Count);
                draft.Cancel(); popup.ButtonToggleSelection(); Equal(0, hero.spellIds.Count); Equal(false, popup.IsVisible);
            });
            Test("Recycled star cards reset silently and headers keep their root visible", () =>
            {
                var card = new SpellRowUI(); var toggle = new Toggle(); var filled = new GameObject(); var outline = new GameObject();
                Set(card, "titleText", new TMPro.TMP_Text()); Set(card, "starredToggle", toggle); Set(card, "filledStar", filled); Set(card, "outlineStar", outline);
                Set(card, "detailsButton", card.gameObject.AddComponent<Button>()); Call(card, "Awake");
                var actions = 0; Action<SpellListEntry> onStar = entry => actions++;
                card.Bind(new SpellListEntry { Id = "spell.fireball", Spell = realCatalog.Find("spell.fireball"), Selected = true, UseStarSelection = true }, null, onStar);
                Check(toggle.isOn); Equal(0, actions); Check(filled.activeSelf); Equal(false, outline.activeSelf);
                card.Bind(new SpellListEntry { Id = "spell.wish", Spell = realCatalog.Find("spell.wish"), UseStarSelection = true }, null, onStar);
                Equal(false, toggle.isOn); Equal(0, actions); toggle.isOn = true; Equal(1, actions);
                card.Bind(new SpellListEntry { Header = "Заговоры" }, null, null); Check(card.gameObject.activeSelf); Equal(false, toggle.gameObject.activeSelf);
                Call(card, "OnDestroy");
            });
            Test("Real spell catalog loads with valid classes, translations and subclass links", () =>
            {
                Equal(483, realCatalog.Data.spells.Count); Equal(8, realCatalog.Data.classes.Count); Equal(31, realCatalog.Data.subclassLists.Count);
                Check(realCatalog.Data.spells.All(spell => spell.classIds.Count > 0 && spell.texts.ContainsKey("ru") && spell.texts.ContainsKey("en")));
                Check(realCatalog.Data.spells.All(spell => !spell.GetText("ru").description.Contains("<br")));
                Equal("Fireball", realCatalog.Find("spell.fireball").GetText("unknown").name);
                Check(realCatalog.Data.subclassLists.Any(list => list.classId == "cleric" && list.spellIds.Contains("spell.fireball")));
                Check(!realCatalog.Find("spell.fireball").classIds.Contains("cleric"));
            });
            Test("Spell search combines class and bilingual text including yo/e", () =>
            {
                Check(realCatalog.Search("wizard", "FIREBALL").Any(spell => spell.id == "spell.fireball"));
                Check(realCatalog.Search("wizard", "ОГНЕННЫЙ ШАР").Any(spell => spell.id == "spell.fireball"));
                Check(realCatalog.Search("cleric", "fireball").All(spell => spell.id != "spell.fireball"));
                Check(realCatalog.Search(null, "корона звезд").Any(spell => spell.id == "spell.crown-of-stars"));
                Check(realCatalog.Search(null, "Пекло Аганаззара").Any(spell => spell.id == "spell.aganazzar-s-scorcher"));
                Equal(483, realCatalog.Search(null, "").Count); Equal(0, realCatalog.Search("barbarian", "").Count);
                Check(realCatalog.Search(null, "A line of roaring flame").Count > 0);
            });
            Test("Character spell selections are independent and survive save/export", () =>
            {
                SetSaveFolder("character-spells"); var a = new CharacterData(); var b = new CharacterData();
                Check(CharacterSpellSelection.Add(a, realCatalog, "spell.fireball"));
                Equal(false, CharacterSpellSelection.Add(a, realCatalog, "spell.fireball")); Equal(0, b.spellIds.Count);
                Check(CharacterSpellSelection.Add(b, realCatalog, "spell.fireball"));
                Check(CharacterSpellSelection.Remove(a, "spell.fireball")); Equal(1, b.spellIds.Count);
                b.characterClassId = "cleric"; Check(b.Save()); Equal(1, CharacterData.Load(b.id).spellIds.Count);
                b.spellIds.Add("future-or-missing-id"); b.spellIds.Add("spell.fireball"); b.spellIds.Add(null); Check(b.Save());
                var imported = JsonStorage.Deserialize<ExportList>(JsonStorage.Serialize(new ExportList { items = new() { b } })).items.Single();
                Equal(2, imported.spellIds.Count); Check(imported.spellIds.Contains("future-or-missing-id"));
                var entries = SpellListEntry.ForCharacter(imported, realCatalog);
                Check(entries.Any(entry => entry.Id == "future-or-missing-id" && entry.Spell == null && entry.Selected));
                Check(CharacterSpellSelection.Remove(imported, "future-or-missing-id"));
            });
            Test("Failed spell save rolls back the selection", () =>
            {
                SetSaveFolder("failed-spell-save"); var hero = new CharacterData(); Directory.CreateDirectory(HeroPath(hero));
                Equal(false, CharacterSpellSelection.Add(hero, realCatalog, "spell.fireball")); Equal(0, hero.spellIds.Count);
                hero.spellIds.Add("spell.fireball"); Equal(false, CharacterSpellSelection.Remove(hero, "spell.fireball"));
                Equal("spell.fireball", hero.spellIds.Single());
            });
            Test("Spell virtual list reuses a bounded pool and clears hidden rows", () =>
            {
                var view = MakeSpellList(); var entries = SpellListEntry.Group(realCatalog.Search(null, "").Select(spell => new SpellListEntry { Id = spell.id, Spell = spell }));
                view.SetItems(entries, null, null);
                var rows = Get<List<SpellRowUI>>(view, "pool"); Check(rows.Count < 15);
                var scroll = Get<ScrollRect>(view, "scrollRect"); scroll.content.anchoredPosition = new Vector2(0, 5000); Call(view, "Refresh");
                Check(Get<SpellListEntry>(rows[0], "entry") != entries[0]); Equal(entries.Count * 88f, scroll.content.sizeDelta.y);
                view.SetItems(new List<SpellListEntry>(), null, null); Check(rows.All(row => !row.gameObject.activeSelf));
            });
            Test("Selected spell list uses an existing LayoutGroup without changing its toolbar", () =>
            {
                var parent = new RectTransform(); var toolbar = new RectTransform(); toolbar.SetParent(parent);
                var view = new SpellListView(); Set(view, "layoutContent", parent); Set(view, "rowPrefab", new SpellRowUI());
                var entries = SpellListEntry.Group(realCatalog.Search("wizard", "fireball").Select(spell => new SpellListEntry { Id = spell.id, Spell = spell }));
                view.SetItems(entries, null, null);
                var rows = Get<List<SpellRowUI>>(view, "pool"); Equal(entries.Count, rows.Count);
                Check(rows.All(row => row.transform.parent == parent)); Equal(0, toolbar.GetSiblingIndex());
                Check(rows.All(row => row.GetComponent<LayoutElement>().preferredHeight == 88));
                view.SetItems(new List<SpellListEntry>(), null, null); Check(rows.All(row => !row.gameObject.activeSelf));
                view.SetItems(entries, null, null); Equal(entries.Count, rows.Count); Equal(0, toolbar.GetSiblingIndex());
            });
            Test("Spell library defaults to character class and rejects stale character actions", () =>
            {
                SetSaveFolder("spell-popup"); var manager = MakeManager();
                var hero = manager.CreateCharacter("Mage", "Race", "Wizard", true, 0, "human", "wizard"); manager.SetActiveCharacter(hero);
                var popup = MakeSpellPopup(); Call(popup, "Awake"); popup.Open(hero, null);
                var ids = Get<List<string>>(popup, "classIds"); var filter = Get<TMPro.TMP_Dropdown>(popup, "classFilter");
                Equal("wizard", ids[filter.value]);
                Get<TMPro.TMP_InputField>(popup, "searchInput").text = "fireball"; Call(popup, "Refresh");
                Call(popup, "ToggleSelection", new SpellListEntry { Id = "spell.fireball", Spell = realCatalog.Find("spell.fireball") });
                Equal(0, hero.spellIds.Count); popup.ButtonApplySelection();
                Equal("spell.fireball", hero.spellIds.Single());
                var other = manager.CreateCharacter("Fighter", "Race", "Fighter", true, 0, "human", "fighter"); manager.SetActiveCharacter(other);
                Call(popup, "ToggleSelection", new SpellListEntry { Id = "spell.wish", Spell = realCatalog.Find("spell.wish") });
                Equal(1, hero.spellIds.Count); Equal(0, other.spellIds.Count); Equal(false, popup.IsVisible);
                popup.Open(other, null); Equal("fighter", ids[filter.value]);
                filter.value = 0; Call(popup, "Refresh"); Check(Get<TMPro.TMP_Text>(popup, "resultsText").text.Contains("483"));
            });
            Test("Spell details save selection and close when the character changes", () =>
            {
                SetSaveFolder("spell-details"); var manager = MakeManager();
                var hero = manager.CreateCharacter("Mage", "Race", "Wizard", true, 0); manager.SetActiveCharacter(hero);
                var popup = new SpellDetailsPopup();
                foreach (var field in typeof(SpellDetailsPopup).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                    if (field.FieldType == typeof(TMPro.TMP_Text)) field.SetValue(popup, new TMPro.TMP_Text());
                Call(popup, "Awake"); var changed = 0;
                popup.Open(realCatalog.Find("spell.wish"), hero, () => changed++); popup.ButtonToggleSelection();
                Equal(1, changed); Equal("spell.wish", CharacterData.Load(hero.id).spellIds.Single());
                var other = manager.CreateCharacter("Other", "Race", "Class", true, 0); manager.SetActiveCharacter(other);
                Equal(false, popup.IsVisible); popup.ButtonToggleSelection(); Equal(1, changed); Equal(0, other.spellIds.Count);
            });
            Test("New fixed tabs do not overlap rebuilt dynamic tabs", () =>
            {
                var group = MakeTabGroup(); var staticPanels = Get<List<DnD.UI.TabContent>>(group, "contents");
                for (var i=0; i<3; i++) { var button = new TabButton(); button.transform.SetParent(Get<Transform>(group, "content")); group.Register(button); staticPanels.Add(new DnD.UI.TabContent { Button = button, Content = new GameObject() }); }
                var hero = new CharacterData(); hero.tabs.Add(new TabData { name = "A", index = 3 }); hero.tabs.Add(new TabData { name = "B", index = 4 });
                Call(group, "Invalidate", hero); Equal(4, hero.tabs[0].index); Equal(5, hero.tabs[1].index);
            });
            Test("Dropdown choices follow stable IDs through rename and reorder", () =>
            {
                var dropdown = new TMPro.TMP_Dropdown();
                var choice = new CharacterOptionDropdown();
                var wizard = new CharacterOption { id = "wizard", displayName = "Волшебник" };
                var cleric = new CharacterOption { id = "cleric", displayName = "Жрец" };
                choice.Initialize(dropdown, new[] { wizard, cleric }, "wizard");
                Equal("wizard", choice.Selection.id); Equal(1, dropdown.value);
                wizard.displayName = "Wizard";
                choice.Initialize(dropdown, new[] { cleric, wizard }, "wizard");
                Equal("wizard", choice.Selection.id); Equal(2, dropdown.value); Equal("Wizard", dropdown.options[2].text);
                choice.Initialize(dropdown, new[] { wizard }, "Old free text");
                Equal(0, dropdown.value); Equal<CharacterOption>(null, choice.Selection);
            });
            Test("Empty dropdowns and invalid catalog entries cannot become selections", () =>
            {
                var choice = new CharacterOptionDropdown(); var dropdown = new TMPro.TMP_Dropdown();
                choice.Initialize(dropdown, new CharacterOption[]
                {
                    null, new() { id = "", displayName = "Empty ID" },
                    new() { id = "wizard", displayName = "Wizard" }, new() { id = "wizard", displayName = "Duplicate" }
                });
                Equal(2, dropdown.options.Count); Equal<CharacterOption>(null, choice.Selection);
                dropdown.value = 1; Equal("wizard", choice.Selection.id);
                dropdown.value = 99; Equal<CharacterOption>(null, choice.Selection);
                choice.Initialize(null, null); Equal<CharacterOption>(null, choice.Selection);
            });
            Test("Create form saves catalog race/class IDs and display names", () =>
            {
                SetSaveFolder("catalog-create"); var lib = new ItemsLibrary(); lib.InitializeSelf(); var manager = MakeManager();
                var popup = new CreateCharacterPopup();
                Set(popup, "avatar", new Avatar()); Set(popup, "toggleMale", new Toggle()); Set(popup, "inputName", new TMPro.TMP_InputField());
                Set(popup, "raceDropdown", new TMPro.TMP_Dropdown()); Set(popup, "classDropdown", new TMPro.TMP_Dropdown());
                Set(popup, "optionsProfile", MakeCharacterOptions());
                Call(popup, "ResetForm"); Get<TMPro.TMP_InputField>(popup, "inputName").text = "New Hero";
                popup.ButtonCreate(); Equal(0, manager.GetCharacters().Count);
                Get<TMPro.TMP_Dropdown>(popup, "raceDropdown").value = 1; Get<TMPro.TMP_Dropdown>(popup, "classDropdown").value = 1;
                popup.ButtonCreate();
                var hero = CharacterData.Load(manager.ActiveCharacter.id);
                Equal("human", hero.raceId); Equal("Человек", hero.race); Equal("wizard", hero.characterClassId); Equal("Волшебник", hero.characterClass);
            });
            Test("Edit form requires old text to be selected again and restores catalog IDs later", () =>
            {
                SetSaveFolder("catalog-edit");
                var old = new CharacterData { name = "Old Hero", race = "Человек", characterClass = "Волшебник" };
                var popup = new EditCharacterPopup();
                Set(popup, "toggleMale", new Toggle()); Set(popup, "toggleFemale", new Toggle()); Set(popup, "inputName", new TMPro.TMP_InputField());
                Set(popup, "raceDropdown", new TMPro.TMP_Dropdown()); Set(popup, "classDropdown", new TMPro.TMP_Dropdown());
                Set(popup, "optionsProfile", MakeCharacterOptions());
                Call(popup, "InitPopup", old, null);
                Equal(0, Get<TMPro.TMP_Dropdown>(popup, "raceDropdown").value);
                Equal(0, Get<TMPro.TMP_Dropdown>(popup, "classDropdown").value);
                popup.ButtonSave(); Equal<string>(null, old.characterClassId); Check(popup.IsVisible);
                Get<TMPro.TMP_Dropdown>(popup, "raceDropdown").value = 1; Get<TMPro.TMP_Dropdown>(popup, "classDropdown").value = 1;
                popup.ButtonSave(); Equal(false, popup.IsVisible);
                var loaded = CharacterData.Load(old.id); Equal("human", loaded.raceId); Equal("wizard", loaded.characterClassId);
                Call(popup, "InitPopup", loaded, null);
                Equal(1, Get<TMPro.TMP_Dropdown>(popup, "raceDropdown").value);
                Equal(1, Get<TMPro.TMP_Dropdown>(popup, "classDropdown").value);
            });
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
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            Environment.ExitCode = 1;
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
    private static (SpellListView view, RectTransform parent, ScrollRect scroll, List<SpellRowUI> rows) MakeDraggableSpells(CharacterData hero, SpellCatalog catalog, Action<SpellListEntry> remove = null)
    {
        var canvas = new Canvas(); var scroll = new ScrollRect(); scroll.transform.SetParent(canvas.transform);
        var parent = new RectTransform(); parent.SetParent(scroll.transform);
        var view = new SpellListView(); Set(view, "layoutContent", parent);
        var rows = Get<List<SpellRowUI>>(view, "pool");
        for(var i=0;i<hero.spellIds.Count;i++)
        {
            var row = new SpellRowUI(); Set(row, "titleText", new TMPro.TMP_Text()); row.transform.SetParent(parent);
            var rect=(RectTransform)row.transform; rect.rect=new Rect { width=500,height=88 }; rect.position=new Vector3(0,300-i*100,0);
            row.gameObject.AddComponent<CanvasGroup>(); row.gameObject.AddComponent<LayoutElement>(); var drag=row.gameObject.AddComponent<SpellCardDrag>();
            var handle=new GameObject(); handle.transform.SetParent(row.transform); var dragButton=handle.AddComponent<DragButton>(); Call(dragButton,"Awake");
            Set(drag,"dragHandle",handle); rows.Add(row);
        }
        view.SetItems(SpellListEntry.ForCharacter(hero,catalog),null,remove);
        return (view,parent,scroll,rows);
    }
    private static SpellListView MakeSpellList()
    {
        var view = new SpellListView(); Set(view, "scrollRect", new ScrollRect()); Set(view, "rowPrefab", new SpellRowUI()); return view;
    }
    private static SpellLibraryPopup MakeSpellPopup()
    {
        var popup = new SpellLibraryPopup(); Set(popup, "searchInput", new TMPro.TMP_InputField()); Set(popup, "classFilter", new TMPro.TMP_Dropdown());
        Set(popup, "listView", MakeSpellList()); Set(popup, "resultsText", new TMPro.TMP_Text()); Set(popup, "emptyMessage", new GameObject());
        return popup;
    }
    private static CharacterOptionsProfile MakeCharacterOptions()
    {
        var profile = ScriptableObject.CreateInstance<CharacterOptionsProfile>();
        Set(profile, "races", new List<CharacterOption> { new() { id = "human", displayName = "Человек" } });
        Set(profile, "classes", new List<CharacterOption> { new() { id = "wizard", displayName = "Волшебник" } });
        return profile;
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
