using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DnD.UI
{
    public enum EFrameAnimation {None, Popup, FromLeft, FromRight, FromTop, FromBottom}

    public abstract class Frame : MonoBehaviour
    {
        public delegate void FrameHandler(Frame frame);
        public static event FrameHandler OnShow;

        public const float AnimationTime = 0.25f;
        private const float AnimationDistance = 100f;

        [SerializeField]
        private EFrameAnimation frameAnimation = EFrameAnimation.None;
        [SerializeField]
        private bool invokeShowEvents = true;

        public bool IsVisible { get; private set; }

        private AnimationCurve _curve;
        private Coroutine _animationCoroutine;
        private CanvasGroup _canvasGroup;
        private RectTransform _rect;
        private Vector2 _position;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rect = GetRect();
            _position = _rect.anchoredPosition;
            _curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            OnShow += OnFrameShow;

            if (!IsVisible)
                gameObject.SetActive(false);
        }

        private void OnFrameShow(Frame frame)
        {
            if (frame != this && IsVisible && invokeShowEvents)
                Hide();
        }

        protected virtual RectTransform GetRect()
        {
            return GetComponent<RectTransform>();
        }

        public void ButtonToggle()
        {
            SoundManager.Instance.PlayClick();
            Toggle();
        }

        public void ButtonShow()
        {
            SoundManager.Instance.PlayClick();
            Show();
        }

        public void ButtonHide()
        {
            SoundManager.Instance.PlayClick();
            Hide();
        }

        public void Toggle()
        {
            if (IsVisible)
                Hide();
            else
                Show();
        }

        public void Show()
        {
            if (IsVisible)
                return;
            IsVisible = true;
            transform.SetAsLastSibling();
            if(invokeShowEvents)
                OnShow?.Invoke(this);

            OnShowAction();

            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            gameObject.SetActive(true);
            _animationCoroutine = StartCoroutine(ShowInternal());
        }

        public void ForceShowEvent()
        {
            OnShow?.Invoke(this);
        }

        public void Hide()
        {
            if (!IsVisible)
                return;

            OnHideAction();

            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _animationCoroutine = StartCoroutine(HideInternal());
            IsVisible = false;
        }

        protected virtual void OnShowAction()
        {

        }

        protected virtual void OnHideAction()
        {

        }

        private IEnumerator HideInternal()
        {
            var speed = 1f / AnimationTime;

            var vector = GetAnimationVector();
            var scale = GetAnimationScale();
            var startPos = _position - vector;

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = false;

            while (_canvasGroup.alpha > 0f)
            {
                _canvasGroup.alpha = Mathf.Max(0f, _canvasGroup.alpha - speed * Time.deltaTime);
                var value = _curve.Evaluate(_canvasGroup.alpha);
                _rect.anchoredPosition = Vector3.Lerp(startPos, _position, value);
                _rect.localScale = Vector3.Lerp(scale, Vector3.one, value);
                yield return null;
            }

            gameObject.SetActive(false);
        }

        private IEnumerator ShowInternal()
        {
            var speed = 1f / AnimationTime;

            var vector = GetAnimationVector();
            var scale = GetAnimationScale();
            var startPos = _position - vector;

            _canvasGroup.alpha = 0f;

            while(_canvasGroup.alpha < 1f)
            {
                _canvasGroup.alpha = Mathf.Min(1f, _canvasGroup.alpha + speed * Time.deltaTime);
                var value = _curve.Evaluate(_canvasGroup.alpha);
                _rect.anchoredPosition = Vector3.Lerp(startPos, _position, value);
                _rect.localScale = Vector3.Lerp(scale, Vector3.one, value);
                yield return null;
            }

            _canvasGroup.interactable = true;
        }

        private Vector3 GetAnimationScale()
        {
            switch(frameAnimation)
            {
                case EFrameAnimation.Popup:
                    return Vector3.one * 0.9f;
                default:
                    return Vector3.one;
            }
        }

        private Vector2 GetAnimationVector()
        {
            switch(frameAnimation)
            {
                case EFrameAnimation.FromLeft:
                    return Vector2.right * AnimationDistance;
                case EFrameAnimation.FromRight:
                    return Vector2.left * AnimationDistance;
                case EFrameAnimation.FromTop:
                    return Vector2.down * AnimationDistance;
                case EFrameAnimation.FromBottom:
                    return Vector2.up * AnimationDistance;
                default:
                    return Vector2.zero;
            }
        }
    }
}