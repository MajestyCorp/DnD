using DnD.Model;
using TMPro;
using UnityEngine;

namespace DnD.UI.Tabs
{
    public class ManageTabItem : DraggableItem
    {
        [SerializeField]
        private TMP_Text titleText;

        public TabData Data { get; private set; }

        public void Initialize(TabData data)
        {
            this.Data = data;

            Invalidate();

            gameObject.SetActive(true);
        }

        private void Invalidate()
        {
            name = Data.name;
            titleText.text = Data.name;
        }

        public void ButtonDelete()
        {
            SoundManager.Instance.PlayClick();
            Destroy(gameObject);
        }
    }
}