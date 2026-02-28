using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DnD.Model;

namespace DnD.UI
{
    public enum EUpdatePropMode { Attribute = 0, CurrentStat = 1, MaxStat = 2, IntValue = 3, ThrowAttribute = 4, SkillAttribute = 5 }
    public class UpdatePropPopup : Frame
    {
        protected static UpdatePropPopup Instance { get; private set; }

        [SerializeField]
        private TMP_Text titleText;

        [SerializeField, Header("Current Value")]
        private TMP_Text currentLabelText;
        [SerializeField]
        private Transform currentValuePanel;
        [SerializeField]
        private TMP_InputField currentValueInput;

        [SerializeField, Header("Max Value")]
        private CanvasGroup maxValuePanel;
        [SerializeField]
        private TMP_InputField maxValueInput;

        [SerializeField, Header("Modificator Value")]
        private Transform modificatorValuePanel;
        [SerializeField]
        private Toggle modificatorToggle;
        [SerializeField]
        private TMP_InputField modificatorValueInput;
        [SerializeField]
        private Button decModificatorButton;
        [SerializeField]
        private Button incModificatorButton;

        [SerializeField, Header("Modifier Value")]
        private Transform modifierValuePanel;
        [SerializeField]
        private TMP_InputField modifierValueInput;
        [SerializeField]
        private Button decModifierButton;
        [SerializeField]
        private Button incModifierButton;

        [SerializeField, Header("Master Toggle")]
        private Transform masterTogglePanel;
        [SerializeField]
        private Toggle masterToggle;

        [SerializeField, Header("Read Only Value")]
        private Transform readOnlyValuePanel;
        [SerializeField]
        private TMP_InputField readOnlyValueInput;

        [SerializeField, Header("Add Value")]
        private Transform addValuePanel;
        [SerializeField]
        private TMP_InputField addValueInput;

        [SerializeField, Header("Reduce Value")]
        private Transform reduceValuePanel;
        [SerializeField]
        private TMP_InputField reduceValueInput;

        [SerializeField, Header("Buttons")]
        private Button addButton;
        [SerializeField]
        private Button changeButton;

        private bool addBehaviour = false;
        private string title;
        private AttributeValue attributeValue;
        private StatValue statValue;
        private int intValue;
        private ThrowAttribute throwAttribute;
        private SkillAttribute skillAttribute;
        private EUpdatePropMode propMode;

        private Action callback;
        private Action<int> intCallback;

        public static void Popup(string title, SkillAttribute attribute, Action callback)
        {
            var instance = GetInstance();

            instance.addBehaviour = false;
            instance.title = title;
            instance.propMode = EUpdatePropMode.SkillAttribute;
            instance.skillAttribute = attribute;
            instance.callback = callback;
            instance.Init();
        }

        public static void Popup(string title, ThrowAttribute attribute, int masteryBonus, Action callback)
        {
            var instance = GetInstance();

            instance.addBehaviour = false;
            instance.title = title;
            instance.propMode = EUpdatePropMode.ThrowAttribute;
            instance.throwAttribute = attribute;
            instance.intValue = masteryBonus;
            instance.callback = callback;
            instance.Init();
        }

        public static void Popup(string title, AttributeValue attribute, Action callback)
        {
            var instance = GetInstance();

            instance.addBehaviour = false;
            instance.title = title;
            instance.propMode = EUpdatePropMode.Attribute;
            instance.attributeValue = attribute;
            instance.callback = callback;
            instance.Init();
        }

        public static void Popup(string title, StatValue attribute, EUpdatePropMode mode, Action callback)
        {
            var instance = GetInstance();

            instance.addBehaviour = false;
            instance.title = title;
            instance.propMode = mode;
            instance.statValue = attribute;
            instance.callback = callback;
            instance.Init();
        }

        public static void Popup(string title, int attribute, bool addBehaviour, Action<int> callback)
        {
            var instance = GetInstance();

            instance.addBehaviour = addBehaviour;
            instance.title = title;
            instance.propMode = EUpdatePropMode.IntValue;
            instance.intValue = attribute;
            instance.intCallback = callback;
            instance.Init();
        }

        private static UpdatePropPopup GetInstance()
        {
            if (Instance == null)
            {
                Instance = UnityEngine.Object.FindFirstObjectByType<UpdatePropPopup>(FindObjectsInactive.Include);
            }

            return Instance;
        }

        private void Init()
        {
            titleText.text = (addBehaviour ? "Добавить " : "Изменить ") + title;

            InvalidateItems(propMode);
            InvalidateValues(propMode);

            this.Show();
        }

        private void InvalidateItems(EUpdatePropMode mode)
        {
            currentValuePanel.gameObject.SetActive(
                mode == EUpdatePropMode.Attribute ||
                mode == EUpdatePropMode.CurrentStat ||
                (mode == EUpdatePropMode.IntValue && !addBehaviour)
            );

            maxValuePanel.gameObject.SetActive(mode == EUpdatePropMode.MaxStat || mode == EUpdatePropMode.CurrentStat);
            maxValuePanel.interactable = mode == EUpdatePropMode.MaxStat;

            modificatorValuePanel.gameObject.SetActive(mode == EUpdatePropMode.Attribute);

            addButton.gameObject.SetActive(false);
            changeButton.gameObject.SetActive(
                mode == EUpdatePropMode.Attribute ||
                mode == EUpdatePropMode.CurrentStat ||
                mode == EUpdatePropMode.MaxStat ||
                (mode == EUpdatePropMode.IntValue && !addBehaviour) ||
                mode == EUpdatePropMode.ThrowAttribute ||
                mode == EUpdatePropMode.SkillAttribute
            );

            readOnlyValuePanel.gameObject.SetActive(
                (mode == EUpdatePropMode.IntValue && addBehaviour) ||
                mode == EUpdatePropMode.ThrowAttribute ||
                mode == EUpdatePropMode.SkillAttribute
            );
            modifierValuePanel.gameObject.SetActive(
                mode == EUpdatePropMode.ThrowAttribute ||
                mode == EUpdatePropMode.SkillAttribute
            );
            masterTogglePanel.gameObject.SetActive(mode == EUpdatePropMode.ThrowAttribute);
            addValuePanel.gameObject.SetActive(
                mode == EUpdatePropMode.CurrentStat ||
                (mode == EUpdatePropMode.IntValue && addBehaviour)
            );
            reduceValuePanel.gameObject.SetActive(
                mode == EUpdatePropMode.CurrentStat ||
                (mode == EUpdatePropMode.IntValue && addBehaviour)
            );
        }

        private void InvalidateValues(EUpdatePropMode mode)
        {
            switch (mode)
            {
                case EUpdatePropMode.Attribute:
                    SetCurrentValue(attributeValue.baseValue);
                    SetModificatorValue(attributeValue.modifier);
                    InvalidateModificatorPanel(attributeValue.auto);
                    modificatorToggle.isOn = attributeValue.auto;
                    break;
                case EUpdatePropMode.CurrentStat:
                case EUpdatePropMode.MaxStat:
                    SetCurrentValue(statValue.current);
                    SetMaxValue(statValue.max);
                    SetInputValue(addValueInput, 0);
                    SetInputValue(reduceValueInput, 0);
                    break;
                case EUpdatePropMode.IntValue:
                    SetCurrentValue(intValue);
                    SetInputValue(readOnlyValueInput, intValue);
                    SetInputValue(addValueInput, 0);
                    SetInputValue(reduceValueInput, 0);
                    break;
                case EUpdatePropMode.ThrowAttribute:
                    SetInputValue(readOnlyValueInput, throwAttribute.baseValue);
                    SetInputValue(modifierValueInput, throwAttribute.modifier);
                    SetToggle(masterToggle, throwAttribute.master);
                    break;
                case EUpdatePropMode.SkillAttribute:
                    SetInputValue(readOnlyValueInput, skillAttribute.baseValue);
                    SetInputValue(modifierValueInput, skillAttribute.modifier);
                    break;
            }
        }
        #region CurrentValue
        private int GetCurrentValue()
        {
            var value = currentValueInput.text;
            return int.TryParse(value, out var number) ? number : 0;
        }

        private void SetCurrentValue(int value, bool withoutNotify = false)
        {
            if (withoutNotify)
                currentValueInput.SetTextWithoutNotify(value.ToString());
            else
                currentValueInput.text = value.ToString();
        }

        private bool CanIncCurrentValue(int value)
        {
            return propMode switch
            {
                EUpdatePropMode.CurrentStat => value < statValue.max,
                _ => true,
            };
        }

        public void ButtonDecCurrentValue()
        {
            SoundManager.Instance.PlayClick();
            var value = GetCurrentValue();
            SetCurrentValue(--value);
        }

        public void ButtonIncCurrentValue()
        {
            SoundManager.Instance.PlayClick();
            var value = GetCurrentValue();
            if (CanIncCurrentValue(value))
                SetCurrentValue(++value);
        }
        #endregion
        #region MaxValue
        private int GetMaxValue()
        {
            var value = maxValueInput.text;
            return int.TryParse(value, out var number) ? number : 0;
        }

        private void SetMaxValue(int value)
        {
            maxValueInput.SetTextWithoutNotify(value.ToString());
        }

        public void ButtonDecMaxValue()
        {
            SoundManager.Instance.PlayClick();
            var value = GetMaxValue();
            if (value > 0)
                SetMaxValue(--value);
        }

        public void ButtonIncMaxValue()
        {
            SoundManager.Instance.PlayClick();
            var value = GetMaxValue();
            SetMaxValue(++value);
        }
        #endregion
        #region ModificatorValue
        private int GetModificatorValue()
        {
            var value = modificatorValueInput.text;
            return int.TryParse(value, out var number) ? number : 0;
        }

        private void SetModificatorValue(int value)
        {
            modificatorValueInput.text = value >= 0 ? "+" + value.ToString() : value.ToString();
        }

        public void ButtonAddValue()
        {
            SoundManager.Instance.PlayClick();
            var value = GetInputValue(addValueInput);

            if (value == 0)
                return;

            switch (propMode)
            {
                case EUpdatePropMode.CurrentStat:
                    statValue.current = Mathf.Min(statValue.current + value, statValue.max);
                    callback.Invoke();
                    break;
                case EUpdatePropMode.IntValue:
                    intCallback?.Invoke(intValue + value);
                    break;
            }

            Hide();
        }

        public void ButtonReduceValue()
        {
            SoundManager.Instance.PlayClick();
            var value = GetInputValue(reduceValueInput);

            if (value == 0)
                return;

            switch (propMode)
            {
                case EUpdatePropMode.CurrentStat:
                    statValue.current = Mathf.Max(statValue.current - value, 0);
                    callback.Invoke();
                    break;
                case EUpdatePropMode.IntValue:
                    intCallback?.Invoke(Mathf.Max(intValue - value, 0));
                    break;
            }

            Hide();
        }

        private void SetInputValue(TMP_InputField input, int value)
        {
            input.text = value.ToString();
        }

        private void SetToggle(Toggle toggle, bool value)
        {
            toggle.isOn = value;
        }

        private int GetInputValue(TMP_InputField input)
        {
            var value = input.text;
            return int.TryParse(value, out var number) ? number : 0;
        }

        public void ButtonDecModificatorValue()
        {
            SoundManager.Instance.PlayClick();
            var value = GetModificatorValue();
            SetModificatorValue(--value);
        }

        public void ButtonIncModificatorValue()
        {
            SoundManager.Instance.PlayClick();
            var value = GetModificatorValue();
            SetModificatorValue(++value);
        }

        public void ButtonDecModifierValue()
        {
            SoundManager.Instance.PlayClick();
            var value = GetInputValue(modifierValueInput);
            SetInputValue(modifierValueInput, --value);

            switch(propMode)
            {
                case EUpdatePropMode.ThrowAttribute:
                    SetInputValue(readOnlyValueInput, throwAttribute.GetOriginalValue() + value);
                    break;
                case EUpdatePropMode.SkillAttribute:
                    SetInputValue(readOnlyValueInput, skillAttribute.GetOriginalValue() + value);
                    break;
            }
        }

        public void ButtonIncModifierValue()
        {
            SoundManager.Instance.PlayClick();
            var value = GetInputValue(modifierValueInput);
            SetInputValue(modifierValueInput, ++value);

            switch (propMode)
            {
                case EUpdatePropMode.ThrowAttribute:
                    SetInputValue(readOnlyValueInput, throwAttribute.GetOriginalValue() + value);
                    break;
                case EUpdatePropMode.SkillAttribute:
                    SetInputValue(readOnlyValueInput, skillAttribute.GetOriginalValue() + value);
                    break;
            }
        }
        #endregion

        public void ButtonChange()
        {
            SoundManager.Instance.PlayClick();
            switch (propMode)
            {
                case EUpdatePropMode.Attribute:
                    attributeValue.baseValue = GetCurrentValue();
                    attributeValue.modifier = GetModificatorValue();
                    attributeValue.auto = modificatorToggle.isOn;
                    callback.Invoke();
                    break;
                case EUpdatePropMode.CurrentStat:
                    statValue.current = GetCurrentValue();
                    callback.Invoke();
                    break;
                case EUpdatePropMode.MaxStat:
                    statValue.SetMaxValue(GetMaxValue());
                    callback.Invoke();
                    break;
                case EUpdatePropMode.IntValue:
                    intCallback?.Invoke(GetCurrentValue());
                    break;
                case EUpdatePropMode.ThrowAttribute:
                    throwAttribute.baseValue = GetInputValue(readOnlyValueInput);
                    throwAttribute.modifier = GetInputValue(modifierValueInput);
                    throwAttribute.master = masterToggle.isOn;
                    callback.Invoke();
                    break;
                case EUpdatePropMode.SkillAttribute:
                    skillAttribute.baseValue = GetInputValue(readOnlyValueInput);
                    skillAttribute.modifier = GetInputValue(modifierValueInput);
                    callback.Invoke();
                    break;
            }
            Hide();
        }

        public void OnCurrentValueChange(string text)
        {
            var value = GetCurrentValue();
            switch (propMode)
            {
                case EUpdatePropMode.CurrentStat:
                    if (value > statValue.max)
                        SetCurrentValue(statValue.max, true);
                    break;
                case EUpdatePropMode.Attribute:
                    if (modificatorToggle.isOn)
                    {
                        var modifier = Mathf.FloorToInt((value - 10) / 2f);
                        SetModificatorValue(modifier);
                    }
                    break;
            }

        }

        public void OnModifierValueChange(string text)
        {
            var value = GetInputValue(modifierValueInput);
            switch (propMode)
            {
                case EUpdatePropMode.ThrowAttribute:
                    var points = throwAttribute.GetOriginalValue() + value;
                    if (throwAttribute.master)
                        points -= intValue;

                    points += masterToggle.isOn ? intValue : 0;
                    SetInputValue(readOnlyValueInput, points);
                    break;
                case EUpdatePropMode.SkillAttribute:
                    SetInputValue(readOnlyValueInput, value + skillAttribute.GetOriginalValue());
                    break;
            }

        }

        public void OnModificatorToggleChanged(bool value)
        {
            SoundManager.Instance.PlayClick();
            InvalidateModificatorPanel(value);
            OnCurrentValueChange("");
        }

        public void OnMasterToggleChanged(bool value)
        {
            SoundManager.Instance.PlayClick();
            var points = throwAttribute.GetOriginalValue() + GetInputValue(modifierValueInput);
            if (throwAttribute.master)
            {
                points -= intValue;
            }

            points += value ? intValue : 0;
            SetInputValue(readOnlyValueInput, points);
        }

        private void InvalidateModificatorPanel(bool autoCalc)
        {
            modificatorValueInput.interactable = decModificatorButton.interactable = incModificatorButton.interactable = !autoCalc;
        }
    }
}