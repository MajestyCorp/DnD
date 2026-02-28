using System;
using UnityEngine;

namespace DnD.Model
{
    [Serializable]
    public class StatValue
    {
        public int current;
        public int max;

        public StatValue(int maxValue = 10)
        {
            max = maxValue;
            current = maxValue;
        }

        public void SetMaxValue(int value)
        {
            max = current = value;
        }

        public override string ToString()
        {
            return current.ToString() + "/" + max.ToString();
        }
    }
}