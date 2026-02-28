using DnD.Model;
using DnD.Profiles;
using UnityEngine;
using UnityEngine.UI;

namespace DnD.UI
{
    public class Avatar : MonoBehaviour
    {
        [SerializeField]
        private bool isCreator = false;
        [SerializeField]
        private AvatarsProfile profile;
        [SerializeField]
        private Image avatarImage;
        [SerializeField]
        private Button prevButton;
        [SerializeField]
        private Button nextButton;

        public int CurrentIndex => currentIndex;

        private int currentIndex = 0;
        private CharacterData hero;

        public void Invalidate(CharacterData data)
        {
            hero = data;
            var images = profile.Avatars;
            currentIndex = data != null ? data.avatarIndex : 0;

            if (currentIndex >= 0 && currentIndex < images.Count)
            {
                avatarImage.sprite = null;
                avatarImage.sprite = images[currentIndex];
            } 
        }

        public void SetEditMode(bool value)
        {
            prevButton.gameObject.SetActive(value);
            nextButton.gameObject.SetActive(value);
        }

        public void ButtonPrev()
        {
            SoundManager.Instance.PlayClick();
            var images = profile.Avatars;

            if (isCreator)
            {
                currentIndex = Mathf.Max(0, currentIndex - 1);
            } else
            {
                currentIndex = Mathf.Max(0, hero.avatarIndex - 1);
                hero.avatarIndex = currentIndex;
            }

            avatarImage.sprite = null;
            avatarImage.sprite = images[currentIndex];
        }

        public void ButtonNext()
        {
            SoundManager.Instance.PlayClick();
            var images = profile.Avatars;

            if (isCreator)
            {
                currentIndex = Mathf.Min(images.Count - 1, currentIndex + 1);
            }
            else
            {
                currentIndex = Mathf.Min(images.Count - 1, hero.avatarIndex + 1);
                hero.avatarIndex = currentIndex;
            }

            avatarImage.sprite = null;
            avatarImage.sprite = images[currentIndex];
        }
    }
}