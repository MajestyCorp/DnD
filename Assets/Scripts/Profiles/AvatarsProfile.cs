using System.Collections.Generic;
using UnityEngine;

namespace DnD.Profiles
{
    [CreateAssetMenu(fileName = "New Avatars", menuName = "Profiles/Avatars")]
    public class AvatarsProfile : ScriptableObject
    {
        [SerializeField]
        private List<Sprite> avatars;

        public IReadOnlyList<Sprite> Avatars => avatars;
    }
}