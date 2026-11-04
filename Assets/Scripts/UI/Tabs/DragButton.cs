using UnityEngine;
using UnityEngine.EventSystems;

namespace DnD.UI.Tabs
{
    public class DragButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public DraggableItem DraggableItem { get; private set; }

        protected virtual void Awake()
        {
            DraggableItem = GetComponentInParent<DraggableItem>(true);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            DraggableItem.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            DraggableItem.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DraggableItem.OnEndDrag(eventData);
        }
    }
}