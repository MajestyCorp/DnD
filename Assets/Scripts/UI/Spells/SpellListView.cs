using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DnD.UI.Spells
{
    // Assign layoutContent for an existing LayoutGroup, or scrollRect for a virtual list.
    public class SpellListView : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private SpellRowUI rowPrefab;

        private readonly List<SpellRowUI> pool = new();
        private List<SpellListEntry> entries = new();
        private Action<SpellListEntry> onAction;
        private bool editable;
        private Action<string, int> onMoved;

        private void Awake()
        {
            rowPrefab.gameObject.SetActive(false);
        }

        public void SetEditable(bool value, Action<string, int> onMoved)
        {
            editable = value;
            this.onMoved = onMoved;
            foreach (var row in pool) 
                ConfigureDrag(row);
        }

        private void ConfigureDrag(SpellRowUI row)
        {
            if (row is SpellTabCard card)
                card.SetEditable(editable, CardMoved);
        }

        private void CardMoved(SpellCardDrag drag)
        {
            var ordinal = 0;
            for (var i = 0; i < content.childCount; i++)
            {
                var child = content.GetChild(i);
                var row = child.GetComponent<SpellRowUI>();
                if (!child.gameObject.activeSelf || row?.SpellId == null) 
                    continue;
                if (child == drag.transform) 
                { 
                    onMoved?.Invoke(row.SpellId, ordinal); 
                    return; 
                }
                ordinal++;
            }
        }

        public void SetItems(List<SpellListEntry> entries, Action<SpellListEntry> onAction)
        {
            foreach (var row in pool) 
                row.GetComponent<SpellCardDrag>()?.CancelDrag();

            this.entries = entries;
            this.onAction = onAction;
            
            Refresh();
        }

        private void Refresh()
        {
            while (pool.Count < entries.Count)
            {
                var row = Instantiate(rowPrefab, content);
                pool.Add(row);
            }

            for (var i = 0; i < pool.Count; i++)
            {
                var row = pool[i];
                row.gameObject.SetActive(i < entries.Count);

                if (i >= entries.Count) 
                    continue;

                row.transform.SetAsLastSibling();
                row.Bind(entries[i], onAction);
                ConfigureDrag(row);
            }
        }
    }
}
