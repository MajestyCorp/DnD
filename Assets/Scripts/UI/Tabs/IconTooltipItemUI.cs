using DnD.Model;
using DnD.Profiles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DnD.UI.Tabs
{
    public class IconTooltipItemUI : TabItemUI
    {
        [SerializeField]
        private ItemIconsProfile profile;
        [SerializeField, Header("UI")]
        private Image iconImage;
        [SerializeField]
        private TextMeshProUGUI titleText;
        [SerializeField]
        private TextMeshProUGUI descriptionText;
        [SerializeField]
        private Button tooltipButton;

        private IconTooltipContent data;
        private RectTransform rectButton;

        protected void Awake()
        {
            rectButton = tooltipButton.GetComponent<RectTransform>();
        }

        public override EContentType GetContentType()
        {
            return EContentType.IconTooltip;
        }

        public override void Initialize(CharacterTabContent tab, DnD.Model.TabContent item)
        {
            base.Initialize(tab, item);
            data = item as IconTooltipContent;
            Invalidate();
            gameObject.SetActive(true);
        }

        protected override void Invalidate()
        {
            iconImage.sprite = null;
            iconImage.sprite = profile.GetIcon(data.icon_group, data.icon_id);
            name = data.title;
            titleText.text = data.title;
            descriptionText.text = data.description;

            descriptionText.gameObject.SetActive(!string.IsNullOrEmpty(data.description));
            tooltipButton.gameObject.SetActive(!string.IsNullOrEmpty(data.tooltip));
        }

        public void ButtonTooltip()
        {
            SoundManager.Instance.PlayClick();
            TooltipManager.Instance.ShowFor(rectButton, data.tooltip);
        }
    }
}