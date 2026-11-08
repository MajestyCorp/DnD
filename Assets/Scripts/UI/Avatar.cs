using DnD.Model;
using DnD.Profiles;
using UnityEngine;
using UnityEngine.UI;

namespace DnD.UI
{
    public class Avatar : MonoBehaviour
    {
        [SerializeField] private bool isCreator = false;
        [SerializeField] private ItemIconsProfile profile;
        [SerializeField] private Image avatarImage;
        [SerializeField] private Button selectButton;
        [SerializeField] private SelectAvatarPopup selectPopup;

        public int CurrentIndex => currentIndex;
        public int CurrentGroup => currentGroup;
        private int currentIndex;
        private int currentGroup;
        private bool editable;
        private CharacterData hero;
        private int selectionVersion;

        private void Awake()
        {
            if (selectButton != null) selectButton.onClick.AddListener(ButtonSelect);
        }

        private void OnDestroy()
        {
            if (selectButton != null) selectButton.onClick.RemoveListener(ButtonSelect);
        }

        private void OnDisable()
        {
            selectionVersion++;
        }

        public void Invalidate(CharacterData data)
        {
            selectionVersion++;
            hero = data;
            currentGroup = data?.avatarGroupIndex ?? 0;
            currentIndex = data?.avatarIndex ?? 0;
            RefreshImage();
        }

        public void SetEditMode(bool value)
        {
            editable = value;
            if (!value) selectionVersion++;
            if (selectButton != null) selectButton.gameObject.SetActive(value);
        }

        private void RefreshImage()
        {
            avatarImage.sprite = profile != null ? profile.GetIcon(currentGroup, currentIndex) : null;
        }

        public void ButtonSelect()
        {
            if (!editable || profile == null || selectPopup == null || (!isCreator && hero == null)) return;
            var version = ++selectionVersion;
            selectPopup.Open(profile, (group, index) =>
            {
                if (this == null || !isActiveAndEnabled || !editable || version != selectionVersion
                    || profile.GetIcon(group, index) == null) return;
                if (!isCreator)
                {
                    if (!ReferenceEquals(hero, CharacterManager.Instance.ActiveCharacter)) return;
                    var previousGroup = hero.avatarGroupIndex;
                    var previousIndex = hero.avatarIndex;
                    hero.avatarGroupIndex = group;
                    hero.avatarIndex = index;
                    if (!hero.Save())
                    {
                        hero.avatarGroupIndex = previousGroup;
                        hero.avatarIndex = previousIndex;
                        return;
                    }
                }
                currentGroup = group;
                currentIndex = index;
                RefreshImage();
            });
        }
    }
}
