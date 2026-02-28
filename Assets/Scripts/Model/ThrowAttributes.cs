using UnityEngine;
using System;

namespace DnD.Model
{
    [Serializable]
    public class ThrowAttributes
    {
        public ThrowAttribute strength = new();
        public ThrowAttribute dexterity = new();
        public ThrowAttribute constitution = new();
        public ThrowAttribute intelligence = new();
        public ThrowAttribute wisdom = new();
        public ThrowAttribute charisma = new();

        public void Invalidate(CharacterData data)
        {
            strength.Invalidate(data.strength, data.masteryBonus);
            dexterity.Invalidate(data.dexterity, data.masteryBonus);
            constitution.Invalidate(data.constitution, data.masteryBonus);
            intelligence.Invalidate(data.intelligence, data.masteryBonus);
            wisdom.Invalidate(data.wisdom, data.masteryBonus);
            charisma.Invalidate(data.charisma, data.masteryBonus);
        }
    }
}