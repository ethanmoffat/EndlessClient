using System.Linq;
using AutomaticTypeMapper;

using Optional;

namespace EndlessClient.HUD.Spells
{
    [AutoMappedType]
    public class SpellSelectActions : ISpellSelectActions
    {
        private readonly ISpellSlotDataRepository _spellSlotDataRepository;

        public SpellSelectActions(ISpellSlotDataRepository spellSlotDataRepository)
        {
            _spellSlotDataRepository = spellSlotDataRepository;
        }

        public void SelectSpellBySlot(int slot)
        {
            _spellSlotDataRepository.SpellSlots[slot].Match(
                some: x =>
                {
                    _spellSlotDataRepository.SelectedSpellSlot = Option.Some(slot);
                    _spellSlotDataRepository.SpellIsPrepared = true;
                },
                none: () =>
                {
                    _spellSlotDataRepository.SelectedSpellSlot = Option.None<int>();
                    _spellSlotDataRepository.SpellIsPrepared = false;
                }
            );
        }
    }

    public interface ISpellSelectActions
    {
        void SelectSpellBySlot(int slot);
    }
}
