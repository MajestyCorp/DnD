using UnityEngine;
using System.Collections.Generic;

namespace DnD.Profiles
{
    [CreateAssetMenu(fileName = "New Item Icons", menuName = "Profiles/Item Icons")]
    public class ItemIconsProfile : ScriptableObject
    {
        [SerializeField]
        private List<ItemGroup> groups;

        public IList<ItemGroup> Groups => groups;

        public Sprite GetIcon(int groupId, int id)
        {
            if (groupId < 0 || groupId >= groups.Count)
            {
                return null;
            }

            var icons = groups[groupId].icons;

            if (id < 0 || id >= icons.Count)
            {
                return null;
            }

            return icons[id];
        }
    }

    [System.Serializable]
    public class ItemGroup
    {
        public string Name;
        public List<Sprite> icons;
    }
}