using System;
using UnityEngine;
using UnityEngine.UI;

namespace DnD.UI.Spells
{
    public sealed class SpellLibraryCard : SpellRowUI
    {
        [SerializeField] private Toggle starredToggle;
        [SerializeField] private GameObject selectedHighlight;

        protected override void Awake()
        {
            base.Awake();
            if (starredToggle != null) 
                starredToggle.onValueChanged.AddListener(OnStarChanged);
        }

        private void OnDestroy()
        {
            if (starredToggle != null) 
                starredToggle.onValueChanged.RemoveListener(OnStarChanged);
        }

        public override void Bind(SpellListEntry entry, Action<SpellListEntry> onAction)
        {
            base.Bind(entry, onAction);
            if (starredToggle != null)
            {
                starredToggle.gameObject.SetActive(entry.Header == null && entry.UseStarSelection && onAction != null);
                starredToggle.SetIsOnWithoutNotify(entry.Selected);
            }
            UpdateStar(entry.Selected);
        }

        private void UpdateStar(bool selected)
        {
            if (selectedHighlight != null) selectedHighlight.SetActive(entry?.Header == null && selected);
        }

        private void OnStarChanged(bool value)
        {
            if (entry == null || entry.Header != null || !entry.UseStarSelection) return;
            UpdateStar(value);
            ButtonAction();
        }
    }
}
