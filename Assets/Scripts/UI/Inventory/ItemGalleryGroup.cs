using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DnD.Profiles;
using System;

namespace DnD.UI.Inventory
{
    public class ItemGalleryGroup : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text title;
        [SerializeField]
        private ItemToSelect prefab;
        [SerializeField]
        private Transform content;

        private int groupId;
        private Action<int, int> callback;

        public void Initialize(ItemGroup group, int groupId, Action<int, int> callback)
        {
            if (prefab.gameObject.activeSelf)
            {
                prefab.gameObject.SetActive(false);
            }

            name = title.text = group.Name;
            this.groupId = groupId;
            this.callback = callback;

            Build(group.icons);

            gameObject.SetActive(true);
        }

        private void Build(List<Sprite> items)
        {
            for (var i = 0; i < items.Count; i++)
            {
                var icon = items[i];
                var item = Instantiate(prefab, content);

                item.Initialize(icon, groupId, i, callback);
            }
        }
    }
}