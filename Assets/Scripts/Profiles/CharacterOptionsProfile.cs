using System;
using System.Collections.Generic;
using UnityEngine;

namespace DnD.Profiles
{
    [Serializable]
    public class CharacterOption
    {
        // Stable, language-independent ID. Do not change after releasing saved profiles.
        public string id;
        public string displayName;
    }

    [CreateAssetMenu(fileName = "CharacterOptions", menuName = "Profiles/Character Options")]
    public class CharacterOptionsProfile : ScriptableObject
    {
        [SerializeField]
        private List<CharacterOption> races = new();
        [SerializeField]
        private List<CharacterOption> classes = new();

        public IReadOnlyList<CharacterOption> Races => races;
        public IReadOnlyList<CharacterOption> Classes => classes;
    }
}
