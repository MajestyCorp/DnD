using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace DnD.UI
{
    public class CreateCharacterPopup : Frame
    {
        [SerializeField]
        private Avatar avatar;
        [SerializeField]
        private Toggle toggleMale;
        [SerializeField]
        private Toggle toggleFemale;
        [SerializeField]
        private TMP_InputField inputName;
        [SerializeField]
        private TMP_InputField inputRace;
        [SerializeField]
        private TMP_InputField inputClass;

        private void OnEnable()
        {
            ResetForm();
        }

        private void ResetForm()
        {
            toggleMale.isOn = true;
            inputName.text = inputRace.text = inputClass.text = "";
            avatar.Invalidate(null);
            avatar.SetEditMode(true);
        }

        public void ButtonCreate()
        {
            SoundManager.Instance.PlayClick();

            if (!IsValidated())
            {
                return;
            }

            var hero = CharacterManager.Instance.CreateCharacter(inputName.text, inputRace.text, inputClass.text, toggleMale.isOn, avatar.CurrentIndex);
            CharacterManager.Instance.SetActiveCharacter(hero);

            this.ButtonHide();
            this.ForceShowEvent();
        }

        private bool IsValidated()
        {
            if (string.IsNullOrEmpty(inputName.text))
            {
                EventSystem.current.SetSelectedGameObject(inputName.gameObject);
                return false;
            }

            if (string.IsNullOrEmpty(inputRace.text))
            {
                EventSystem.current.SetSelectedGameObject(inputRace.gameObject);
                return false;
            }

            if (string.IsNullOrEmpty(inputClass.text))
            {
                EventSystem.current.SetSelectedGameObject(inputClass.gameObject);
                return false;
            }

            return true;
        }

        public void ButtonLeft()
        {
            SoundManager.Instance.PlayClick();
        }

        public void ButtonRight()
        {
            SoundManager.Instance.PlayClick();
        }
    }
}