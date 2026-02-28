using DnD.Model;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace DnD.UI
{
    public enum EEditMode { Show = 0, Edit = 1 }
    public class Character : MonoBehaviour
    {
        [SerializeField]
        private EEditMode editMode = EEditMode.Show;
        [SerializeField]
        private ButtonFadeAnimation buttonAnimation;

        [SerializeField, Header("Title")]
        private TMP_Text heroNameLabel;
        [SerializeField]
        private Button heroNameButton;
        [SerializeField]
        private GameObject placeholder;

        [SerializeField, Header("Avatar")]
        private Avatar avatar;

        [SerializeField, Header("Left Labels")]
        private TextMeshProUGUI labelStrength;
        [SerializeField]
        private TextMeshProUGUI labelDexterity;
        [SerializeField]
        private TextMeshProUGUI labelConstitution;
        [SerializeField]
        private TextMeshProUGUI labelIntelligence;
        [SerializeField]
        private TextMeshProUGUI labelWisdom;
        [SerializeField]
        private TextMeshProUGUI labelCharisma;

        [SerializeField, Header("Right Labels")]
        private TextMeshProUGUI labelHealth;
        [SerializeField]
        private TextMeshProUGUI labelInitiative;
        [SerializeField]
        private TextMeshProUGUI labelArmor;
        [SerializeField]
        private TextMeshProUGUI labelLevel;
        [SerializeField]
        private TextMeshProUGUI labelExperience;
        [SerializeField]
        private TextMeshProUGUI labelMasteryBonus;

        [SerializeField, Header("Buttons")]
        private List<Button> buttons;

        [SerializeField, Header("Throws")]
        private Throws throws;
        [SerializeField, Header("Skills")]
        private Skills skills;
        [SerializeField, Header("Inventory")]
        private DnD.UI.Inventory.Inventory inventory;

        private CharacterData _character = null;
        private int _currentPropIndex;


        private void Awake()
        {
            throws.character = this;

            CharacterManager.Instance.OnSetActiveCharacter += OnSetActiveCharacter;
            Invalidate(CharacterManager.Instance.ActiveCharacter);

            SetEditMode(false);
        }

        private void OnSetActiveCharacter(CharacterData data)
        {
            Invalidate(data);
        }

        public void Invalidate(CharacterData data)
        {
            _character = data;
            heroNameLabel.text = "";

            gameObject.SetActive(data != null);
            placeholder.SetActive(data == null);

            avatar.Invalidate(data);

            if (data == null)
                return;

            heroNameLabel.text = data.name;

            labelStrength.text = data.strength.ToString();
            labelDexterity.text = data.dexterity.ToString();
            labelConstitution.text = data.constitution.ToString();
            labelIntelligence.text = data.intelligence.ToString();
            labelWisdom.text = data.wisdom.ToString();
            labelCharisma.text = data.charisma.ToString();

            labelHealth.text = data.hitPoints.ToString();
            labelInitiative.text = data.initiative.ToString();
            labelArmor.text = data.armorClass.ToString();
            labelLevel.text = data.level.ToString();
            labelExperience.text = data.experience.ToString();
            labelMasteryBonus.text = data.masteryBonus.ToString();

            throws.Invalidate(data);
            skills.Invalidate(data);
            inventory.Invalidate(data);
        }

        private void SetActiveButtons(bool value)
        {
            for (var i=0;i<buttons.Count;i++)
            {
                buttons[i].interactable = value;
            }
        }

        public void ButtonEditName()
        {
            SoundManager.Instance.PlayClick();
            EditCharacterPopup.Popup(OnEditNameChange);
        }

        private void OnEditNameChange()
        {
            Invalidate(_character);
        }

        public void ButtonToggleEdit()
        {
            SoundManager.Instance.PlayClick();

            SetEditMode(editMode == EEditMode.Show);
        }

        private void SetEditMode(bool value)
        {
            editMode = value ? EEditMode.Edit : EEditMode.Show;
            buttonAnimation.SetState(value ? ButtonFadeAnimationState.State2 : ButtonFadeAnimationState.State1);

            heroNameButton.interactable = editMode == EEditMode.Edit;
            SetActiveButtons(value);

            avatar.SetEditMode(value);
            throws.SetEditable(value);
            skills.SetEditable(value);
            inventory.SetEditable(value);

            if (!value && _character != null)
                _character.Save();
        }

        public void ButtonEditAttribute(int index)
        {
            SoundManager.Instance.PlayClick();
            _currentPropIndex = index;

            switch (index)
            {
                case 0:
                    UpdatePropPopup.Popup("Силу", _character.strength, SaveCharacter);
                    break;
                case 1:
                    UpdatePropPopup.Popup("Ловкость", _character.dexterity, SaveCharacter);
                    break;
                case 2:
                    UpdatePropPopup.Popup("Телосложение", _character.constitution, SaveCharacter);
                    break;
                case 3:
                    UpdatePropPopup.Popup("Интеллект", _character.intelligence, SaveCharacter);
                    break;
                case 4:
                    UpdatePropPopup.Popup("Мудрость", _character.wisdom, SaveCharacter);
                    break;
                case 5:
                    UpdatePropPopup.Popup("Харизму", _character.charisma, SaveCharacter);
                    break;
            }
        }

        public void ButtonEditStat(int index)
        {
            SoundManager.Instance.PlayClick();
            _currentPropIndex = index;

            switch (index)
            {
                case 0:
                    if (editMode == EEditMode.Edit)
                        UpdatePropPopup.Popup("Хит Поинты", _character.hitPoints, EUpdatePropMode.MaxStat, SaveCharacter);
                    else
                        UpdatePropPopup.Popup("Хит Поинты", _character.hitPoints, EUpdatePropMode.CurrentStat, SaveCharacter);
                    break;
            }
        }

        public void ButtonEditValue(int index)
        {
            SoundManager.Instance.PlayClick();
            _currentPropIndex = index;

            switch (index)
            {
                case 0:
                    UpdatePropPopup.Popup("Инициативу", _character.initiative, false, SaveValue);
                    break;
                case 1:
                    UpdatePropPopup.Popup("Класс Брони", _character.armorClass, false, SaveValue);
                    break;
                case 2:
                    UpdatePropPopup.Popup("Уровень", _character.level, false, SaveValue);
                    break;
                case 3:
                    UpdatePropPopup.Popup("Опыт", _character.experience, editMode == EEditMode.Show, SaveValue);
                    break;
                case 4:
                    UpdatePropPopup.Popup("Бонус мастерства", _character.masteryBonus, false, SaveValue);
                    break;
            }
        }

        private void SaveValue(int value)
        {
            switch (_currentPropIndex)
            {
                case 0:
                    _character.initiative = value;
                    break;
                case 1:
                    _character.armorClass = value;
                    break;
                case 2:
                    _character.level = value;
                    break;
                case 3:
                    _character.experience = value;
                    break;
                case 4:
                    _character.masteryBonus = value;
                    break;
            }

            SaveCharacter();
        }

        private void SaveCharacter()
        {
            _character.Save();
            Invalidate(_character);
        }
    }
}