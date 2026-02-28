using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DnD.Model;
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
        private TMP_InputField inputRace;
        [SerializeField]
        private TMP_InputField inputClass;

        private Action _callback;

        public static void Popup(Action callback)
        {
            if (Instance == null)
            {
                Instance = UnityEngine.Object.FindFirstObjectByType<EditCharacterPopup>(FindObjectsInactive.Include);
            }

            Instance.InitPopup(CharacterManager.Instance.ActiveCharacter, callback);
        }

        private void InitPopup(CharacterData data, Action callback)
        {
            _callback = callback;

            if (data.isMale)
            {
                toggleFemale.isOn = false;
                toggleMale.isOn = true;
            }
            else
            {
                toggleMale.isOn = false;
                toggleFemale.isOn = true;
            }

            inputName.text = data.name;
            inputRace.text = data.race;
            inputClass.text = data.characterClass;

            Show();
        }

        public void ButtonSave()
        {
            SoundManager.Instance.PlayClick();

            var data = CharacterManager.Instance.ActiveCharacter;

            data.name = inputName.text;
            data.race = inputRace.text;
            data.characterClass = inputClass.text;
            data.isMale = toggleMale.isOn;

            data.Save();

            _callback?.Invoke();

            Hide();
        }
    }
}