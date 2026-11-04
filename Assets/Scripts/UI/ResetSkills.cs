using UnityEngine;

namespace DnD.UI
{
    public class ResetSkills : MonoBehaviour
    {
        public delegate void VoidHandler();
        public static event VoidHandler OnReset;

        public void ButtonReset()
        {
            if (CharacterManager.Instance.ActiveCharacter == null)
                return;
            SoundManager.Instance.PlayClick();
            OnReset?.Invoke();
            CharacterManager.Instance.ActiveCharacter.Save();
        }
    }
}