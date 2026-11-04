using DnD.Converters;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace DnD.Model
{
    [System.Serializable]
    public enum EContentType
    {
        Title = 0,
        Description = 1,
        IconTooltip = 2,
        SpellCells = 3,
    }

    [System.Serializable]
    public abstract class TabContent
    {
        public EContentType contentType;
        public abstract bool HasIndent();
        public abstract TabContent Clone();

        public static TabContent Factory(EContentType contentType)
        {
            return contentType switch
            {
                EContentType.Title => new TitleContent(),
                EContentType.Description => new DescriptionContent(),
                EContentType.IconTooltip => new IconTooltipContent(),
                EContentType.SpellCells => new SpellCellsContent(),
                _ => throw new Exception($"Unknown type {contentType}")
            };
        }
    }

    public class TitleContent : TabContent
    {
        public string title;

        public TitleContent()
        {
            contentType = EContentType.Title;
        }

        public override bool HasIndent()
        {
            return false;
        }

        public override TabContent Clone()
        {
            var clone = new TitleContent();
            clone.title = title;

            return clone;
        }
    }

    public class DescriptionContent : TabContent
    {
        public string title;
        public string description;

        public DescriptionContent()
        {
            contentType = EContentType.Description;
        }

        public override bool HasIndent()
        {
            return true;
        }

        public override TabContent Clone()
        {
            var clone = new DescriptionContent();
            clone.title = title;
            clone.description = description;

            return clone;
        }
    }

    public class IconTooltipContent : TabContent
    {
        public int icon_group;
        public int icon_id;
        public string title;
        public string description;
        public string tooltip;

        public IconTooltipContent()
        {
            contentType = EContentType.IconTooltip;
        }

        public override bool HasIndent()
        {
            return true;
        }

        public override TabContent Clone()
        {
            var clone = new IconTooltipContent();
            clone.icon_group = icon_group;
            clone.icon_id = icon_id;
            clone.title = title;
            clone.description = description;
            clone.tooltip = tooltip;

            return clone;
        }
    }

    public class SpellCellsContent : TabContent
    {
        public int level;
        public int maxSlots;
        public int usedSlots;

        public SpellCellsContent()
        {
            contentType = EContentType.SpellCells;
        }

        public override bool HasIndent()
        {
            return true;
        }

        public override TabContent Clone()
        {
            var clone = new SpellCellsContent();
            clone.level = level;
            clone.maxSlots = maxSlots;
            clone.usedSlots = usedSlots;

            return clone;
        }
    }
}
