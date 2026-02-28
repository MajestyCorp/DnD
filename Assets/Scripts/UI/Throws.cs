using DnD.Model;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace DnD.UI
{
    public class Throws : MonoBehaviour, IEditable
    {
        [SerializeField]
        private List<Button> buttons;
        [SerializeField]
        private List<GameObject> borders;

        [SerializeField]
        private TMP_Text strengthValueLabel;
        [SerializeField]
        private TMP_Text dexterityValueLabel;
        [SerializeField]
        private TMP_Text constitutionValueLabel;
        [SerializeField]
        private TMP_Text intelligenceValueLabel;
        [SerializeField]
        private TMP_Text wisdomValueLabel;
        [SerializeField]
        private TMP_Text charismaValueLabel;

        private CharacterData _data;
        public Character character;

        public void Invalidate(CharacterData data)
        {
            _data = data;
            var throws = data.throws;

            throws.Invalidate(data);

            strengthValueLabel.text = throws.strength.ToString();
            dexterityValueLabel.text = throws.dexterity.ToString();
            constitutionValueLabel.text = throws.constitution.ToString();
            intelligenceValueLabel.text = throws.intelligence.ToString();
            wisdomValueLabel.text = throws.wisdom.ToString();
            charismaValueLabel.text = throws.charisma.ToString();

            borders[0].SetActive(throws.strength.master);
            borders[1].SetActive(throws.dexterity.master);
            borders[2].SetActive(throws.constitution.master);
            borders[3].SetActive(throws.intelligence.master);
            borders[4].SetActive(throws.wisdom.master);
            borders[5].SetActive(throws.charisma.master);
        }

        public void SetEditable(bool value)
        {
            for (var i = 0; i < buttons.Count; i++)
            {
                buttons[i].interactable = value;
            }
        }

        public void ButtonClick(int index)
        {
            SoundManager.Instance.PlayClick();
            switch(index)
            {
                case 0:
                    UpdatePropPopup.Popup("Силу", _data.throws.strength, _data.masteryBonus, SaveCharacter);
                    break;
                case 1:
                    UpdatePropPopup.Popup("Ловкость", _data.throws.dexterity, _data.masteryBonus, SaveCharacter);
                    break;
                case 2:
                    UpdatePropPopup.Popup("Телосложение", _data.throws.constitution, _data.masteryBonus, SaveCharacter);
                    break;
                case 3:
                    UpdatePropPopup.Popup("Интеллект", _data.throws.intelligence, _data.masteryBonus, SaveCharacter);
                    break;
                case 4:
                    UpdatePropPopup.Popup("Мудрость", _data.throws.wisdom, _data.masteryBonus, SaveCharacter);
                    break;
                case 5:
                    UpdatePropPopup.Popup("Харизму", _data.throws.charisma, _data.masteryBonus, SaveCharacter);
                    break;
            }
        }

        private void SaveCharacter()
        {
            _data.Save();
            character.Invalidate(_data);
        }
    }
}