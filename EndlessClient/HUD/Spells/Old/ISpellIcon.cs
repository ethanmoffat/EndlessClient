using EOLib.IO.Pub;

namespace EndlessClient.HUD.Spells.Old
{
    public interface ISpellIcon
    {
        int Slot { get; set; }

        bool Selected { get; set; }

        bool IsDragging { get; }

        short Level { get; set; }

        ESFRecord SpellData { get; }

        void SetDisplaySlot(int displaySlot);

        //defined in XNAControl base class
        bool MouseOver { get; }

        bool Visible { get; set; }
    }
}