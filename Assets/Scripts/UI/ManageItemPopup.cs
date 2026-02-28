using DnD.Model.Inventory;
using DnD.Profiles;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DnD.UI.Inventory;
using System.Collections.Generic;

namespace DnD.UI
{
    public class ManageItemPopup : Frame, IInitializer
    {
        public static ManageItemPopup Instance { get; private set; }

        [SerializeField]
        private ItemIconsProfile profile;
        [SerializeField, Header("UI")]
        private TMP_Text titleText;
        [SerializeField]
        private Button createButton;
        [SerializeField]
        private Button changeButton;
        [SerializeField, Header("Fields")]
        private Image itemIcon;
        [SerializeField]
        private GameObject itemDefaultIcon;

        [SerializeField]
        private RectTransform rootContent;
        [SerializeField]
        private ScrollRect rootScrollRect;

        [SerializeField]
        private TMP_InputField nameField;
        [SerializeField]
        private TMP_InputField descriptionField;
        [SerializeField]
        private TMP_Dropdown typeDropdown;
        [SerializeField]
        private TMP_Dropdown rarityDropdown;
        [SerializeField]
        private GameObject countContainer;
        [SerializeField]
        private TMP_InputField countField;
        [SerializeField]
        private TMP_InputField weightField;
        [SerializeField]
        private TMP_InputField copperField;
        [SerializeField]
        private TMP_InputField silverField;
        [SerializeField]
        private TMP_InputField goldField;

        [SerializeField, Header("Effects")]
        private RectTransform effectsContainer;
        [SerializeField]
        private TMP_InputField effectField;
        [SerializeField]
        private EffectItem effectItemPrefab;

        private Action<Item> callback;
        private int groupId;
        private int iconId;
        private Item targetItem;
        private bool createMode;

        public static void PopupCreate(Action<Item> onCreatedItem)
        {
            Instance.createMode = true;
            Instance.targetItem = null;
            Instance.callback = onCreatedItem;
            Instance.ButtonShow();
        }

        public static void PopupEdit(Item item, Action<Item> onEditedItem)
        {
            Instance.createMode = false;
            Instance.targetItem = item;
            Instance.callback = onEditedItem;
            Instance.ButtonShow();
        }

        public void InitializeSelf()
        {
            Instance = this;
        }

        public void InitializeAfter()
        {
        }

        public void ButtonSelectIcon()
        {
            SelectIconPopup.Popup(profile, OnIconSelected, OnIconCancelled);
        }

        public void ButtonAddEffect()
        {
            var value = effectField.text;

            if (string.IsNullOrEmpty(value))
            {
                SoundManager.Instance.PlayNegative();
                return;
            }

            SoundManager.Instance.PlayClick();
            effectField.text = "";

            var effect = Instantiate(effectItemPrefab, effectsContainer);
            effect.Initialize(value);

            StartCoroutine(RebuildLayout());
        }

        public void OnTypeDropdownSelected(int value)
        {
            var t = (EItemType)value;

            countContainer.SetActive(t == EItemType.Consumable);
        }

        private System.Collections.IEnumerator RebuildLayout(float scroll = 0)
        {
            yield return null;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rootContent);
            rootScrollRect.verticalNormalizedPosition = 0;
        }

        public void ButtonCreate()
        {
            if (!Validated())
            {
                SoundManager.Instance.PlayNegative();
                return;
            }

            var item = CreateItem();

            callback?.Invoke(item);
            ButtonHide();
        }

        public void ButtonChange()
        {
            if (!Validated())
            {
                SoundManager.Instance.PlayNegative();
                return;
            }

            UpdateItem(targetItem);
            ItemsLibrary.Instance.UpdateModel(targetItem);
            callback?.Invoke(targetItem);
            ButtonHide();
        }

        private bool Validated()
        {
            if (!itemIcon.gameObject.activeSelf)
                return false;

            if (string.IsNullOrEmpty(nameField.text))
                return false;

            if (typeDropdown.value == (int)EItemType.Consumable && 
                (string.IsNullOrEmpty(countField.text) || int.Parse(countField.text) <= 0))
                return false;

            if (!string.IsNullOrEmpty(weightField.text) && float.Parse(weightField.text) < 0)
                return false;

            if (!string.IsNullOrEmpty(copperField.text) && int.Parse(copperField.text) < 0)
                return false;
            if (!string.IsNullOrEmpty(silverField.text) && int.Parse(silverField.text) < 0)
                return false;
            if (!string.IsNullOrEmpty(goldField.text) && int.Parse(goldField.text) < 0)
                return false;

            return true;
        }

        private Item CreateItem()
        {
            var item = Item.CreateDummy();

            item.name = nameField.text;
            item.description = descriptionField.text;
            item.icon_group = groupId;
            item.icon_id = iconId;
            item.type = (EItemType) typeDropdown.value;
            item.rarity = (EItemRarity)rarityDropdown.value;

            if (item.IsConsumable)
                item.count = int.Parse(countField.text);

            if (!string.IsNullOrEmpty(weightField.text))
                item.weight = float.Parse(weightField.text);

            if (!string.IsNullOrEmpty(copperField.text))
                item.costCopper = int.Parse(copperField.text);

            if (!string.IsNullOrEmpty(silverField.text))
                item.costSilver = int.Parse(silverField.text);

            if (!string.IsNullOrEmpty(goldField.text))
                item.costGold = int.Parse(goldField.text);

            FillEffects(item);

            return item;
        }

        private void UpdateItem(Item item)
        {
            item.name = nameField.text;
            item.description = descriptionField.text;
            item.icon_group = groupId;
            item.icon_id = iconId;
            item.type = (EItemType)typeDropdown.value;
            item.rarity = (EItemRarity)rarityDropdown.value;
            item.weight = item.count = item.costCopper = item.costSilver = item.costGold = 0;

            if (item.IsConsumable)
                item.count = int.Parse(countField.text);

            if (!string.IsNullOrEmpty(weightField.text))
                item.weight = float.Parse(weightField.text);

            if (!string.IsNullOrEmpty(copperField.text))
                item.costCopper = int.Parse(copperField.text);

            if (!string.IsNullOrEmpty(silverField.text))
                item.costSilver = int.Parse(silverField.text);

            if (!string.IsNullOrEmpty(goldField.text))
                item.costGold = int.Parse(goldField.text);

            FillEffects(item);
        }

        private void FillEffects(Item item)
        {
            var count = effectsContainer.childCount;

            for (var i = 0; i < count; i++)
            {
                var child = effectsContainer.GetChild(i);

                if (!child.gameObject.activeSelf)
                    continue;

                var effect = child.GetComponent<EffectItem>();
                item.effects.Add(effect.name);
            }

            var value = effectField.text;

            if (!string.IsNullOrEmpty(value))
                item.effects.Add(value);
        }

        public void ButtonClose()
        {
            callback?.Invoke(null);
            ButtonHide();
        }

        protected override void OnShowAction()
        {
            base.OnShowAction();

            UpdateUI();

            if (createMode)
                ResetFields();
            else
                Invalidate(targetItem);

            rootScrollRect.verticalNormalizedPosition = 1;
        }

        private void UpdateUI()
        {
            titleText.text = createMode ? "Создать\nпредмет" : "Изменить\nпредмет";
            createButton.gameObject.SetActive(createMode);
            changeButton.gameObject.SetActive(!createMode);
        }

        private void Invalidate(Item item)
        {
            groupId = item.icon_group;
            iconId = item.icon_id;

            itemIcon.gameObject.SetActive(true);
            itemDefaultIcon.SetActive(false);
            itemIcon.sprite = null;
            itemIcon.sprite = profile.GetIcon(item.icon_group, item.icon_id);

            countContainer.SetActive(item.IsConsumable);

            nameField.text = item.name;
            descriptionField.text = item.description;
            typeDropdown.value = (int) item.type;
            rarityDropdown.value = (int) item.rarity;
            countField.text = item.count.ToString();
            weightField.text = item.weight.ToString();
            copperField.text = item.costCopper > 0 ? item.costCopper.ToString() : "";
            silverField.text = item.costSilver > 0 ? item.costSilver.ToString() : "";
            goldField.text = item.costGold > 0 ? item.costGold.ToString() : "";
            effectField.text = "";

            ClearEffects();
            InvalidateEffects(item.effects);
        }

        private void InvalidateEffects(List<string> effects)
        {
            for(var i=0;i<effects.Count;i++)
            {
                var value = effects[i];

                var effect = Instantiate(effectItemPrefab, effectsContainer);
                effect.Initialize(value);
            }
        }

        private void ResetFields()
        {
            itemIcon.gameObject.SetActive(false);
            itemDefaultIcon.SetActive(true);

            countContainer.SetActive(false);

            nameField.text = "";
            descriptionField.text = "";
            typeDropdown.value = 0;
            rarityDropdown.value = 0;
            countField.text = "";
            weightField.text = "";
            copperField.text = "";
            silverField.text = "";
            goldField.text = "";
            effectField.text = "";

            ClearEffects();
        }

        private void ClearEffects()
        {
            var count = effectsContainer.childCount;

            for (var i = count-1;i>=0;i--)
            {
                var child = effectsContainer.GetChild(i);
                
                if (!child.gameObject.activeSelf)
                    continue;

                Destroy(child.gameObject);
            }
        }

        private void OnIconSelected(int group, int id)
        {
            var icon = profile.GetIcon(group, id);

            groupId = group;
            iconId = id;

            itemIcon.sprite = null;
            itemIcon.sprite = icon;

            itemIcon.gameObject.SetActive(true);
            itemDefaultIcon.SetActive(false);
        }

        private void OnIconCancelled()
        {
        }
    }
}