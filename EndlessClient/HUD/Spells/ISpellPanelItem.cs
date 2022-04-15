using EOLib.Domain.Character;
using EOLib.IO.Pub;
using System;
using XNAControls;

using static EndlessClient.HUD.Spells.SpellPanelItem;

namespace EndlessClient.HUD.Spells
{
    public interface ISpellPanelItem : IXNAControl
    {
        int Slot { get; set; }

        int DisplaySlot { get; set; }

        bool IsBeingDragged { get; }

        IInventorySpell InventorySpell { get; set;  }

        ESFRecord SpellData { get; }

        event EventHandler Clicked;

        event EventHandler<SpellDragCompletedEventArgs> DoneDragging;
    }
}
