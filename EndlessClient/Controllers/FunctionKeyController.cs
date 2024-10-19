using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Actions;
using EndlessClient.HUD;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Panels;
using EndlessClient.HUD.Spells;
using EndlessClient.Rendering;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Domain.Spells;
using EOLib.IO;
using EOLib.IO.Repositories;
using EOLib.Localization;

namespace EndlessClient.Controllers
{
    [AutoMappedType]
    public class FunctionKeyController : IFunctionKeyController
    {
        private readonly IMapActions _mapActions;
        private readonly ICharacterActions _characterActions;
        private readonly ISpellSelectActions _spellSelectActions;
        private readonly ICharacterAnimationActions _characterAnimationActions;
        private readonly ISpellCastValidationActions _spellCastValidationActions;
        private readonly IInGameDialogActions _inGameDialogActions;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly ICharacterProvider _characterProvider;
        private readonly IESFFileProvider _esfFileProvider;
        private readonly ISpellSlotDataProvider _spellSlotDataProvider;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly ISfxPlayer _sfxPlayer;

        public FunctionKeyController(IMapActions mapActions,
                                     ICharacterActions characterActions,
                                     ISpellSelectActions spellSelectActions,
                                     ICharacterAnimationActions characterAnimationActions,
                                     ISpellCastValidationActions spellCastValidationActions,
                                     IInGameDialogActions inGameDialogActions,
                                     IStatusLabelSetter statusLabelSetter,
                                     ICharacterProvider characterProvider,
                                     IESFFileProvider esfFileProvider,
                                     ISpellSlotDataProvider spellSlotDataProvider,
                                     IHudControlProvider hudControlProvider,
                                     ISfxPlayer sfxPlayer)
        {
            _mapActions = mapActions;
            _characterActions = characterActions;
            _spellSelectActions = spellSelectActions;
            _characterAnimationActions = characterAnimationActions;
            _spellCastValidationActions = spellCastValidationActions;
            _inGameDialogActions = inGameDialogActions;
            _statusLabelSetter = statusLabelSetter;
            _characterProvider = characterProvider;
            _esfFileProvider = esfFileProvider;
            _spellSlotDataProvider = spellSlotDataProvider;
            _hudControlProvider = hudControlProvider;
            _sfxPlayer = sfxPlayer;
        }

        public bool SelectSpell(int index, bool isAlternate)
        {
            if (_characterProvider.MainCharacter.RenderProperties.IsActing(CharacterActionState.Standing))
            {
                _spellSelectActions.SelectSpellBySlot(index + (isAlternate ? ActiveSpellsPanel.SpellRowLength : 0));

                _spellSlotDataProvider.SelectedSpellInfo.Match(
                    some: x =>
                    {
                        var spellData = _esfFileProvider.ESFFile[x.ID];
                        if (spellData.Type == SpellType.Bard && _spellCastValidationActions.ValidateBard())
                        {
                            _inGameDialogActions.ShowBardDialog();
                        }
                        else
                        {
                            var castResult = _spellCastValidationActions.ValidateSpellCast(x.ID);

                            switch (castResult)
                            {
                                case SpellCastValidationResult.ExhaustedNoTp:
                                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.ATTACK_YOU_ARE_EXHAUSTED_TP);
                                    break;
                                case SpellCastValidationResult.ExhaustedNoSp:
                                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.ATTACK_YOU_ARE_EXHAUSTED_SP);
                                    break;
                                case SpellCastValidationResult.NotMemberOfGroup:
                                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.SPELL_ONLY_WORKS_ON_GROUP);
                                    break;
                                case SpellCastValidationResult.Frozen:
                                    // no-op
                                    break;
                                default:
                                    _statusLabelSetter.SetStatusLabel(EOResourceID.SKILLMASTER_WORD_SPELL, $"{spellData.Name} ", EOResourceID.SPELL_WAS_SELECTED);
                                    if (spellData.Target == SpellTarget.Normal)
                                    {
                                        _sfxPlayer.PlaySfx(SoundEffectID.SpellActivate);
                                    }
                                    else if (_characterAnimationActions.PrepareMainCharacterSpell(x.ID, _characterProvider.MainCharacter))
                                    {
                                        _characterActions.PrepareCastSpell(x.ID);
                                    }
                                    break;
                            }
                        }
                    },
                    none: () => _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.SPELL_NOTHING_WAS_SELECTED)
                );

                return true;
            }

            return false;
        }

        public bool Sit()
        {
            if (_characterProvider.MainCharacter.RenderProperties.IsActing(CharacterActionState.Walking, CharacterActionState.Attacking, CharacterActionState.SpellCast))
                return false;

            var cursorRenderer = _hudControlProvider.GetComponent<IMapRenderer>(HudControlIdentifier.MapRenderer);
            _characterActions.Sit(cursorRenderer.GridCoordinates);

            return true;
        }

        public bool RefreshMapState()
        {
            _mapActions.RequestRefresh();
            return true;
        }
    }

    public interface IFunctionKeyController
    {
        bool SelectSpell(int index, bool isAlternate);

        bool Sit();

        bool RefreshMapState();
    }
}
