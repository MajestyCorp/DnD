using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DnD.UI.Tabs
{
    public class TitleButton : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI titleText;

        public void Init(string title)
        {
            name = titleText.text = title;
            gameObject.SetActive(true);
        }
    }
}