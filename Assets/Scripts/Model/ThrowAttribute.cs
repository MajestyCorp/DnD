using UnityEngine;
using System;

namespace DnD.Model
{
    [Serializable]
    public class ThrowAttribute
    {
        public int baseValue;
        public int modifier;
        public bool master = false;

        public ThrowAttribute(int baseValue = 0, int modifier = 0, bool master = false)
        {
            this.baseValue = baseValue;
            this.modifier = modifier;
            this.master = master;
        }

        public void Invalidate(AttributeValue attribute, int bonus)
        {
            baseValue = master ? attribute.TotalModifier + bonus : attribute.TotalModifier;
            baseValue += modifier;
        }

        public int GetOriginalValue()
        {
            return baseValue - modifier;
        }

        public override string ToString()
        {
            return baseValue >= 0 ? 
                "+" + baseValue :
                baseValue.ToString();
        }
    }
}