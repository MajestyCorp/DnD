using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace DnD.UI
{
    public class ToggleRoll : MonoBehaviour
    {
        [SerializeField]
        private Image leftSpace;
        [SerializeField]
        private Color colorOn;
        [SerializeField]
        private Color colorOff;
        [SerializeField]
        private Toggle currentToggle;
        [SerializeField]
        private List<ToggleRoll> leftToggles;
        [SerializeField]
        private List<ToggleRoll> rightToggles;

        public bool IsOn => currentToggle.isOn;

        private LifeOrDeathRolls _rolls;

        private void Awake()
        {
            _rolls = GetComponentInParent<LifeOrDeathRolls>(true);
        }

        public void DoToggle(bool enabled)
        {
            SoundManager.Instance.PlayClick();

            if (leftSpace != null)
            {
                leftSpace.color = enabled ? colorOn : colorOff;
            }

            for (var i=0;i<leftToggles.Count;i++)
            {
                var toggle = leftToggles[i];

                if (!toggle.IsOn && enabled)
                {
                    toggle.ForceToggleValue(true);
                }
            }

            for (var i = 0; i < rightToggles.Count; i++)
            {
                var toggle = rightToggles[i];

                if (toggle.IsOn && !enabled)
                {
                    toggle.ForceToggleValue(false);
                }
            }

            if (_rolls != null)
                _rolls.StateChanged();
        }

        public void ForceToggleValue(bool value)
        {
            if (leftSpace != null)
            {
                leftSpace.color = value ? colorOn : colorOff;
            }

            currentToggle.SetIsOnWithoutNotify(value);
        }
    }
}