using DnD.Model;
using DnD.Profiles;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DnD.UI.Tabs
{
    public class ManageTabItemPopup : Frame, IInitializer
    {
        public static ManageTabItemPopup Instance { get; private set; }

        [SerializeField]
        private ItemIconsProfile profile;
        [SerializeField, Header("UI")]
        private Image iconImage;
        [SerializeField]
        private GameObject defaultIconImage;
        [SerializeField]
        private TMP_Text titleText;
        [SerializeField]
        private ScrollRect rootScrollRect;
        [SerializeField, Header("UI Panels")]
        private GameObject iconPanel;
        [SerializeField]
        private GameObject titlePanel;
        [SerializeField]
        private GameObject namePanel;
        [SerializeField]
        private GameObject descriptionPanel;
        [SerializeField]
        private GameObject tooltipPanel;
        [SerializeField]
        private GameObject cantripPanel;
        [SerializeField]
        private GameObject levelPanel;
        [SerializeField]
        private GameObject cellsAmountPanel;

        [SerializeField]
        private TMP_InputField titleInput;
        [SerializeField]
        private TMP_InputField nameInput;
        [SerializeField]
        private TMP_InputField descriptionInput;
        [SerializeField]
        private TMP_InputField tooltipInput;
        [SerializeField]
        private Toggle cantripToggle;
        [SerializeField]
        private TMP_InputField levelInput;
        [SerializeField]
        private CanvasGroup levelCanvasGroup;
        [SerializeField]
        private TMP_InputField cellsAmountInput;


        private int selectedIconGroup, selectedIconId;
        private bool createMode;
        private DnD.Model.TabContent item;
        private EContentType contentType;
        private Action<DnD.Model.TabContent> callback;

        public static void PopupCreate(EContentType contentType, Action<DnD.Model.TabContent> onCreatedItem)
        {
            Instance.selectedIconGroup = Instance.selectedIconId = -1;
            Instance.createMode = true;
            Instance.item = null;
            Instance.contentType = contentType;
            Instance.callback = onCreatedItem;
            Instance.ButtonShow();
        }

        public static void PopupUpdate(DnD.Model.TabContent item, Action<DnD.Model.TabContent> onUpdatedItem)
        {
            Instance.createMode = false;
            Instance.item = item;
            Instance.contentType = item.contentType;
            Instance.callback = onUpdatedItem;
            Instance.ButtonShow();
        }

        public void InitializeSelf()
        {
            Instance = this;
        }

        public void InitializeAfter()
        {
        }

        public void ButtonDec(int mode)
        {
            TMP_InputField input = null;

            switch (mode)
            {
                case 0:
                    input = levelInput;
                    break;
                case 1:
                    input = cellsAmountInput;
                    break;
            }

            var value = GetIntValue(input, 1);
            value = Mathf.Max(1, value - 1);
            input.text = value.ToString();
        }

        public void ButtonInc(int mode)
        {
            TMP_InputField input = null;

            switch (mode)
            {
                case 0:
                    input = levelInput;
                    break;
                case 1:
                    input = cellsAmountInput;
                    break;
            }

            var value = GetIntValue(input, 0);
            value = value + 1;
            input.text = value.ToString();
        }

        private int GetIntValue(TMP_InputField input, int defaultValue)
        {
            return int.TryParse(input.text, out var value)
                ? value
                : defaultValue;
        }

        public void ButtonSave()
        {
            if (createMode)
                item = DnD.Model.TabContent.Factory(contentType);
                
            UpdateItem(item);

            callback?.Invoke(item);

            ButtonHide();
        }

        public void ButtonSelectIcon()
        {
            SelectIconPopup.Popup(profile, OnIconSelected, OnIconCancelled);
        }

        private void OnIconSelected(int group, int id)
        {
            var icon = profile.GetIcon(group, id);

            selectedIconGroup = group;
            selectedIconId = id;

            iconImage.sprite = null;
            iconImage.sprite = icon;

            iconImage.gameObject.SetActive(true);
            defaultIconImage.SetActive(false);
        }

        private void OnIconCancelled()
        {
        }

        private void UpdateItem(DnD.Model.TabContent item)
        {
            switch (contentType)
            {
                case EContentType.Title:
                    var title = item as TitleContent;
                    title.title = titleInput.text;
                    break;

                case EContentType.Description:
                    var descr = item as DescriptionContent;
                    descr.title = nameInput.text;
                    descr.description = descriptionInput.text;
                    break;

                case EContentType.IconTooltip:
                    var iconTooltip = item as IconTooltipContent;
                    iconTooltip.icon_group = selectedIconGroup;
                    iconTooltip.icon_id = selectedIconId;
                    iconTooltip.title = nameInput.text;
                    iconTooltip.description = descriptionInput.text;
                    iconTooltip.tooltip = tooltipInput.text;

                    break;

                case EContentType.SpellCells:
                    var spellCells = item as SpellCellsContent;
                    spellCells.level = cantripToggle.isOn
                        ? 0
                        : (int.TryParse(levelInput.text, out var levelValue) ? levelValue : 1);
                    spellCells.maxSlots = int.TryParse(cellsAmountInput.text, out var cellsValue) ? cellsValue : 1;
                    spellCells.usedSlots = Mathf.Min(spellCells.usedSlots, spellCells.maxSlots);

                    break;
            }
        }

        protected override void OnShowAction()
        {
            base.OnShowAction();

            UpdateUI();

            if (createMode)
                ResetFields();
            else
                Invalidate(item);

            rootScrollRect.verticalNormalizedPosition = 1;
        }

        private void SetMultiField(TMP_InputField input, string value)
        {
            input.text = value;
        }

        private void Invalidate(DnD.Model.TabContent item)
        {
            switch(contentType)
            {
                case EContentType.Title:
                    var titleItem = item as TitleContent;
                    titleInput.text = titleItem.title;
                    break;

                case EContentType.Description:
                    var descriptionItem = item as DescriptionContent;
                    nameInput.text = descriptionItem.title;
                    SetMultiField(descriptionInput, descriptionItem.description);
                    break;

                case EContentType.IconTooltip:
                    var iconTooltipItem = item as IconTooltipContent;
                    LoadIcon(iconTooltipItem.icon_group, iconTooltipItem.icon_id);

                    nameInput.text = iconTooltipItem.title;
                    SetMultiField(descriptionInput, iconTooltipItem.description);
                    SetMultiField(tooltipInput, iconTooltipItem.tooltip);

                    break;

                case EContentType.SpellCells:
                    var spellCellsItem = item as SpellCellsContent;

                    cantripToggle.isOn = spellCellsItem.level == 0;
                    levelInput.text = spellCellsItem.level > 0 ? spellCellsItem.level.ToString() : "1";
                    cellsAmountInput.text = spellCellsItem.maxSlots.ToString();

                    break;
            }
        }

        public void OnCantripToggleChange(bool value)
        {
            levelCanvasGroup.interactable = !value;
        }

        private void LoadIcon(int group, int id)
        {
            selectedIconGroup = group;
            selectedIconId = id;

            iconImage.sprite = null;
            iconImage.sprite = profile.GetIcon(group, id);
            iconImage.gameObject.SetActive(iconImage.sprite != null);
            defaultIconImage.SetActive(iconImage.sprite == null);
        }

        private void ResetFields()
        {
            iconImage.gameObject.SetActive(false);
            defaultIconImage.SetActive(true);

            titleInput.text = "";
            nameInput.text = "";
            SetMultiField(descriptionInput, "");
            SetMultiField(tooltipInput, "");

            cantripToggle.isOn = false;
            levelInput.text = "1";
            cellsAmountInput.text = "1";
        }

        private void UpdateUI()
        {
            titleText.text = createMode ? "Создать\nэлемент" : "Изменить\nэлемент";

            iconPanel.SetActive(
                contentType == EContentType.IconTooltip
               );
            titlePanel.SetActive(
                contentType == EContentType.Title 
               );
            namePanel.SetActive(
                contentType == EContentType.Description 
                || contentType == EContentType.IconTooltip
            );
            descriptionPanel.SetActive(
                contentType == EContentType.Description 
                || contentType == EContentType.IconTooltip
            );
            tooltipPanel.SetActive(
                contentType == EContentType.IconTooltip
            );
            cantripPanel.SetActive(
                contentType == EContentType.SpellCells
            );
            levelPanel.SetActive(
                contentType == EContentType.SpellCells
            );
            cellsAmountPanel.SetActive(
                contentType == EContentType.SpellCells
            );
        }
    }
}