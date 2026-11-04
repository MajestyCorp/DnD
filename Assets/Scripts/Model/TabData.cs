using System.Collections.Generic;
using UnityEngine;

namespace DnD.Model
{
    [System.Serializable]
    public class TabData
    {
        public string name;
        public int index;

        public List<TabContent> items = new();
    }
}