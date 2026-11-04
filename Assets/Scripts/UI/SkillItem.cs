using UnityEngine;
using TMPro;
using DnD.Model;
using UnityEngine.UI;

namespace DnD.UI
{
    public class SkillItem : MonoBehaviour
    {
        [SerializeField]
        private GameObject border;
        [SerializeField]
        private TMP_Text valueText;
        [SerializeField]
        private TMP_Text labelText;
        [SerializeField]
        private Button button;

        private SkillAttribute attribute;
        private CharacterData character;

        public void ButtonClick()
        {
            SoundManager.Instance.PlayClick();
            UpdatePropPopup.Popup(this.name, attribute, character.masteryBonus, SaveData);
        }

        public void SetEditable(bool value)
        {
            button.interactable = value;
        }

        private void SaveData()
        {
            character.Recalculate();
            Invalidate(this.name, attribute, character);
            character.Save();
        }

        public void Invalidate(string name, SkillAttribute attribute, CharacterData character)
        {
            this.name = name;
            this.attribute = attribute;
            this.character = character;

            border.SetActive(attribute.master || SkillAttributes.IsRelatedAbilitySelected(character, name));
            valueText.text = attribute.ToString();
            labelText.text = name;
        }
    }
}