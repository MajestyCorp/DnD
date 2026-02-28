using UnityEngine;
using UnityEngine.UI;

namespace DnD.UI
{
    public class TabButton : Button
    {
        [SerializeField]
        private TabGroup tabGroup;

        private bool isSelected;

        protected override void Awake()
        {
            base.Awake();

            if (tabGroup == null)
                tabGroup = GetComponentInParent<TabGroup>(true);

            tabGroup.Register(this);
        }

        public override void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            SoundManager.Instance.PlayClick();
            base.OnPointerClick(eventData);
            tabGroup.SelectTab(this);
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;

            DoStateTransition(selected ? SelectionState.Selected : SelectionState.Normal, false);

            UpdateVisualState();
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (isSelected)
            {
                // если вкладка выбрана Ч игнорируем hover/pressed
                base.DoStateTransition(SelectionState.Selected, instant);
                return;
            }

            base.DoStateTransition(state, instant);
        }

        private void UpdateVisualState()
        {
            //targetGraphic.color = isSelected ? colors.selectedColor : colors.normalColor;
        }
    }
}