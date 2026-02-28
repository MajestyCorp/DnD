using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DnD.Model;

namespace DnD.UI
{
    public class ToggleCharacter : MonoBehaviour
    {
        [SerializeField]
        private Toggle toggleSelect;
        [SerializeField]
        private TextMeshProUGUI labelName;
        [SerializeField]
        private TextMeshProUGUI labelClass;
        [SerializeField]
        private TextMeshProUGUI labelHitPoints;
        [SerializeField]
        private TextMeshProUGUI labelLevel;

        public bool IsOn => toggleSelect.isOn;
        public CharacterData Character { get; private set; }

        public void Init(CharacterData character)
        {
            Character = character;
            FillData(character);
        }

        public void SetToggle(bool value)
        {
            toggleSelect.isOn = value;
        }

        public void ButtonDelete()
        {
            SoundManager.Instance.PlayClick();
            ConfirmPopup.Popup("¬ы действительно хотите удалить персонажа <b>" + Character.name + "</b>?", ConfirmDelete);
        }

        private void ConfirmDelete()
        {
            CharacterManager.Instance.Delete(Character);
            gameObject.SetActive(false);
        }

        private void FillData(CharacterData character)
        {
            toggleSelect.SetIsOnWithoutNotify(character.isSelected);
            labelName.text = character.name;
            labelClass.text = character.characterClass;
            labelHitPoints.text = character.hitPoints.max.ToString();
            labelLevel.text = character.level.ToString();

            gameObject.SetActive(true);
        }
    }
}