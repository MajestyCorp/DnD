using DnD.Model.Inventory;
using DnD.Profiles;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

namespace DnD.UI.Inventory
{
    public class ItemTooltip : MonoBehaviour
    {
        [SerializeField]
        private ItemIconsProfile profile;
        [SerializeField]
        private Canvas canvas;
        [SerializeField]
        private RectTransform rect;
        [SerializeField]
        private RectTransform arrowRect;
        [SerializeField]
        private RectTransform arrowRect2;

        [SerializeField, Header("Fields")]
        private TMP_Text titleText;
        [SerializeField]
        private TMP_Text descriptionText;
        [SerializeField]
        private Image rarityImage;
        [SerializeField]
        private Image iconImage;
        [SerializeField]
        private TMP_Text typeText;
        [SerializeField]
        private TMP_Text rarityText;
        [SerializeField]
        private TMP_Text countText;
        [SerializeField]
        private TMP_Text weightText;
        [SerializeField]
        private Transform costContainer;
        [SerializeField]
        private TMP_Text goldText;
        [SerializeField]
        private TMP_Text silverText;
        [SerializeField]
        private TMP_Text copperText;
        [SerializeField]
        private Transform effectsContainer;
        [SerializeField]
        private EffectItem effectPrefab;

        [SerializeField]
        private Button deleteButton;
        [SerializeField]
        private Button incButton;

        private RectTransform target;
        private Item item;
        private bool isTemplate;
        private List<EffectItem> effects = new();
        private RectTransform canvasRect;

        private Action<Item> onDeleteCallback;
        private Action<Item> onIncCallback;
        private Action<Item> onEditedCallback;

        public void Initialize(RectTransform target, Item item, bool isTemplate = false)
        {
            this.target = target;
            this.item = item;
            this.isTemplate = isTemplate;

            InvalidateData(item, isTemplate);

            gameObject.SetActive(true);

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            PositionTooltip(target);

            onDeleteCallback = null;
            onIncCallback = null;
            onEditedCallback = null;
        }

        public void SetDeleteCallback(Action<Item> callback)
        {
            onDeleteCallback = callback;
        }

        public void SetIncCallback(Action<Item> callback)
        {
            onIncCallback = callback;
        }

        public void SetEditedCallback(Action<Item> callback)
        {
            onEditedCallback = callback;
        }

        private void Awake()
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }

        public void ButtonDelete()
        {
            SoundManager.Instance.PlayClick();
            onDeleteCallback?.Invoke(item);
            Hide();
        }

        public void ButtonInc()
        {
            if (!item.IsConsumable)
            {
                SoundManager.Instance.PlayNegative();
                return;
            }

            SoundManager.Instance.PlayClick();
            item.count++;

            countText.text = item.count.ToString();

            onIncCallback?.Invoke(item);
        }

        public void ButtonEdit()
        {
            SoundManager.Instance.PlayClick();
            ManageItemPopup.PopupEdit(item, onEditedCallback);
            Hide();
        }

        private void PositionTooltip(RectTransform target)
        {
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                corners[2],
                canvas.worldCamera,
                out Vector2 topLocal
            );

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                corners[0],
                canvas.worldCamera,
                out Vector2 bottomLocal
            );

            float popupHeight = rect.rect.height;
            float popupWidth = rect.rect.width;

            float canvasTop = canvasRect.rect.yMax;
            float canvasBottom = canvasRect.rect.yMin;

            bool fitsAbove = topLocal.y + popupHeight < canvasTop;
            var cellHeight = target.rect.height * 0.3f;

            float y = fitsAbove
                ? topLocal.y + popupHeight * .5f + cellHeight
                : bottomLocal.y - popupHeight * .5f - cellHeight;
            float x = (topLocal.x + bottomLocal.x) * .5f;

            rect.localPosition = ClampToCanvas(new Vector2(x, y));

            arrowRect.gameObject.SetActive(fitsAbove);
            arrowRect2.gameObject.SetActive(!fitsAbove);
            if (fitsAbove)
            {
                var arrowPos = arrowRect.position;
                arrowPos.x = target.position.x;
                arrowRect.position = arrowPos;
            }
            else
            {
                var arrowPos = arrowRect2.position;
                arrowPos.x = target.position.x;
                arrowRect2.position = arrowPos;
            }
        }

        private Vector3 ClampToCanvas(Vector2 pos)
        {
            Vector2 size = rect.rect.size;
            Vector2 canvasSize = canvasRect.rect.size;

            float halfX = canvasSize.x / 2;
            float halfY = canvasSize.y / 2;

            pos.x = Mathf.Clamp(pos.x,
                -halfX + size.x / 2,
                 halfX - size.x / 2);

            pos.y = Mathf.Clamp(pos.y,
                -halfY + size.y / 2,
                 halfY - size.y / 2);

            return pos;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, canvas.worldCamera))
                {
                    Hide();
                }
            }

            PositionTooltip(target);
        }

        private void Hide()
        {
            gameObject.SetActive(false);
        }

        private void InvalidateData(Item item, bool isTemplate)
        {
            if (effectPrefab.gameObject.activeSelf)
                effectPrefab.gameObject.SetActive(false);

            titleText.text = item.name;
            var color = item.GetRarityColor();
            color.a = 1;
            titleText.color = color;

            iconImage.sprite = null;
            iconImage.sprite = profile.GetIcon(item.icon_group, item.icon_id);
            rarityImage.color = item.GetRarityColor();
            descriptionText.text = item.description;
            descriptionText.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(item.description));

            typeText.text = item.GetTypeText();
            rarityText.text = item.GetRarityText();
            countText.text = item.count.ToString();
            countText.transform.parent.gameObject.SetActive(item.IsConsumable && !isTemplate);
            weightText.text = item.weight.ToString();
            weightText.transform.parent.gameObject.SetActive(item.weight > 0);

            goldText.text = item.costGold.ToString();
            goldText.gameObject.SetActive(item.costGold > 0);
            silverText.text = item.costSilver.ToString();
            silverText.gameObject.SetActive(item.costSilver > 0);
            copperText.text = item.costCopper.ToString();
            copperText.gameObject.SetActive(item.costCopper > 0);
            costContainer.gameObject.SetActive(item.costGold > 0 || item.costSilver > 0 || item.costCopper > 0);

            ClearEffects();
            BuildEffects(item.effects);

            deleteButton.gameObject.SetActive(!isTemplate);
            incButton.gameObject.SetActive(!isTemplate && item.IsConsumable);
        }

        private void BuildEffects(List<string> effectTexts)
        {
            for(var i=0;i< effectTexts.Count;i++)
            {
                var value = effectTexts[i];

                var effect = Instantiate(effectPrefab, effectsContainer);
                effect.Initialize(value);
                effects.Add(effect);
            }
        }

        private void ClearEffects()
        {
            for(var i=0;i<effects.Count;i++)
            {
                Destroy(effects[i].gameObject);
            }

            effects.Clear();
        }

    }
}