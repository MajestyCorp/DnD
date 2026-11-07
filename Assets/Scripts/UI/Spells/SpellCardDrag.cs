using System;
using DnD.UI.Tabs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DnD.UI.Spells
{
    // Uses the same DragButton / DropArea contract as custom tab items.
    public class SpellCardDrag : DraggableItem
    {
        [SerializeField] private GameObject dragHandle;
        private bool editable, dragging;
        private Transform spellOriginalParent;
        private int spellOriginalIndex;
        private RectTransform rect, spellPlaceholder, canvasRect;
        private Vector2 oldAnchorMin, oldAnchorMax, oldPivot, oldSize;
        private Vector3 spellDragOffset;
        private ScrollRect scroll;
        private bool scrollWasEnabled;
        private Action<SpellCardDrag> onMoved;

        public void SetEditable(bool value, Action<SpellCardDrag> onMoved)
        {
            if (!value) CancelDrag();
            editable = value;
            this.onMoved = onMoved;
            if (dragHandle != null) dragHandle.SetActive(value);
        }

        public override bool CanPerformAction(EDropAction action)
        {
            return editable && GetComponent<SpellRowUI>()?.SpellId != null
                && (action == EDropAction.Drag || (action == EDropAction.Delete && dragging));
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (dragging || !CanPerformAction(EDropAction.Drag)) return;
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
            rect = (RectTransform)transform;
            canvasRect = (RectTransform)canvas.rootCanvas.transform;
            spellOriginalParent = transform.parent;
            spellOriginalIndex = transform.GetSiblingIndex();
            oldAnchorMin = rect.anchorMin;
            oldAnchorMax = rect.anchorMax;
            oldPivot = rect.pivot;
            oldSize = rect.sizeDelta;
            var size = rect.rect.size;
            spellPlaceholder = (RectTransform)new GameObject("Spell drag placeholder", typeof(RectTransform), typeof(LayoutElement)).transform;
            spellPlaceholder.SetParent(spellOriginalParent, false);
            spellPlaceholder.SetSiblingIndex(spellOriginalIndex);
            var layout = spellPlaceholder.GetComponent<LayoutElement>();
            layout.preferredHeight = size.y;
            layout.preferredWidth = size.x;
            layout.flexibleHeight = 0;
            scroll = GetComponentInParent<ScrollRect>();
            if (scroll != null) { scrollWasEnabled = scroll.enabled; scroll.StopMovement(); scroll.enabled = false; }
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out var point);
            spellDragOffset = rect.position - point;
            rect.SetParent(canvasRect, true);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.position = point + spellDragOffset;
            dragging = true;
            NotifyDragStarted();
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!dragging) return;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out var point))
                rect.position = point + spellDragOffset;
            var index = spellOriginalParent.childCount;
            var corners = new Vector3[4];
            for (var i = 0; i < spellOriginalParent.childCount; i++)
            {
                var child = spellOriginalParent.GetChild(i);
                var row = child.GetComponent<SpellRowUI>();
                if (child == spellPlaceholder || row == null || row.SpellId == null || !child.gameObject.activeSelf) continue;
                ((RectTransform)child).GetWorldCorners(corners);
                var midpoint = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, (corners[0] + corners[2]) * 0.5f);
                if (eventData.position.y > midpoint.y) { index = i; break; }
            }
            if (index > spellPlaceholder.GetSiblingIndex()) index--;
            if (spellPlaceholder.GetSiblingIndex() != index) spellPlaceholder.SetSiblingIndex(index);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (!dragging) return;
            CompleteDrag(true);
            onMoved?.Invoke(this);
        }

        public override void PerformAction(EDropAction action)
        {
            if (!CanPerformAction(action)) return;
            if (action == EDropAction.Delete)
            {
                CompleteDrag(false);
                GetComponent<SpellRowUI>().ButtonAction();
            }
        }

        public void CancelDrag() { if (dragging) CompleteDrag(false); }
        private void OnDisable() { CancelDrag(); }

        private void CompleteDrag(bool keepPosition)
        {
            var index = keepPosition ? spellPlaceholder.GetSiblingIndex() : spellOriginalIndex;
            // Detach before destruction: Destroy() itself waits until the end of the frame.
            spellPlaceholder.SetParent(null, false);
            Destroy(spellPlaceholder.gameObject);
            spellPlaceholder = null;
            rect.SetParent(spellOriginalParent, false);
            rect.anchorMin = oldAnchorMin;
            rect.anchorMax = oldAnchorMax;
            rect.pivot = oldPivot;
            rect.sizeDelta = oldSize;
            rect.SetSiblingIndex(index);
            canvasGroup.blocksRaycasts = true;
            if (scroll != null) scroll.enabled = scrollWasEnabled;
            dragging = false;
            NotifyDragEnded();
        }
    }
}
