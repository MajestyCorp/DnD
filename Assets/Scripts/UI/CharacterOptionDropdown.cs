using System;
using System.Collections.Generic;
using DnD.Profiles;
using TMPro;
using UnityEngine;

namespace DnD.UI
{
    // UI indices and translated labels never become character identity.
    public class CharacterOptionDropdown
    {
        private TMP_Dropdown dropdown;
        private readonly List<CharacterOption> options = new();

        public CharacterOption Selection
        {
            get
            {
                if (dropdown == null || dropdown.value <= 0 || dropdown.value > options.Count)
                    return null;
                return options[dropdown.value - 1];
            }
        }

        public void Initialize(TMP_Dropdown dropdown, IReadOnlyList<CharacterOption> choices, string selectedId = null)
        {
            this.dropdown = dropdown;
            options.Clear();
            if (dropdown == null)
                return;

            var labels = new List<string> { "Выберите…" };
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var selectedIndex = 0;
            if (choices != null)
            {
                foreach (var choice in choices)
                {
                    if (choice == null || string.IsNullOrWhiteSpace(choice.id)
                        || string.IsNullOrWhiteSpace(choice.displayName) || !ids.Add(choice.id))
                    {
                        Debug.LogWarning("Character options contain an empty or duplicate ID/name.");
                        continue;
                    }
                    options.Add(choice);
                    labels.Add(choice.displayName);
                    if (string.Equals(choice.id, selectedId, StringComparison.Ordinal))
                        selectedIndex = options.Count;
                }
            }

            dropdown.ClearOptions();
            dropdown.AddOptions(labels);
            dropdown.SetValueWithoutNotify(selectedIndex);
            dropdown.RefreshShownValue();
        }
    }
}
