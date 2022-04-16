using AutomaticTypeMapper;
using EndlessClient.HUD.Panels;
using EndlessClient.HUD.Spells;
using EndlessClient.Rendering.Character;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.IO;
using EOLib.IO.Repositories;

namespace EndlessClient.Controllers
{
    [AutoMappedType]
    public class FunctionKeyController : IFunctionKeyController
    {
        private readonly IMapActions _mapActions;
        private readonly ICharacterActions _characterActions;
        private readonly ISpellSelectActions _spellSelectActions;
        private readonly ICharacterAnimationActions _characterAnimationActions;
        private readonly ICharacterProvider _characterProvider;
        private readonly IESFFileProvider _esfFileProvider;
        private readonly ISpellSlotDataProvider _spellSlotDataProvider;

        public FunctionKeyController(IMapActions mapActions,
                                     ICharacterActions characterActions,
                                     ISpellSelectActions spellSelectActions,
                                     ICharacterAnimationActions characterAnimationActions,
                                     ICharacterProvider characterProvider,
                                     IESFFileProvider esfFileProvider,
                                     ISpellSlotDataProvider spellSlotDataProvider)
        {
            _mapActions = mapActions;
            _characterActions = characterActions;
            _spellSelectActions = spellSelectActions;
            _characterAnimationActions = characterAnimationActions;
            _characterProvider = characterProvider;
            _esfFileProvider = esfFileProvider;
            _spellSlotDataProvider = spellSlotDataProvider;
        }

        public bool SelectSpell(int index, bool isAlternate)
        {
            if (_characterProvider.MainCharacter.RenderProperties.IsActing(CharacterActionState.Standing))
            {
                _spellSelectActions.SelectSpellBySlot(index + (isAlternate ? ActiveSpellsPanel.SpellRowLength : 0));

                _spellSlotDataProvider.SelectedSpellInfo.MatchSome(x =>
                {
                    var spellData = _esfFileProvider.ESFFile[x.ID];
                    if (spellData.Target == SpellTarget.Self && _characterAnimationActions.PrepareMainCharacterSpell(x.ID, _characterProvider.MainCharacter))
                    {
                        _characterActions.PrepareCastSpell(x.ID);
                    }
                });

                return true;
            }

            return false;
        }

        public bool Sit()
        {
            _characterActions.ToggleSit();
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
