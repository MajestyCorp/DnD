using DnD.Model;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DnD.UI
{
    public class ImportPopup : Frame
    {
        [SerializeField]
        private ToggleCharacter togglePrefab;
        [SerializeField]
        private RectTransform content;
        [SerializeField]
        private GameObject tooltipText;
        [SerializeField]
        private GameObject errorText;

        protected List<ToggleCharacter> _items = new();
        private TextEditor _editor;

        public void ButtonSave()
        {
            if(!SaveItems())
            {
                SoundManager.Instance.PlayNegative();
                return;
            }

            SoundManager.Instance.PlayClick();
            Hide();
        }

        public void ButtonImport()
        {
            SoundManager.Instance.PlayClick();

            ClearItems();
            tooltipText.SetActive(false);

            //var buffer = UniClipboard.GetText();

            _editor = _editor ?? new TextEditor();
            _editor.Paste();
            var buffer = _editor.text;

            try
            {
                var list = JsonUtility.FromJson<ExportList>(buffer);
                BuildItems(list.items);
            }
            catch (Exception e)
            {
                errorText.SetActive(true);
                return;
            }
        }

        protected override void OnShowAction()
        {
            base.OnShowAction();

            ClearItems();
            tooltipText.SetActive(true);
            errorText.SetActive(false);
        }

        private void ClearItems()
        {
            togglePrefab.gameObject.SetActive(false);

            for (var i = 0; i < _items.Count; i++)
            {
                _items[i].gameObject.SetActive(false);
            }
        }

        private void BuildItems(List<CharacterData> items)
        {
            for (var i = 0; i < items.Count; i++)
            {
                var prefab = GetPrefab(i);

                prefab.Init(items[i]);
                prefab.SetToggle(true);
            }
        }

        private bool SaveItems()
        {
            var saved = false;
            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item.gameObject.activeSelf && item.IsOn)
                {
                    item.Character.Save();
                    saved = true;
                }
            }

            if (saved)
            {
                CharacterManager.Instance.Refresh();
                return true;
            }

            return false;
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