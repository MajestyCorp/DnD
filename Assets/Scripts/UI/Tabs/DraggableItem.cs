using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DnD.UI.Tabs
{
    [RequireComponent(typeof(LayoutElement), typeof(CanvasGroup))]
    public abstract class DraggableItem : MonoBehaviour
    {
        public delegate void ItemHandler(DraggableItem item);
        public static event ItemHandler DragStarted;
        public static event ItemHandler DragEnded;

        [SerializeField]
        protected CanvasGroup canvasGroup;

        private Transform originalParent;
        private int originalIndex;
        private Transform placeholder;
        private CanvasGroup placeholderCanvasGroup;
        private Vector3 offset;

        private float draggingAlpha = 0.3f;

        public int OriginalIndex => originalIndex;

        protected void NotifyDragStarted() { DragStarted?.Invoke(this); }
        protected void NotifyDragEnded() { DragEnded?.Invoke(this); }

        public virtual bool CanPerformAction(EDropAction action)
        {
            return true;
        }

        public virtual void PerformAction(EDropAction action)
        {

        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            originalParent = transform.parent;
            originalIndex = transform.GetSiblingIndex();
            offset = transform.position - (Vector3)eventData.position;

            CreatePlaceholder();

            canvasGroup.blocksRaycasts = false;
            transform.SetParent(TabItemLibrary.Instance.RootCanvas);

            DragStarted?.Invoke(this);
        }

        private void CreatePlaceholder()
        {
            placeholder = Instantiate(transform, transform.parent);
            placeholder.name = "Placeholder";

            DestroyImmediate(placeholder.GetComponent<DraggableItem>());

            var rect = GetComponent<RectTransform>();
            var layout = placeholder.GetComponent<LayoutElement>();
            layout.preferredHeight = rect.rect.height;

            placeholderCanvasGroup = placeholder.GetComponent<CanvasGroup>();

            if (placeholderCanvasGroup == null)
                placeholderCanvasGroup = placeholder.gameObject.AddComponent<CanvasGroup>();

            placeholderCanvasGroup.alpha = draggingAlpha;
            placeholderCanvasGroup.blocksRaycasts = false;

            placeholder.transform.SetSiblingIndex(transform.GetSiblingIndex());
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            transform.position = (Vector3)eventData.position + offset;
            var obj = eventData.pointerEnter;

            if (obj != null && obj.TryGetComponent<DropArea>(out var dropArea))
            {
                if (placeholder.gameObject.activeSelf)
                    placeholder.gameObject.SetActive(false);
            } else 
            {
                if (!placeholder.gameObject.activeSelf)
                {
                    placeholder.gameObject.SetActive(true);
                }
                MovePlaceholder(eventData.position, placeholder.GetSiblingIndex());
            }
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            var index = originalIndex;

            if (placeholder.gameObject.activeSelf)
            {
                MovePlaceholder(eventData.position, placeholder.GetSiblingIndex());
                index = placeholder.GetSiblingIndex();
            } else
            {
                index = index + 1;//because placeholder is hided and it change index order
            }
            transform.SetParent(originalParent, false);
            transform.SetSiblingIndex(index);

            canvasGroup.blocksRaycasts = true;

            Destroy(placeholder.gameObject);
            placeholder = null;

            DragEnded?.Invoke(this);

            if (originalIndex != index)
            {
                PerformAction(EDropAction.Drag);
            }
        }

        private void MovePlaceholder(Vector3 position, int startIndex)
        {
            var parent = originalParent;
            int childCount = parent.childCount;
            int newIndex = startIndex;//placeholder.GetSiblingIndex();
            var placeholderRect = placeholder as RectTransform;

            Vector3[] cornersOwner = new Vector3[4];
            placeholderRect.GetWorldCorners(cornersOwner);
            var ownerHeight = cornersOwner[1].y - cornersOwner[0].y;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = parent.GetChild(i);
                var otherContent = child.GetComponent<DraggableItem>();

                if ((child != placeholder && otherContent == null) || !child.gameObject.activeSelf)
                    continue;

                RectTransform rect = child as RectTransform;

                Vector3[] corners = new Vector3[4];
                rect.GetWorldCorners(corners);

                var top = corners[1].y;
                var bottom = corners[0].y;
                var height = top - bottom;
                var threshold = bottom;
                var heightThreshold = top - ownerHeight;
                threshold = Mathf.Max(threshold, heightThreshold);

                if (position.y <= top && position.y > threshold)
                {
                    newIndex = i;
                }

            }

            if (placeholder.transform.GetSiblingIndex() != newIndex)
                placeholder.transform.SetSiblingIndex(newIndex);
        }
    }
}