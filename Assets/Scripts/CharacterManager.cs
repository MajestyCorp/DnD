using DnD.Model;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DnD
{
    public class CharacterManager : MonoBehaviour, IInitializer
    {
        public delegate void CharacterHandler(CharacterData data);
        public event CharacterHandler OnSetActiveCharacter;

        public static CharacterManager Instance { get; private set; }
        public CharacterData ActiveCharacter { get; private set; } = null;
        public int AmountOfActiveCharacters { get; private set; } = 0;

        [SerializeField]
        private Button leftButton;
        [SerializeField]
        private Button rightButton;

        private List<CharacterData> _models = null;

        public void InitializeSelf()
        {
            Instance = this;

            var activeId = PlayerPrefs.GetString("ActiveCharacterId", "");

            _models = CharacterData.LoadAll();
            AmountOfActiveCharacters = GetAmountOfActiveCharacters();
            ActiveCharacter = GetCharacterById(activeId);
        }

        public void InitializeAfter()
        {
            for (var i=0;i<_models.Count; i++)
            {
                var model = _models[i];

                ItemsLibrary.Instance.Sync(model);
            }
        }

        private void Start()
        {
            InvalidateButtons();
            OnSetActiveCharacter?.Invoke(ActiveCharacter);
        }

        public CharacterData CreateCharacter(string name, string race, string charClass, bool isMale, int avatarIndex)
        {
            var newChar = new CharacterData
            {
                name = name,
                race = race,
                characterClass = charClass,
                level = 1,
                experience = 0,
                armorClass = 10,
                initiative = 0,
                speed = 30,
                isMale = isMale,
                avatarIndex = avatarIndex,
                isSelected = true,
            };

            _models.Add(newChar);
            newChar.Save();
            return newChar;
        }

        public void Refresh()
        {
            var currentId = "";

            if (ActiveCharacter != null)
            {
                currentId = ActiveCharacter.id;
            }

            _models = CharacterData.LoadAll();
            AmountOfActiveCharacters = GetAmountOfActiveCharacters();
            ActiveCharacter = GetCharacterById(currentId);

            InvalidateButtons();
            OnSetActiveCharacter?.Invoke(ActiveCharacter);
        }

        public void Delete(CharacterData data)
        {
            _models.Remove(data);
            data.Delete();
        }

        public IReadOnlyList<CharacterData> GetCharacters()
        {
            return _models;
        }

        public CharacterData GetCharacterById(string id)
        {
            for (var i=0;i<_models.Count;i++)
            {
                if (_models[i].id == id)
                {
                    return _models[i];
                }
            }

            return null;
        }

        public void SetActiveCharacter(CharacterData character)
        {
            if (ActiveCharacter != null && ActiveCharacter.id.CompareTo(character.id) != 0)
            {
                ActiveCharacter.Save();
            }

            ActiveCharacter = character;
            AmountOfActiveCharacters = GetAmountOfActiveCharacters();
            PlayerPrefs.SetString("ActiveCharacterId", character.id);

            InvalidateButtons();
            OnSetActiveCharacter?.Invoke(character);
        }

        public void SaveAll()
        {
            foreach (var model in _models)
            {
                model.Save();
            }
        }

        public void ButtonPrev()
        {
            SoundManager.Instance.PlayClick();
            SetActiveCharacter(GetPrevCharacter());
        }

        public void ButtonNext()
        {
            SoundManager.Instance.PlayClick();
            SetActiveCharacter(GetNextCharacter());
        }

        private void InvalidateButtons()
        {
            leftButton.gameObject.SetActive(GetPrevCharacter() != null);
            rightButton.gameObject.SetActive(GetNextCharacter() != null);
        }

        private CharacterData GetPrevCharacter()
        {
            var detected = false;

            for (var i=_models.Count-1;i>=0;i--)
            {
                var model = _models[i];
                if (model == ActiveCharacter)
                {
                    detected = true;
                } else if (detected && model.isSelected)
                {
                    return model;
                }
            }

            return null;
        }

        private CharacterData GetNextCharacter()
        {
            var detected = false;

            for (var i=0;i<_models.Count;i++)
            {
                var model = _models[i];
                if (model == ActiveCharacter)
                {
                    detected = true;
                }
                else if (detected && model.isSelected)
                {
                    return model;
                }
            }

            return null;
        }

        private int GetAmountOfActiveCharacters()
        {
            var count = 0;

            for (var i = 0; i < _models.Count; i++)
            {
                if (_models[i].isSelected)
                    count++;
            }

            return count;
        }
    }
}