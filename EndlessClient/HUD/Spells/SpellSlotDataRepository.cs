using AutomaticTypeMapper;
using EndlessClient.HUD.Panels;
using EOLib.Domain.Character;
using Optional;
using System.Collections.Generic;

namespace EndlessClient.HUD.Spells
{
    public interface ISpellSlotDataRepository
    {
        /// <summary>
        /// Slot for the selected spell displayed in the UI (or none if nothing is selected). A selected spell is not necessarily prepared for cast via hotkey selection.
        /// </summary>
        Option<int> SelectedSpellSlot { get; set; }

        /// <summary>
        /// True if the selected spell slot has been prepared by using a hotkey.
        /// </summary>
        bool SpellIsPrepared { get; set; }

        /// <summary>
        /// Array of inventory spells by their slot number.
        /// </summary>
        Option<IInventorySpell>[] SpellSlots { get; set; }
    }

    public interface ISpellSlotDataProvider
    {
        Option<int> SelectedSpellSlot { get; }

        bool SpellIsPrepared { get; }

        IReadOnlyCollection<Option<IInventorySpell>> SpellSlots { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class SpellSlotDataRepository : ISpellSlotDataRepository, ISpellSlotDataProvider
    {
        public Option<int> SelectedSpellSlot { get; set; }

        public bool SpellIsPrepared { get; set; }

        public Option<IInventorySpell>[] SpellSlots { get; set; }

        IReadOnlyCollection<Option<IInventorySpell>> ISpellSlotDataProvider.SpellSlots => SpellSlots;

        public SpellSlotDataRepository()
        {
            SpellSlots = new Option<IInventorySpell>[ActiveSpellsPanel.SpellRows * ActiveSpellsPanel.SpellRowLength];
        }
    }
}
