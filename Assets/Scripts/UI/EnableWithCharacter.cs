using DnD.Model;
using System;
using UnityEngine;

namespace DnD.UI
{
    public class EnableWithCharacter : MonoBehaviour
    {
        private void Awake()
        {
            CharacterManager.Instance.OnSetActiveCharacter += OnSetActiveCharacter;
            Invalidate(CharacterManager.Instance.ActiveCharacter);
        }

        private void Invalidate(CharacterData activeCharacter)
        {
            if (gameObject.activeSelf && activeCharacter == null)
            {
                gameObject.SetActive(false);
                return;
            }

            if (!gameObject.activeSelf && activeCharacter != null)
            {
                gameObject.SetActive(true);
                return;
            }
        }

        private void OnSetActiveCharacter(CharacterData data)
        {
            Invalidate(data);
        }

        private void OnDestroy()
        {
            if (CharacterManager.Instance != null)
                CharacterManager.Instance.OnSetActiveCharacter -= OnSetActiveCharacter;
        }
    }
}