using EOLib.Domain.Character;
using EOLib.IO.Pub;
using System;
using XNAControls;

namespace EndlessClient.HUD.Spells
{
    public interface ISpellPanelItem : IXNAControl
    {
        int Slot { get; set; }

        bool IsSelected { get; set; }

        bool IsBeingDragged { get; }

        IInventorySpell InventorySpell { get; set;  }

        ESFRecord SpellData { get; }

        event EventHandler Clicked;
        event EventHandler Selected;
    }
}
