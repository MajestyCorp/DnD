using UnityEngine;

namespace DnD.UI
{
    public class MenuPopup : Frame
    {
        [SerializeField]
        private ButtonFadeAnimation button;

        public void ButtonExit()
        {
            SoundManager.Instance.PlayClick();
            if (!CharacterManager.Instance.SaveAll())
                return;
            Application.Quit();
        }

        protected override void OnShowAction()
        {
            base.OnShowAction();

            button.SetState(ButtonFadeAnimationState.State2);
        }

        protected override void OnHideAction()
        {
            base.OnHideAction();

            button.SetState(ButtonFadeAnimationState.State1);
        }
    }
}