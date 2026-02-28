using DnD.Model.Inventory;
using DnD.UI.Inventory;
using System;
using UnityEngine;

namespace DnD
{
    public class TooltipManager : MonoBehaviour, IInitializer
    {
        public static TooltipManager Instance { get; private set; }

        [SerializeField]
        private RectTransform itemTransformTooltip;
        [SerializeField]
        private ItemTooltip itemTooltip;
        [SerializeField]
        private Canvas canvas;

        public void InitializeSelf()
        {
            Instance = this;
            itemTooltip.gameObject.SetActive(false);
        }

        public void InitializeAfter()
        {
        }

        public void ShowFor(RectTransform target, Item item, Action<Item> deleteCallback, Action<Item> incCallback, Action<Item> editedCallback)
        {
            itemTooltip.Initialize(target, item, false);
            itemTooltip.SetDeleteCallback(deleteCallback);
            itemTooltip.SetIncCallback(incCallback);
            itemTooltip.SetEditedCallback(editedCallback);
        }

        public void ShowFor(RectTransform target, Item item, Action<Item> editedCallback)
        {
            itemTooltip.Initialize(target, item, true);
            itemTooltip.SetEditedCallback(editedCallback);
        }

        public void ShowFor(RectTransform target, Item item, bool isTemplate = false)
        {
            itemTooltip.Initialize(target, item, isTemplate);
        }
    }
}