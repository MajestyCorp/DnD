using UnityEngine;
using System;

namespace DnD.Model
{
    [Serializable]
    public class SkillAttribute
    {
        public int baseValue;
        public int modifier;
        public bool master = false;

        public SkillAttribute(int baseValue = 0, int modifier = 0, bool master = false)
        {
            this.baseValue = baseValue;
            this.modifier = modifier;
            this.master = master;
        }

        public void Invalidate(ThrowAttribute attribute)
        {
            baseValue = attribute.baseValue + modifier;
            master = attribute.master;
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