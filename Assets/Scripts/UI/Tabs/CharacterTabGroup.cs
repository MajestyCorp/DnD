using DnD.Model;
using DnD.Model.Inventory;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DnD.UI.Tabs
{
    public class CharacterTabGroup : TabGroup
    {
        public const int TAB_INDEX = 3;

        [SerializeField, Header("Tab Button")]
        private Transform content;
        [SerializeField]
        private TitleButton tabButtonPrefab;
        [SerializeField, Header("Tab Content")]
        private Transform contentContainer;
        [SerializeField]
        private CharacterTabContent tabContentPrefab;
        [SerializeField, Header("UI")]
        private Transform buttonAdd;
        [SerializeField]
        private Transform buttonManage;

        private bool editable;
        private CharacterData characterData = null;

        private List<TitleButton> tabButtons = new();
        private List<CharacterTabContent> tabContents = new();
        private CharacterTabContent activeContent = null;

        private void Awake()
        {
            CharacterManager.Instance.OnSetActiveCharacter += OnSetActiveCharacter;
            Invalidate(CharacterManager.Instance.ActiveCharacter);
        }

        private void OnDestroy()
        {
            if (CharacterManager.Instance != null)
                CharacterManager.Instance.OnSetActiveCharacter -= OnSetActiveCharacter;
        }

        private void OnSetActiveCharacter(CharacterData data)
        {
            Invalidate(data);
        }

        private void Invalidate(CharacterData data)
        {
            var selected = SelectedTab;
            var selectedDynamicIndex = activeContent != null ? tabContents.IndexOf(activeContent) : -1;
            var sameCharacter = characterData != null && data != null && characterData.id == data.id;
            characterData = data;

            ClearTabs();
            SetEditable(editable);

            if (data == null)
            {
                SelectTab(defaultSelectedTab);
                return;
            }

            for(var i=0;i<data.tabs.Count;i++)
            {
                var tab = data.tabs[i];
                var button = AddTabButton(tab.name);
                tabButtons.Add(button);

                var content = AddContent(tab);
                content.SetEditable(editable);
                tabContents.Add(content);
            }

            for (var i = 0; i < data.tabs.Count; i++)
            {
                var tab = data.tabs[i];
                var button = tabButtons[i];
                button.transform.SetSiblingIndex(tab.index);
            }

            SetEditable(editable);
            if (sameCharacter && selectedDynamicIndex >= 0 && selectedDynamicIndex < tabButtons.Count)
                SelectTab(tabButtons[selectedDynamicIndex].GetComponent<TabButton>());
            else if (selected != null && selectedDynamicIndex < 0)
                SelectTab(selected);
            else
                SelectTab(defaultSelectedTab);
        }

        private void ClearTabs()
        {
            for (var i = 0; i < tabButtons.Count; i++)
            {
                tabButtons[i].gameObject.SetActive(false);
                Destroy(tabButtons[i].gameObject);
            }
            tabButtons.Clear();

            for (var i=0; i < tabContents.Count; i++)
            {
                tabContents[i].gameObject.SetActive(false);
                Destroy(tabContents[i].gameObject);
            }
            tabContents.Clear();
            activeContent = null;
        }

        public void SetEditable(bool value)
        {
            editable = value;
            buttonAdd.gameObject.SetActive(value && characterData != null);
            buttonManage.gameObject.SetActive(value && tabContents.Count > 0);

            for(var i=0;i<tabContents.Count;i++)
            {
                var content = tabContents[i];

                content.SetEditable(value);
            }
        }

        public void ButtonCreate()
        {
            SoundManager.Instance.PlayClick();
            ManageTabPopup.PopupCreate(OnCreatedCallback);
        }

        private void OnCreatedCallback(string tabName)
        {
            CreateTab(tabName);
        }

        private void CreateTab(string tabName)
        {
            var data = new TabData();
            data.name = tabName;

            var button = AddTabButton(tabName);
            data.index = button.transform.GetSiblingIndex();
            tabButtons.Add(button);

            var content = AddContent(data);
            tabContents.Add(content);
            characterData.tabs.Add(data);

            //make new tab active
            SelectTab(button.GetComponent<TabButton>());
            SetEditable(editable);
            characterData.Save();
        }

        protected override void UpdateTab(TabButton tab)
        {
            base.UpdateTab(tab);

            activeContent = tab != null ? GetActiveContent(tab.gameObject) : null;
            UpdateContentStates(activeContent);
        }

        private void UpdateContentStates(CharacterTabContent selectedContent)
        {
            for(var i=0;i<tabContents.Count;i++)
            {
                var tabContent = tabContents[i];
                tabContent.gameObject.SetActive(tabContent == selectedContent);
            }
        }

        private CharacterTabContent GetActiveContent(GameObject buttonGameObject)
        {
            for(var i=0;i<tabButtons.Count;i++)
            {
                var tabButton = tabButtons[i];

                if (tabButton.gameObject == buttonGameObject)
                {
                    return tabContents[i];
                }
            }

            return null;
        }

        protected TitleButton AddTabButton(string title)
        {
            var button = Instantiate(tabButtonPrefab, content);
            button.Init(title);

            buttonAdd.transform.SetAsLastSibling();
            buttonManage.transform.SetAsLastSibling();

            return button;
        }

        protected CharacterTabContent AddContent(TabData tabData)
        {
            var content = Instantiate(tabContentPrefab, contentContainer);
            content.FillContent(tabData);

            content.gameObject.SetActive(true);
            content.gameObject.SetActive(false);

            return content;

        }

        public void ButtonManage()
        {
            ManageTabsPopup.Popup(OnTabsSaved);
        }

        private void OnTabsSaved(List<TabData> list)
        {
            var oldList = characterData.tabs;

            for (var i=0;i<list.Count;i++)
            {
                var item = list[i];
                var oldIndex = oldList.IndexOf(item);

                item.index = i + TAB_INDEX;

                var button = tabButtons[oldIndex];
                button.transform.SetSiblingIndex(i + TAB_INDEX);
            }

            var selectLastTab = false;
            var selectedIndex = -1;

            if (activeContent != null)
                selectedIndex = tabContents.IndexOf(activeContent);

            for(var i=oldList.Count - 1; i>= 0;i--)
            {
                var item = oldList[i];

                if (list.IndexOf(item) >= 0)
                    continue;

                oldList.RemoveAt(i);
                var button = tabButtons[i];
                var content = tabContents[i];

                Destroy(button.gameObject);
                Destroy(content.gameObject);
                tabButtons.RemoveAt(i);
                tabContents.RemoveAt(i);

                if (content == activeContent)
                    selectLastTab = true;
            }

            if (selectLastTab)
            {
                if (tabButtons.Count > 0)
                {
                    var tabIndex = Mathf.Min(selectedIndex, tabButtons.Count - 1);
                    SelectTab(tabButtons[tabIndex].GetComponent<TabButton>());
                } else
                {
                    SelectTab(defaultSelectedTab);
                }
            }

            SetEditable(editable);
            characterData.Save();
        }
    }
}