using DnD.Model;
using System;
using UnityEngine;

namespace DnD.UI.Tabs
{
    public class SelectTabItemPopup : Frame, IInitializer
    {
        public static SelectTabItemPopup Instance { get; private set; }

        [SerializeField, Header("UI")]
        private Transform content;


        private Action<EContentType> callback;

        public static void PopupSelect(Action<EContentType> onSelectedItem)
        {
            Instance.callback = onSelectedItem;
            Instance.ButtonShow();
        }

        public void InitializeSelf()
        {
            Instance = this;
        }

        public void InitializeAfter()
        {
        }

        public void ButtonAdd(int contentTypeIndex)
        {
            EContentType item = (EContentType)contentTypeIndex;
            callback?.Invoke(item);
            ButtonHide();
        }

        protected override void Awake()
        {
            base.Awake();

            var items = content.GetComponentsInChildren<TabItemUI>(true);

            for(var i=0;i<items.Length; i++)
            {
                var item = items[i];
                item.SetEditable(false);
            }
        }
    }
}