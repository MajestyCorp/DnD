using DnD.Model;
using System.Collections.Generic;
using UnityEngine;

namespace DnD.UI
{
    public class SkillsRow : MonoBehaviour
    {
        [SerializeField]
        private List<SkillItem> items;

        public void SetItem(int index, string name, SkillAttribute attribute, CharacterData character)
        {
            items[index].Invalidate(name, attribute, character);
        }

        public void SetEditable(bool value)
        {
            for(var i=0;i<items.Count;i++)
            {
                items[i].SetEditable(value);
            }
        }
    }
}