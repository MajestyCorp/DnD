using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace DnD.UI
{
    public class MobileTMPFix : MonoBehaviour, IPointerDownHandler
    {
        private TMP_InputField input;

        void Awake()
        {
            input = GetComponent<TMP_InputField>();
        }
#if UNITY_EDITOR
        public void OnPointerDown(PointerEventData eventData)
        { }    
#else
        public void OnPointerDown(PointerEventData eventData)
        {
            input.ActivateInputField();

            Vector2 localMousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                input.textViewport,
                eventData.position,
                eventData.pressEventCamera,
                out localMousePos
            );

            int index = TMP_TextUtilities.GetCursorIndexFromPosition(
                input.textComponent,
                localMousePos,
                eventData.pressEventCamera
            );

            input.caretPosition = index;
        }
#endif
    }
}