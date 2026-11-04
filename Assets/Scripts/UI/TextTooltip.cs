using DnD.Model.Inventory;
using DnD.Profiles;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;

namespace DnD
{
    public class TextTooltip : MonoBehaviour
    {
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

        private RectTransform target;
        private RectTransform canvasRect;

        protected bool IsMousePressed => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        protected bool IsTouchPressed => Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        public void Initialize(RectTransform target, string text)
        {
            this.target = target;

            InvalidateData(text);

            gameObject.SetActive(true);

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            PositionTooltip(target);
        }

        private void Awake()
        {
            canvasRect = canvas.GetComponent<RectTransform>();
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
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                Vector2 pos = Pointer.current.position.ReadValue();

                if (!RectTransformUtility.RectangleContainsScreenPoint(rect, pos, canvas.worldCamera))
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

        private void InvalidateData(string text)
        {
            titleText.text = text;
        }
    }
}