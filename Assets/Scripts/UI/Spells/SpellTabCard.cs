using System;

namespace DnD.UI.Spells
{
    // Compact character card; SpellCardDrag handles reordering and exclusion.
    public sealed class SpellTabCard : SpellRowUI
    {
        public void SetEditable(bool editable, Action<SpellCardDrag> onMoved)
        {
            var drag = GetComponent<SpellCardDrag>();
            if (drag != null) drag.SetEditable(editable && SpellId != null, onMoved);
        }
    }
}
