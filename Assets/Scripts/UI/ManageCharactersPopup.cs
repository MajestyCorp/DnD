using DnD.Model;
using System.Collections.Generic;
using UnityEngine;

namespace DnD.UI
{
    public class ManageCharactersPopup : Frame
    {
        [SerializeField]
        private ToggleCharacter togglePrefab;
        [SerializeField]
        private RectTransform content;

        protected List<ToggleCharacter> _items = new();

        public void ButtonSelect()
        {
            SoundManager.Instance.PlayClick();
            if (!ApplySelection())
                return;
            Hide();
            ForceShowEvent();
        }

        protected override void OnShowAction()
        {
            base.OnShowAction();

            ClearItems();
            BuildItems();
        }

        private bool ApplySelection()
        {
            CharacterData first = null;

            for(var i=0;i<_items.Count;i++)
            {
                var item = _items[i];

                if (!item.gameObject.activeSelf)
                {
                    continue;
                }

                item.Character.isSelected = item.IsOn;

                if (item.IsOn && first == null)
                {
                    first = item.Character;
                }
            }

            if (first == null)
                return false;

            CharacterManager.Instance.SaveAll();
            CharacterManager.Instance.SetActiveCharacter(first);
            return true;
        }

        private void ClearItems()
        {
            togglePrefab.gameObject.SetActive(false);

            for(var i=0;i<_items.Count;i++)
            {
                _items[i].gameObject.SetActive(false);
            }
        }

        private void BuildItems()
        {
            var items = CharacterManager.Instance.GetCharacters();

            for(var i=0;i<items.Count;i++)
            {
                var prefab = GetPrefab(i);

                prefab.Init(items[i]);
            }
        }

        private ToggleCharacter GetPrefab(int index)
        {
            if (_items.Count > index)
            {
                return _items[index];
            }

            var prefab = Instantiate(togglePrefab, content);
            _items.Add(prefab);
            return prefab;
        }
    }
}