using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using DnD.Model;

namespace DnD.UI
{
    public class ToggleCharacter : MonoBehaviour
    {
        [SerializeField]
        private Toggle toggleSelect;
        [SerializeField]
        private TextMeshProUGUI labelName;
        [SerializeField]
        private TextMeshProUGUI labelClass;
        [SerializeField]
        private TextMeshProUGUI labelHitPoints;
        [SerializeField]
        private TextMeshProUGUI labelLevel;
        [SerializeField]
        private Transform areaHealth;
        [SerializeField]
        private Transform areaLevel;
        [SerializeField]
        private Transform buttonDelete;
        [SerializeField] private GameObject damagedIcon;
        [SerializeField] private GameObject rawExportButton;

        public bool IsOn => toggleSelect.isOn;
        public CharacterData Character { get; private set; }
        public DamagedCharacterFile DamagedFile { get; private set; }

        public void Init(CharacterData character)
        {
            DamagedFile = null;
            if (damagedIcon != null) damagedIcon.SetActive(false);
            if (rawExportButton != null) rawExportButton.SetActive(false);
            Character = character;
            FillData(character);
        }

        public void InitDamaged(DamagedCharacterFile file)
        {
            Character = null;
            DamagedFile = file;
            labelName.text = file.FileName;
            labelClass.text = "Повреждён — доступен экспорт исходного файла";
            labelHitPoints.text = "—";
            labelLevel.text = "—";
            if (damagedIcon != null) damagedIcon.SetActive(true);
            if (rawExportButton != null) rawExportButton.SetActive(true);
            toggleSelect.SetIsOnWithoutNotify(false);
            gameObject.SetActive(true);
            buttonDelete.gameObject.SetActive(true);
            areaHealth.gameObject.SetActive(false);
            areaLevel.gameObject.SetActive(false);
        }

        public void ButtonExportRaw()
        {
            if (DamagedFile != null && DamagedFile.TryReadText(out var text))
            {
                GUIUtility.systemCopyBuffer = text;
                JsonStorage.Notify("Исходный текст файла скопирован в буфер обмена.");
            }
        }

        public void SetToggle(bool value)
        {
            toggleSelect.isOn = value;
        }

        public void ButtonDelete()
        {
            SoundManager.Instance.PlayClick();
            if (DamagedFile != null)
            {
                ConfirmPopup.Popup("Удалить повреждённый файл персонажа и его резервную копию? Перед удалением можно экспортировать исходный файл.", ConfirmDelete);
                return;
            }
            ConfirmPopup.Popup("Вы действительно хотите удалить персонажа <b>" + Character.name + "</b>?", ConfirmDelete);
        }

        private void ConfirmDelete()
        {
            if (DamagedFile != null)
            {
                if (CharacterData.DeleteDamagedFile(DamagedFile)) gameObject.SetActive(false);
                return;
            }
            CharacterManager.Instance.Delete(Character);
            if (!CharacterManager.Instance.GetCharacters().Contains(Character))
                gameObject.SetActive(false);
        }

        private void FillData(CharacterData character)
        {
            buttonDelete.gameObject.SetActive(false);
            areaHealth.gameObject.SetActive(true);
            areaLevel.gameObject.SetActive(true);

            toggleSelect.SetIsOnWithoutNotify(character.isSelected);
            labelName.text = character.name;
            labelClass.text = character.characterClass;
            labelHitPoints.text = character.hitPoints.max.ToString();
            labelLevel.text = character.level.ToString();

            gameObject.SetActive(true);
        }
    }
}
