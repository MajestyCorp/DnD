using DnD.Model;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace DnD.UI
{
    public class LifeOrDeathRolls : MonoBehaviour
    {
        [SerializeField, Header("Rolls")]
        private List<ToggleRoll> lifeToggles;
        [SerializeField]
        private List<ToggleRoll> deathToggles;

        private CharacterData _character = null;

        private void Awake()
        {
            CharacterManager.Instance.OnSetActiveCharacter += OnSetActiveCharacter;
            Invalidate(CharacterManager.Instance.ActiveCharacter);
        }

        private void OnSetActiveCharacter(CharacterData data)
        {
            Invalidate(data);
        }

        private void Invalidate(CharacterData data)
        {
            _character = data;
            ResetRolls();

            if (data == null)
                return;

            for (var i=0;i<lifeToggles.Count;i++)
            {
                var toggle = lifeToggles[i];
                toggle.ForceToggleValue(data.lifeRolls > i);
            }

            for (var i = 0; i < deathToggles.Count; i++)
            {
                var toggle = deathToggles[i];
                toggle.ForceToggleValue(data.deathRolls > i);
            }
        }

        private void ResetRolls()
        {
            for (var i = 0; i < lifeToggles.Count; i++)
            {
                lifeToggles[i].ForceToggleValue(false);
            }

            for (var i = 0; i < deathToggles.Count; i++)
            {
                deathToggles[i].ForceToggleValue(false);
            }
        }

        public void StateChanged()
        {
            var life = 0;
            var death = 0;

            for (var i = 0; i < lifeToggles.Count; i++)
            {
                var toggle = lifeToggles[i];
                if (toggle.IsOn)
                    life = i + 1;
            }

            for (var i = 0; i < deathToggles.Count; i++)
            {
                var toggle = deathToggles[i];
                if (toggle.IsOn)
                    death = i + 1;
            }

            _character.lifeRolls = life;
            _character.deathRolls = death;
            _character.Save();
        }
    }
}