using EndlessClient.HUD.Panels;
using EOLib.Domain.Character;
using EOLib.IO.Pub;
using System;

namespace EndlessClient.HUD.Spells
{
    public class EmptySpellPanelItem : BaseSpellPanelItem
    {
        public override IInventorySpell InventorySpell
        {
            get => new InventorySpell(0, 0);
            set => throw new InvalidOperationException("There is no Spell associated with this slot");
        }

        public override ESFRecord SpellData => new ESFRecord();

        public EmptySpellPanelItem(ActiveSpellsPanel parent, int slot)
            : base(parent, slot)
        {
        }
    }
}
