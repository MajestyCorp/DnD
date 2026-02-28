using System.Collections.Generic;
using UnityEngine;

namespace DnD.UI
{
    public class TabGroup : MonoBehaviour
    {
        [SerializeField]
        private TabButton defaultSelectedTab;

        [SerializeField]
        private List<TabContent> contents;

        private readonly List<TabButton> tabs = new();
        private TabButton selectedTab;

        public void Register(TabButton tab)
        {
            if (!tabs.Contains(tab))
                tabs.Add(tab);
        }

        private void Start()
        {
            if (tabs.Count > 0)
                SelectTab(defaultSelectedTab ? defaultSelectedTab : tabs[0]);
        }

        public void SelectTab(TabButton tab)
        {
            if (selectedTab == tab)
                return;

            selectedTab = tab;

            foreach (var t in tabs)
            {
                t.SetSelected(t == tab);
            }

            foreach (var contentTab in contents)
            {
                contentTab.Content.SetActive(contentTab.Button == selectedTab);
            }
        }
    }

    [System.Serializable]
    public class TabContent
    {
        public TabButton Button;
        public GameObject Content;
    }
}