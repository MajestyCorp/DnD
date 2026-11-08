namespace DnD.UI
{
    public sealed class SelectAvatarPopup : SelectIconPopup
    {
        protected override void Awake()
        {
            // Keep the character creation form open beneath the avatar picker.
            InvokeShowEvents = false;
            base.Awake();
        }

        public override void InitializeSelf()
        {
            // Do not replace the singleton used by the item icon picker.
            InitializeTemplate();
        }
    }
}
