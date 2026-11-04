using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

namespace DnD.UI.Tabs
{
    public class ManageTabPopup : Frame, IInitializer
    {
        public enum EMode { Create = 0, Edit = 1 };

        public static ManageTabPopup Instance { get; private set; }


        [SerializeField, Header("UI")]
        private TMP_Text titleText;
        [SerializeField, Header("Fields")]
        private TMP_InputField nameField;

        private Action<String> callback;
        private EMode mode;
        private string value;

        public void InitializeSelf()
        {
            Instance = this;
        }

        public void InitializeAfter()
        {
        }

        public static void PopupCreate(Action<String> onCreated)
        {
            Instance.mode = EMode.Create;
            Instance.value = "";
            Instance.callback = onCreated;
            Instance.ButtonShow();
        }

        public static void PopupEdit(string tabName, Action<String> onEdited)
        {
            Instance.mode = EMode.Edit;
            Instance.value = tabName;
            Instance.callback = onEdited;
            Instance.ButtonShow();
        }

        public void ButtonCreate()
        {
            if (!Validated())
            {
                SoundManager.Instance.PlayNegative();
                return;
            }

            callback?.Invoke(nameField.text);
            ButtonHide();
        }

        public void ButtonChange()
        {
            if (!Validated())
            {
                SoundManager.Instance.PlayNegative();
                return;
            }

            callback?.Invoke(nameField.text);
            ButtonHide();
        }

        private bool Validated()
        {
            if (string.IsNullOrEmpty(nameField.text))
                return false;

            return true;
        }

        public void ButtonClose()
        {
            ButtonHide();
        }

        protected override void OnShowAction()
        {
            base.OnShowAction();

            UpdateUI();

            switch(mode)
            {
                case EMode.Create:
                    ResetFields();
                    break;
            };
        }

        private void UpdateUI()
        {
            titleText.text = mode == EMode.Create ? "Создать\nвкладку" : "Изменить\nвкладку";
        }

        private void ResetFields()
        {
            nameField.text = "";
        }
    }
}