using System;
using UnityEngine;
using UnityEngine.UI;

namespace DnD.UI.Inventory
{
    public class ItemToSelect : MonoBehaviour
    {
        [SerializeField]
        private Image image;

        private int group;
        private int id;
        private Action<int, int> callback;

        public void Initialize(Sprite icon, int group, int id, Action<int, int> callback)
        {
            this.name = icon.name;
            this.group = group;
            this.id = id;
            this.callback = callback;
            image.sprite = null;
            image.sprite = icon;

            gameObject.SetActive(true);
        }

        public void ButtonSelect()
        {
            SoundManager.Instance.PlayClick();
            callback?.Invoke(group, id);
        }
    }
}