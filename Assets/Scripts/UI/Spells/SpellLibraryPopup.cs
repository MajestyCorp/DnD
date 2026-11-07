using System;
using System.Collections.Generic;
using DnD.Model;
using DnD.Model.Spells;
using DnD.Profiles;
using TMPro;
using UnityEngine;

namespace DnD.UI.Spells
{
    public class SpellLibraryPopup : Frame
    {
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private TMP_Dropdown classFilter;
        [SerializeField] private TMP_Dropdown levelFilter;
        [SerializeField] private TMP_Dropdown subclassFilter;
        [SerializeField] private GameObject subclassFilterPanel;
        private readonly CharacterSubclassDropdown subclassChoice = new();
        [SerializeField] private CharacterOptionsProfile optionsProfile;
        [SerializeField] private SpellListView listView;
        [SerializeField] private GameObject emptyMessage;
        [SerializeField] private TMP_Text selectedCountText;
        private SpellSelectionDraft draft;
        private readonly List<string> classIds = new();
        private CharacterData character;
        private Action onChanged;
        private bool searchPending;
        private float searchAt;

        private bool initialized = false;

        protected override void Awake()
        {
            base.Awake();
            searchInput.onValueChanged.AddListener(OnSearchChanged);
            classFilter.onValueChanged.AddListener(OnClassChanged);
            if (levelFilter != null) levelFilter.onValueChanged.AddListener(OnLevelChanged);
            if (subclassFilter != null) subclassFilter.onValueChanged.AddListener(OnLevelChanged);
            CharacterManager.Instance.OnSetActiveCharacter += OnCharacterChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            searchInput.onValueChanged.RemoveListener(OnSearchChanged);
            classFilter.onValueChanged.RemoveListener(OnClassChanged);
            if (levelFilter != null) levelFilter.onValueChanged.RemoveListener(OnLevelChanged);
            if (subclassFilter != null) subclassFilter.onValueChanged.RemoveListener(OnLevelChanged);
            if (CharacterManager.Instance != null)
                CharacterManager.Instance.OnSetActiveCharacter -= OnCharacterChanged;
        }

        protected void Initialize()
        {
            if (initialized)
                return;

            var names = new List<string> { "Все" };
            classIds.Clear();
            classIds.Add(null);

            if (optionsProfile != null)
                foreach (var option in optionsProfile.Classes)
                    if (option != null)
                        AddClass(option.id, option.displayName, names);

            foreach (var entry in SpellLibrary.Catalog.Data.classes)
                AddClass(entry.id, entry.GetName(SpellLibrary.Language), names);

            // Non-casters and custom classes must still default to their own (possibly empty) list.
            AddClass(character.characterClassId, character.characterClass, names);
            classFilter.ClearOptions();
            classFilter.AddOptions(names);
            classFilter.RefreshShownValue();

            if (levelFilter != null)
            {
                var levels = new List<string> { "Все", "Заговор" };
                for (var level = 1; level <= 9; level++)
                    levels.Add($"{level} круг");
                levelFilter.ClearOptions();
                levelFilter.AddOptions(levels);
                levelFilter.RefreshShownValue();
            }

            initialized = true;
        }

        public void Open(CharacterData character, Action onChanged)
        {
            if (character == null || SpellLibrary.Catalog == null) 
                return;

            draft?.Cancel();
            draft = new SpellSelectionDraft(character, SpellLibrary.Catalog);
            this.character = character;
            this.onChanged = onChanged;

            Initialize();

            searchInput.SetTextWithoutNotify("");
            classFilter.SetValueWithoutNotify(Mathf.Max(0, classIds.IndexOf(character.characterClassId)));
            RefreshSubclassFilter(character.characterSubclassId);
            if (levelFilter != null) levelFilter.SetValueWithoutNotify(0);
            searchPending = false;
            Show();
            Refresh();
        }

        private void AddClass(string id, string name, List<string> names)
        {
            if (string.IsNullOrWhiteSpace(id) || classIds.Contains(id)) return;
            classIds.Add(id);
            names.Add(string.IsNullOrWhiteSpace(name) ? "Класс персонажа" : name);
        }

        private void OnCharacterChanged(CharacterData data)
        {
            if (!ReferenceEquals(character, data)) Hide();
        }

        protected override void OnHideAction() 
        { 
            base.OnHideAction(); 
            searchPending = false; 
            draft?.Cancel(); 
        }

        private void OnClassChanged(int index) 
        { 
            RefreshSubclassFilter();
            searchPending = false; 
            Refresh(); 
        }

        private void RefreshSubclassFilter(string selectedId = null)
        {
            var classId = classIds.Count == 0 ? null : classIds[Mathf.Clamp(classFilter.value, 0, classIds.Count - 1)];
            subclassChoice.Initialize(subclassFilter, subclassFilterPanel, classId, selectedId, "Без подкласса");
        }

        private void OnLevelChanged(int index)
        {
            searchPending = false;
            Refresh();
        }

        private void OnSearchChanged(string text)
        { 
            searchPending = true; 
            searchAt = Time.unscaledTime + 0.15f; 
        }

        private void Update()
        {
            if (IsVisible && searchPending && Time.unscaledTime >= searchAt)
            {
                searchPending = false;
                Refresh();
            }
        }

        private void Refresh() 
        { 
            Render(); 
        }

        private void Render()
        {
            if (character == null || SpellLibrary.Catalog == null || classIds.Count == 0) 
                return;

            var classId = classIds[Mathf.Clamp(classFilter.value, 0, classIds.Count - 1)];
            var found = SpellLibrary.Catalog.Search(classId, searchInput.text, SpellLibrary.Language, subclassChoice.SelectedId);
            // Dropdown index 0 means all levels; indices 1..10 map to spell levels 0..9.
            var selectedLevel = levelFilter != null ? levelFilter.value - 1 : -1;
            if (selectedLevel >= 0)
                found.RemoveAll(spell => spell.level != selectedLevel);
            var entries = new List<SpellListEntry>();

            foreach (var spell in found)
                entries.Add(new SpellListEntry 
                { 
                    Id = spell.id, 
                    Spell = spell, 
                    Selected = draft.Contains(spell.id), 
                    UseStarSelection = true 
                });

            listView.SetItems(entries, ToggleSelection);
            if (selectedCountText != null) 
                selectedCountText.text = $"Выбрано: {draft.Count}";

            emptyMessage.SetActive(found.Count == 0);
        }

        private void ToggleSelection(SpellListEntry entry)
        {
            if (!ReferenceEquals(character, CharacterManager.Instance.ActiveCharacter)) 
            { 
                Hide(); 
                return; 
            }
            if (draft != null && draft.Toggle(entry.Id)) 
                Render();
        }

        public void ButtonApplySelection()
        {
            if (!ReferenceEquals(character, CharacterManager.Instance.ActiveCharacter) || draft == null || !draft.IsActive)
            {
                Hide();
                return;
            }

            if (!draft.Apply()) 
                return;

            onChanged?.Invoke();
            Hide();
        }
    }
}
