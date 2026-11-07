using System;
using System.Collections.Generic;
using DnD.Model;
using UnityEngine;

namespace DnD.UI.Spells
{
    public class CharacterSpellsTab : MonoBehaviour
    {
        [SerializeField] private SpellListView listView;
        [SerializeField] private Transform toolbar;
        [SerializeField] private GameObject emptyMessage;
        [SerializeField] private SpellLibraryPopup libraryPopup;
        private bool editable;
        private CharacterData character;

        private void Awake()
        {
            CharacterManager.Instance.OnSetActiveCharacter += OnCharacterChanged;
            OnCharacterChanged(CharacterManager.Instance.ActiveCharacter);
        }

        private void OnEnable()
        {
            if (CharacterManager.Instance != null)
                OnCharacterChanged(CharacterManager.Instance.ActiveCharacter);
        }

        private void OnDestroy()
        {
            if (CharacterManager.Instance != null)
                CharacterManager.Instance.OnSetActiveCharacter -= OnCharacterChanged;
        }

        public void SetEditable(bool value)
        {
            editable = value;
            toolbar.gameObject.SetActive(value);
            listView.SetEditable(value, Reorder);
        }

        private void Reorder(string id, int index)
        {
            if (!editable || !ReferenceEquals(character, CharacterManager.Instance.ActiveCharacter)) 
                return;
            CharacterSpellSelection.Move(character, id, index);
            Refresh();
        }

        private void OnCharacterChanged(CharacterData data) 
        { 
            character = data; 
            Refresh(); 
        }

        private void Refresh()
        {
            var catalog = SpellLibrary.Catalog;
            var entries = character != null && catalog != null
                ? SpellListEntry.ForCharacter(character, catalog) : new List<SpellListEntry>();
            listView.SetItems(entries, Remove);
            SetEditable(editable);
            emptyMessage.SetActive(character != null && character.spellIds.Count == 0);
        }

        public void ButtonAdd()
        {
            if (character != null && SpellLibrary.Catalog != null)
                libraryPopup.Open(character, Refresh);
        }

        private void Remove(SpellListEntry entry)
        {
            if (ReferenceEquals(character, CharacterManager.Instance.ActiveCharacter)
                && CharacterSpellSelection.Remove(character, entry.Id)) Refresh();
        }
    }
}
