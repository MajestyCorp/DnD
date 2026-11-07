using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DnD.Model;
using DnD.Profiles;
using System;

namespace DnD.UI
{
    public class EditCharacterPopup : Frame
    {
        protected static EditCharacterPopup Instance { get; private set; }

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
        [SerializeField] private TMP_Dropdown subclassDropdown;
        [SerializeField] private GameObject subclassPanel;
        private readonly CharacterSubclassDropdown subclassChoice = new();

        protected override void Awake()
        {
            base.Awake();
            if (classDropdown != null) classDropdown.onValueChanged.AddListener(OnClassChanged);
        }

        protected override void OnDestroy()
        {
            if (classDropdown != null) classDropdown.onValueChanged.RemoveListener(OnClassChanged);
            base.OnDestroy();
        }

        private void OnClassChanged(int index)
        {
            subclassChoice.Initialize(subclassDropdown, subclassPanel, classChoice.Selection?.id);
        }

        private readonly CharacterOptionDropdown raceChoice = new();
        private readonly CharacterOptionDropdown classChoice = new();
        private CharacterData character;
        private Action callback;

        public static void Popup(Action callback)
        {
            if (Instance == null)
                Instance = UnityEngine.Object.FindFirstObjectByType<EditCharacterPopup>(FindObjectsInactive.Include);
            Instance.InitPopup(CharacterManager.Instance.ActiveCharacter, callback);
        }

        private void InitPopup(CharacterData data, Action callback)
        {
            if (data == null)
                return;
            character = data;
            this.callback = callback;
            toggleMale.isOn = data.isMale;
            toggleFemale.isOn = !data.isMale;
            inputName.text = data.name;
            raceChoice.Initialize(raceDropdown, optionsProfile != null ? optionsProfile.Races : null, data.raceId);
            classChoice.Initialize(classDropdown, optionsProfile != null ? optionsProfile.Classes : null, data.characterClassId);
            subclassChoice.Initialize(subclassDropdown, subclassPanel, classChoice.Selection?.id, data.characterSubclassId);
            Show();
        }

        public void ButtonSave()
        {
            var race = raceChoice.Selection;
            var characterClass = classChoice.Selection;
            if (character == null || string.IsNullOrWhiteSpace(inputName.text) || race == null || characterClass == null)
            {
                SoundManager.Instance.PlayNegative();
                return;
            }
            SoundManager.Instance.PlayClick();
            character.name = inputName.text;
            character.raceId = race.id;
            character.race = race.displayName;
            character.characterSubclassId = subclassDropdown != null ? subclassChoice.SelectedId
                : character.characterClassId == characterClass.id ? character.characterSubclassId : null;
            character.characterClassId = characterClass.id;
            character.characterClass = characterClass.displayName;
            character.isMale = toggleMale.isOn;
            if (!character.Save())
                return;

            callback?.Invoke();
            Hide();
        }
    }
}
