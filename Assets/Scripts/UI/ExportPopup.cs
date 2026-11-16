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
            DamagedCharacterFile damaged = null;
            var damagedCount = 0;

            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item.gameObject.activeSelf && item.IsOn)
                {
                    if (item.DamagedFile != null)
                    {
                        damaged = item.DamagedFile;
                        damagedCount++;
                    }
                    else if (item.Character != null)
                        list.items.Add(item.Character);
                }
            }

            if (damagedCount > 0)
            {
                if (damagedCount != 1 || list.items.Count != 0)
                {
                    JsonStorage.Notify("Для экспорта исходного файла выберите только одного повреждённого персонажа. Снимите остальные отметки.");
                    return false;
                }
                if (!damaged.TryReadText(out var raw)) return false;
                GUIUtility.systemCopyBuffer = raw;
                return true;
            }

            if (list.items.Count > 0)
            {
                var json = JsonStorage.Serialize(list);

                GUIUtility.systemCopyBuffer = json;

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
            var damaged = CharacterData.DamagedFiles;
            for (var i = 0; i < damaged.Count; i++)
                GetPrefab(items.Count + i).InitDamaged(damaged[i]);
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

    [System.Serializable]
    public class ExportList
    {
        public List<CharacterData> items = new();
    }
}
