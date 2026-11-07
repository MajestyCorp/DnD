using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DnD.UI
{
    // Rebuilt whenever the parent class changes. Stores IDs independently of labels.
    public class CharacterSubclassDropdown
    {
        private TMP_Dropdown dropdown;
        private readonly List<string> ids = new();
        public string SelectedId => dropdown != null && dropdown.value > 0 && dropdown.value < ids.Count
            ? ids[dropdown.value] : null;

        public void Initialize(TMP_Dropdown dropdown, GameObject panel, string classId,
            string selectedId = null, string emptyLabel = "Не выбран")
        {
            this.dropdown = dropdown;
            ids.Clear();
            ids.Add(null);
            var labels = new List<string> { emptyLabel };
            var subclasses = SpellLibrary.Catalog?.GetSubclasses(classId, SpellLibrary.Language);
            if (subclasses != null)
                foreach (var subclass in subclasses)
                {
                    ids.Add(subclass.id);
                    labels.Add(subclass.GetName(SpellLibrary.Language));
                }
            if (dropdown != null)
            {
                dropdown.ClearOptions();
                dropdown.AddOptions(labels);
                dropdown.SetValueWithoutNotify(System.Math.Max(0, ids.IndexOf(selectedId)));
                dropdown.RefreshShownValue();
            }
            var root = panel != null ? panel : dropdown != null ? dropdown.gameObject : null;
            if (root != null) root.SetActive(dropdown != null && ids.Count > 1);
        }
    }
}
