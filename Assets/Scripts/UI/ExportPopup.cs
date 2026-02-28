using DnD.Model;
using System.Collections.Generic;
using UnityEngine;

namespace DnD.UI
{
    public class ExportPopup : Frame
    {
        [SerializeField]
        private ToggleCharacter togglePrefab;
        [SerializeField]
        private RectTransform content;

        protected List<ToggleCharacter> _items = new();

        public void ButtonExport()
        {
            if (!Export())
            {
                SoundManager.Instance.PlayNegative();
                return;
            }

            SoundManager.Instance.PlayClick();
            Hide();
        }

        private bool Export()
        {
            ExportList list = new();

            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item.gameObject.activeSelf && item.IsOn)
                {
                    list.items.Add(item.Character);
                }
            }

            if (list.items.Count > 0)
            {
                var json = JsonUtility.ToJson(list, false);
                UniClipboard.SetText(json);
                return true;
            }

            return false;
        }

        protected override void OnShowAction()
        {
            base.OnShowAction();

            ClearItems();
            BuildItems();
        }

        private void ClearItems()
        {
            togglePrefab.gameObject.SetActive(false);

            for (var i = 0; i < _items.Count; i++)
            {
                _items[i].gameObject.SetActive(false);
            }
        }

        private void BuildItems()
        {
            var items = CharacterManager.Instance.GetCharacters();

            for (var i = 0; i < items.Count; i++)
            {
                var prefab = GetPrefab(i);

                prefab.Init(items[i]);
                prefab.SetToggle(true);
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

    public class ExportList
    {
        public List<CharacterData> items = new();
    }
}