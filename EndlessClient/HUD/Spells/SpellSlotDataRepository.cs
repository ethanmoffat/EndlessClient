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
        /// Spell info for the selected spell (based on SelectedSpellSlot value)
        /// </summary>
        Option<InventorySpell> SelectedSpellInfo { get; }

        /// <summary>
        /// True if the selected spell slot has been prepared by using a hotkey.
        /// </summary>
        bool SpellIsPrepared { get; set; }

        /// <summary>
        /// Array of inventory spells by their slot number.
        /// </summary>
        Option<InventorySpell>[] SpellSlots { get; set; }
    }

    public interface ISpellSlotDataProvider
    {
        Option<int> SelectedSpellSlot { get; }

        Option<InventorySpell> SelectedSpellInfo { get; }

        bool SpellIsPrepared { get; }

        IReadOnlyList<Option<InventorySpell>> SpellSlots { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class SpellSlotDataRepository : ISpellSlotDataRepository, ISpellSlotDataProvider
    {
        public Option<int> SelectedSpellSlot { get; set; }

        public Option<InventorySpell> SelectedSpellInfo =>
            SelectedSpellSlot.Match(
                x => SpellSlots[x].Match(
                    y => Option.Some(y),
                    () => Option.None<InventorySpell>()),
                () => Option.None<InventorySpell>());

        public bool SpellIsPrepared { get; set; }

        public Option<InventorySpell>[] SpellSlots { get; set; }

        IReadOnlyList<Option<InventorySpell>> ISpellSlotDataProvider.SpellSlots => SpellSlots;

        public SpellSlotDataRepository()
        {
            SpellSlots = new Option<InventorySpell>[ActiveSpellsPanel.SpellRows * ActiveSpellsPanel.SpellRowLength];
        }
    }
}