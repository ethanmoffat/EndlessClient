using AutomaticTypeMapper;
using EOLib.IO.Repositories;
using EOLib.Localization;
using Optional;

namespace EndlessClient.HUD.Spells
{
    [AutoMappedType]
    public class SpellSelectActions : ISpellSelectActions
    {
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly ISpellSlotDataRepository _spellSlotDataRepository;
        private readonly IESFFileProvider _esfFileProvider;

        public SpellSelectActions(IStatusLabelSetter statusLabelSetter,
                                  ISpellSlotDataRepository spellSlotDataRepository,
                                  IESFFileProvider esfFileProvider)
        {
            _statusLabelSetter = statusLabelSetter;
            _spellSlotDataRepository = spellSlotDataRepository;
            _esfFileProvider = esfFileProvider;
        }

        public void SelectSpellBySlot(int slot)
        {
            _spellSlotDataRepository.SpellSlots[slot].Match(
                some: si =>
                {
                    var spellData = _esfFileProvider.ESFFile[si.ID];

                    if (spellData.Target == EOLib.IO.SpellTarget.Normal)
                    {
                        _statusLabelSetter.SetStatusLabel(EOResourceID.SKILLMASTER_WORD_SPELL, $"{spellData.Name} ", EOResourceID.SPELL_WAS_SELECTED);
                    }
                    else if (spellData.Target == EOLib.IO.SpellTarget.Group /*&& not in party*/) // todo: parties
                    {
                        _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.SPELL_ONLY_WORKS_ON_GROUP);
                    }

                    _spellSlotDataRepository.SelectedSpellSlot = Option.Some(slot);
                    _spellSlotDataRepository.SpellIsPrepared = true;
                },
                none: () =>
                {
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.SPELL_NOTHING_WAS_SELECTED);

                    _spellSlotDataRepository.SelectedSpellSlot = Option.None<int>();
                    _spellSlotDataRepository.SpellIsPrepared = false;
                });
        }
    }

    public interface ISpellSelectActions
    {
        void SelectSpellBySlot(int slot);
    }
}
