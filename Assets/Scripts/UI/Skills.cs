using DnD.Model;
using System.Collections.Generic;
using UnityEngine;

namespace DnD.UI
{
    public class Skills : MonoBehaviour, IEditable
    {
        [SerializeField]
        private SkillsRow row;
        [SerializeField]
        private Transform content;

        private bool editable = false;
        private List<SkillsRow> items = new();

        public void Invalidate(CharacterData data)
        {
            data.skills.Invalidate(data);

            if (items.Count == 0)
                InitRows(data);
            else
                UpdateRows(data);
        }

        public void SetEditable(bool value)
        {
            editable = value;

            for(var i=0;i<items.Count;i++)
            {
                items[i].SetEditable(value);
            }
        }

        private void InitRows(CharacterData data)
        {
            var skills = data.skills.attributes;
            var index = 0;
            var row = this.row;
            items.Add(row);

            foreach (var (key, value) in skills)
            {
                if (index % 3 == 0 && index > 0)
                {
                    row = Instantiate(this.row, content);
                    items.Add(row);
                }

                row.SetItem(index % 3, key, value, data);
                row.SetEditable(editable);

                index++;
            }
        }

        private void UpdateRows(CharacterData data)
        {
            var skills = data.skills.attributes;
            var index = 0;

            foreach (var (key, value) in skills)
            {
                var row = items[(int)(index / 3)];
                row.SetItem(index % 3, key, value, data);
                row.SetEditable(editable);

                index++;
            }
        }
    }
}