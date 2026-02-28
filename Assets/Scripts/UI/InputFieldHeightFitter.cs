using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DnD.UI
{
    public class InputFieldHeightFitter : MonoBehaviour
    {
        private TMP_InputField mainInputField;
        private TMP_Text heightCalculatorText;

        void Awake()
        {
            heightCalculatorText = GetComponent<TMP_Text>();
            mainInputField = GetComponentInChildren<TMP_InputField>();

            if (mainInputField != null)
            {
                mainInputField.onValueChanged.AddListener(UpdateHeightCalculatorText);
                heightCalculatorText.text = mainInputField.text;
            }
        }

        private void UpdateHeightCalculatorText(string newText)
        {
            heightCalculatorText.text = newText;
        }
    }
}