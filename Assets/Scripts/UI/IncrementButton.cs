using UnityEngine;
using TMPro;

namespace DnD.UI
{
    public class IncrementButton : MonoBehaviour
    {
        [SerializeField]
        private bool decrement = false;
        [SerializeField]
        private bool addSign = false;
        [SerializeField]
        private TMP_InputField inputField;

        public void ButtonClick()
        {
            SoundManager.Instance.PlayClick();
            var value = inputField.text;
            
            if (int.TryParse(value, out var number))
            {
                number += decrement ? -1 : 1;
            } else
            {
                number = 0;
            }

            value = addSign && number >= 0 ? "+" + number.ToString() : number.ToString();

            inputField.text = value;
        }

    }
}