using DnD.Model;
using TMPro;
using UnityEngine;

namespace DnD.UI.Tabs
{
    public class TitleItemUI : TabItemUI
    {
        [SerializeField]
        private TextMeshProUGUI titleText;

        private TitleContent data;

        public override EContentType GetContentType()
        {
            return EContentType.Title;
        }

        public override void Initialize(CharacterTabContent tab, DnD.Model.TabContent item)
        {
            base.Initialize(tab, item);
            data = item as TitleContent;
            Invalidate();
            gameObject.SetActive(true);
        }

        protected override void Invalidate()
        {
            name = data.title;
            titleText.text = data.title;
        }
    }
}