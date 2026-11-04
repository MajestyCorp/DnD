using System;
using UnityEngine;

namespace DnD.Model
{
    [Serializable]
    public class AttributeValue
    {
        public int baseValue;
        public int modifier;
        public bool auto = true;
        public int bonus;

        [Newtonsoft.Json.JsonIgnore]
        public int TotalModifier => modifier + bonus;

        public AttributeValue(int baseValue = 10, int modifier = 0, bool auto = true)
        {
            this.baseValue = baseValue;
            this.modifier = modifier;
            this.auto = auto;
        }

        public void CalculateModifier()
        {
            modifier = Mathf.FloorToInt((baseValue - 10) / 2f);
        }

        public override string ToString()
        {
            return baseValue.ToString() + (TotalModifier >= 0 ? "+" : "") + TotalModifier;
        }
    }
}