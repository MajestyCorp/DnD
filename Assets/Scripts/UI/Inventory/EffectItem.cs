using UnityEngine;
using TMPro;

namespace DnD.UI.Inventory
{
    public class EffectItem : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text text;

        public void Initialize(string value)
        {
            text.text = value;
            name = value;

            gameObject.SetActive(true);
        }

        public void ButtonDelete()
        {
            SoundManager.Instance.PlayClick();
            Destroy(gameObject);
        }
    }
}