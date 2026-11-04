using DnD.Model;
using DnD.Model.Inventory;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DnD.UI.Tabs
{
    public abstract class TabItemUI : DraggableItem
    {
        [SerializeField]
        protected LayoutElement layoutElement;
        [SerializeField]
        protected Button button;
        [SerializeField]
        protected List<Transform> hideOnEdit;
        [SerializeField]
        protected List<Transform> showOnEdit;

        private CharacterTabContent tab;
        public DnD.Model.TabContent Item { get; private set; }

        public abstract EContentType GetContentType();

        public virtual void Initialize(CharacterTabContent tab, DnD.Model.TabContent item)
        {
            this.tab = tab;
            this.Item = item;
        }

        public virtual void ButtonEdit()
        {
            tab.EditItem(this);
        }

        public override void PerformAction(EDropAction action)
        {
            switch (action)
            {
                case EDropAction.Delete:
                    tab.DeleteItem(this);
                    Destroy(gameObject);
                    break;

                case EDropAction.Clone:
                    tab.CloneItem(this);
                    break;

                case EDropAction.Drag:
                    tab.ItemIndexChanged(this, OriginalIndex - transform.GetSiblingIndex());
                    break;
            }
        }

        protected abstract void Invalidate();

        public virtual void SetEditable(bool value)
        {
            button.interactable = value;

            for(var i=0;i<hideOnEdit.Count;i++)
            {
                var item = hideOnEdit[i];
                item.gameObject.SetActive(!value);
            }

            for (var i = 0; i < showOnEdit.Count; i++)
            {
                var item = showOnEdit[i];
                item.gameObject.SetActive(value);
            }
        }
    }
}