using DnD.Model;
using DnD.UI.Tabs;
using System.Collections.Generic;
using UnityEngine;

namespace DnD
{
    public class TabItemLibrary : MonoBehaviour, IInitializer
    {
        public static TabItemLibrary Instance { get; private set; }
        public delegate void ItemHandler(TabContent item);
        public event ItemHandler OnItemUpdated;
        public event ItemHandler OnItemDeleted;

        [SerializeField]
        private RectTransform rootCanvas;
        [SerializeField]
        private List<TabItemUI> items;

        public RectTransform RootCanvas => rootCanvas;

        public void InitializeSelf()
        {
            Instance = this;
        }

        public void InitializeAfter()
        {
        }

        public TabItemUI GetItemPrefab(EContentType contentType)
        {
            for(var i=0;i<items.Count;i++)
            {
                var item = items[i];
                if (item.GetContentType() == contentType)
                {
                    return item;
                }
            }

            return null;
        }

        public void Updated(TabContent item)
        {
            OnItemUpdated?.Invoke(item);
        }

        public void Deleted(TabContent item)
        {
            OnItemDeleted?.Invoke(item);
        }
    }
}
