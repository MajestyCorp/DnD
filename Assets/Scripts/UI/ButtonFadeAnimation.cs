using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System;

namespace DnD.UI
{
    public enum ButtonFadeAnimationState
    { State1, State2 };

    public class ButtonFadeAnimation : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        [SerializeField]
        private float endAngle = -90f;

        [SerializeField]
        private CanvasGroup textIcon1;

        [SerializeField]
        private CanvasGroup textIcon2;

        [SerializeField]
        private float playTime = 0.25f;

        [SerializeField]
        private AnimationCurve rotationCurve;

        public ButtonFadeAnimationState CurrentState { get; private set; } = ButtonFadeAnimationState.State1;

        private Coroutine _coroutine;
        private float _progress;
        private UnscaledTimer _timer = new();

        public void ButtonClick()
        {
            SetState(CurrentState == ButtonFadeAnimationState.State1 ? ButtonFadeAnimationState.State2 : ButtonFadeAnimationState.State1);
        }

        public void InitState(ButtonFadeAnimationState newState)
        {
            if (_coroutine != null)
                StopCoroutine(_coroutine);

            CurrentState = newState;
            _progress = 1f;
            SetProgress(1f);
        }

        public void SetState(ButtonFadeAnimationState newState)
        {
            if (newState == CurrentState)
                return;

            if (_coroutine != null)
                StopCoroutine(_coroutine);

            CurrentState = newState;

            _coroutine = StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            _timer.Activate(playTime, 1f - _progress);
            _progress = _timer.Progress;

            while (_timer.IsActive)
            {
                _progress = _timer.Progress;
                SetProgress(_progress);
                yield return null;
            }

            SetProgress(1f);

            _coroutine = null;
        }

        private void Awake()
        {
            _progress = 1f;
            SetProgress(1f);
        }

        private void SetGraphicProgress(CanvasGroup graphic, float progress, float maxAngle)
        {
            graphic.alpha = 1f - progress;

            graphic.transform.rotation = Quaternion.Euler(0f, 0f, maxAngle * rotationCurve.Evaluate(progress));

            var enabled = progress < 1f;

            if (graphic.gameObject.activeSelf != enabled)
                graphic.gameObject.SetActive(enabled);
        }

        private void SetProgress(float progress)
        {
            if (CurrentState == ButtonFadeAnimationState.State2)
            {
                SetGraphicProgress(textIcon1, progress, endAngle);
                SetGraphicProgress(textIcon2, 1f - progress, -endAngle);
            }
            else
            {
                SetGraphicProgress(textIcon1, 1f - progress, endAngle);
                SetGraphicProgress(textIcon2, progress, -endAngle);
            }
        }
    }
}