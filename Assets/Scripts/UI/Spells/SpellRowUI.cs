using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DnD.UI.Spells
{
    public class SpellRowUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private TMP_Text subtitle2Text;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField, Min(0)] private int previewLength = 180;
        [SerializeField] private GameObject[] cardDecorations;
        [SerializeField] private Transform tooltipInfo;

        protected SpellListEntry entry;
        public string SpellId => entry?.Header == null ? entry?.Id : null;
        private Action<SpellListEntry> onAction;

        protected virtual void Awake()
        {
            titleText.richText = false;
            if (subtitleText != null) subtitleText.richText = false;
            if (subtitle2Text != null) subtitle2Text.richText = false;
            if (descriptionText != null) descriptionText.richText = false;
        }

        public virtual void Bind(SpellListEntry entry, Action<SpellListEntry> onAction)
        {
            this.entry = entry;
            this.onAction = onAction;
            var header = entry.Header != null;
            var text = entry.Spell?.GetText(SpellLibrary.Language);
            titleText.text = header ? entry.Header : text?.name ?? "Заклинание отсутствует в библиотеке";

            if (subtitleText != null)
            {
                subtitleText.text = text == null ? "" : FormatSubtitle(entry.Spell, text);
                subtitleText.gameObject.SetActive(!header && text != null);
            }

            if (subtitle2Text != null)
            {
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(text?.castingTime)) parts.Add(text.castingTime.Trim());
                if (!string.IsNullOrWhiteSpace(text?.duration)) parts.Add(text.duration.Trim());
                if (!string.IsNullOrWhiteSpace(text?.range)) parts.Add(text.range.Trim());
                subtitle2Text.text = string.Join(" · ", parts);
                subtitle2Text.gameObject.SetActive(!header && parts.Count > 0);
            }

            if (descriptionText != null)
            {
                var preview = (text?.description ?? "").Replace('\n', ' ');
                descriptionText.text = preview.Length > previewLength ? preview.Substring(0, previewLength).TrimEnd() + "…" : preview;
                descriptionText.gameObject.SetActive(!header && descriptionText.text.Length > 0);
            }

            if (cardDecorations != null)
                foreach (var decoration in cardDecorations) 
                    if (decoration != null) 
                        decoration.SetActive(!header);
        }

        protected virtual string FormatSubtitle(DnD.Model.Spells.SpellDefinition spell, DnD.Model.Spells.SpellText text)
        {
            return (spell.level == 0 ? "Заговор" : $"{spell.level} круг") + " · " + text.school;
        }

        public void ButtonDetails()
        {
            if (entry?.Spell == null) 
                return;

            if (TooltipManager.Instance != null)
                TooltipManager.Instance.ShowFor(
                    (RectTransform)(tooltipInfo != null ? tooltipInfo : transform), 
                    SpellTooltipText.Format(entry.Spell, SpellLibrary.Language)
                );
        }
        public void ButtonAction() 
        { 
            if (entry != null && entry.Header == null) 
                onAction?.Invoke(entry); 
        }
    }
}
