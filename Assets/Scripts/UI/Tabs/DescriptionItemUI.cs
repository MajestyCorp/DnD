using DnD.Model;
using TMPro;
using UnityEngine;

namespace DnD.UI.Tabs
{
    public class DescriptionItemUI : TabItemUI
    {
        [SerializeField]
        private TextMeshProUGUI titleText;
        [SerializeField]
        private TextMeshProUGUI descriptionText;

        private DescriptionContent data;

        public override EContentType GetContentType()
        {
            return EContentType.Description;
        }

        public override void Initialize(CharacterTabContent tab, DnD.Model.TabContent item)
        {
            base.Initialize(tab, item);
            data = item as DescriptionContent;
            Invalidate();
            gameObject.SetActive(true);
        }

        protected override void Invalidate()
        {
            name = data.title;
            titleText.text = data.title;
            descriptionText.text = data.description;
        }
    }
}