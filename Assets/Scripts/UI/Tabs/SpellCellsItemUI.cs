using DnD.Model;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DnD.UI.Tabs
{
    public class SpellCellsItemUI : TabItemUI
    {
        [SerializeField, Header("UI")]
        private TMP_Text levelText;
        [SerializeField]
        private TMP_Text cellPrefab;

        private SpellCellsContent data;
        [SerializeField]
        private string emptyCell = "?";
        [SerializeField]
        private string checkedCell = "?";
        private List<TMP_Text> items = new();

        public override EContentType GetContentType()
        {
            return EContentType.SpellCells;
        }

        private void Awake()
        {
            cellPrefab.gameObject.SetActive(false);
            ResetSkills.OnReset += OnSkillsReset;
        }

        private void OnDestroy()
        {
            ResetSkills.OnReset -= OnSkillsReset;
        }

        private void OnSkillsReset()
        {
            if (data == null)
                return;
            data.usedSlots = 0;

            UpdateState();
        }

        public override void Initialize(CharacterTabContent tab, DnD.Model.TabContent item)
        {
            base.Initialize(tab, item);
            data = item as SpellCellsContent;
            Invalidate();
            gameObject.SetActive(true);
        }

        protected override void Invalidate()
        {
            levelText.text = data.level.ToString();
            levelText.gameObject.SetActive(data.level > 0);

            ClearItems();
            BuildItems();
        }

        private void ClearItems()
        {
            for(var i=0;i<items.Count;i++)
            {
                Destroy(items[i].gameObject);
            }

            items.Clear();
        }

        private void BuildItems()
        {
            for(var i=0;i<data.maxSlots;i++)
            {
                var item = Instantiate(cellPrefab, transform);
                item.text = i < data.usedSlots ? checkedCell : emptyCell;
                item.gameObject.SetActive(true);

                items.Add(item);
            }
        }

        private void UpdateState()
        {
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                item.text = i < data.usedSlots ? checkedCell : emptyCell;
            }
        }

        public void ButtonCheck()
        {
            SoundManager.Instance.PlayClick();
            data.usedSlots += 1;
            data.usedSlots = data.usedSlots % (data.maxSlots+1);
            CharacterManager.Instance.ActiveCharacter.Save();

            UpdateState();
        }
    }
}