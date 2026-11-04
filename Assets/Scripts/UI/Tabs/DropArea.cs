using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DnD.UI.Tabs
{
    [System.Serializable]
    public enum EDropAction { Delete = 0, Clone = 1, Drag = 2};

    [RequireComponent(typeof(Button))]
    public class DropArea : MonoBehaviour, IDropHandler
    {
        [SerializeField]
        private EDropAction action;

        private Button button;

        protected void Awake()
        {
            button = GetComponent<Button>();
            button.interactable = false;

            DraggableItem.DragStarted += OnDragStarted;
            DraggableItem.DragEnded += OnDragEnded;
        }

        private void OnDestroy()
        {
            DraggableItem.DragStarted -= OnDragStarted;
            DraggableItem.DragEnded -= OnDragEnded;
        }

        private void OnDragStarted(DraggableItem item)
        {
            if (!gameObject.activeSelf || item == null || !item.CanPerformAction(action))
                return;

            button.interactable = true;
        }

        private void OnDragEnded(DraggableItem item)
        {
            if (!gameObject.activeSelf)
                return;
            button.interactable = false;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var drag = eventData.pointerDrag;
            if (drag == null)
                return;

            var item = drag.GetComponent<DragButton>();
            if (item == null || !item.DraggableItem.CanPerformAction(action))
                return;

            item.DraggableItem.PerformAction(action);
        }
    }
}