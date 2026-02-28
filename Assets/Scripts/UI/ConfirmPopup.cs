using System;
using TMPro;
using UnityEngine;

namespace DnD.UI
{
    public class ConfirmPopup : Frame
    {
        protected static ConfirmPopup Instance { get; private set; }

        [SerializeField]
        private TMP_Text text;

        private Action _callback;

        public static void Popup(string text, Action callback)
        {
            if (Instance == null)
            {
                Instance = UnityEngine.Object.FindFirstObjectByType<ConfirmPopup>(FindObjectsInactive.Include);
            }

            Instance.InitPopup(text, callback);
        }

        protected void InitPopup(string text, Action callback)
        {
            this.text.text = text;
            _callback = callback;
            Show();
        }

        public void ButtonOk()
        {
            SoundManager.Instance.PlayClick();
            _callback?.Invoke();
            Hide();
        }
    }
}