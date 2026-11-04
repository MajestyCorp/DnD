using DnD.Profiles;
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
        private CharacterOptionsProfile optionsProfile;
        [SerializeField]
        private TMP_Dropdown raceDropdown;
        [SerializeField]
        private TMP_Dropdown classDropdown;

        private readonly CharacterOptionDropdown raceChoice = new();
        private readonly CharacterOptionDropdown classChoice = new();

        private void OnEnable()
        {
            ResetForm();
        }

        private void ResetForm()
        {
            toggleMale.isOn = true;
            inputName.text = "";
            raceChoice.Initialize(raceDropdown, optionsProfile != null ? optionsProfile.Races : null);
            classChoice.Initialize(classDropdown, optionsProfile != null ? optionsProfile.Classes : null);
            avatar.Invalidate(null);
            avatar.SetEditMode(true);
        }

        public void ButtonCreate()
        {
            if (!IsValidated())
            {
                SoundManager.Instance.PlayNegative();
                return;
            }

            SoundManager.Instance.PlayClick();
            var race = raceChoice.Selection;
            var characterClass = classChoice.Selection;
            var hero = CharacterManager.Instance.CreateCharacter(inputName.text, race.displayName,
                characterClass.displayName, toggleMale.isOn, avatar.CurrentIndex, race.id, characterClass.id);
            if (hero == null)
                return;
            CharacterManager.Instance.SetActiveCharacter(hero);

            ButtonHide();
            ForceShowEvent();
        }

        private bool IsValidated()
        {
            if (string.IsNullOrWhiteSpace(inputName.text))
            {
                EventSystem.current?.SetSelectedGameObject(inputName.gameObject);
                return false;
            }
            if (raceChoice.Selection == null)
            {
                if (raceDropdown != null)
                    EventSystem.current?.SetSelectedGameObject(raceDropdown.gameObject);
                return false;
            }
            if (classChoice.Selection == null)
            {
                if (classDropdown != null)
                    EventSystem.current?.SetSelectedGameObject(classDropdown.gameObject);
                return false;
            }
            return true;
        }

        public void ButtonLeft() { SoundManager.Instance.PlayClick(); }
        public void ButtonRight() { SoundManager.Instance.PlayClick(); }
    }
}
