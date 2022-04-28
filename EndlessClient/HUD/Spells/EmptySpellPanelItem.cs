using EndlessClient.HUD.Panels;
using EOLib.Domain.Character;
using EOLib.IO.Pub;
using System;

namespace EndlessClient.HUD.Spells
{
    public class EmptySpellPanelItem : BaseSpellPanelItem
    {
        public override InventorySpell InventorySpell
        {
            get => new InventorySpell(0, 0);
            set => throw new InvalidOperationException("There is no Spell associated with this slot");
        }

        public override ESFRecord SpellData => new ESFRecord();

        public override event EventHandler<SpellPanelItem.SpellDragCompletedEventArgs> DoneDragging
        {
            add => throw new InvalidOperationException("No dragging of empty spell");
            remove => throw new InvalidOperationException("No dragging of empty spell");
        }

        public EmptySpellPanelItem(ActiveSpellsPanel parent, int slot)
            : base(parent, slot)
        {
        }
    }
}
