using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EndlessClient.HUD.Spells;
using EndlessClient.Input;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.Factories;
using EndlessClient.Rendering.Sprites;
using EOLib.Graphics;
using EOLib.IO.Repositories;

namespace EndlessClient.Rendering.NPC
{
    [MappedType(BaseType = typeof(INPCRendererFactory))]
    public class NPCRendererFactory : INPCRendererFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEndlessGameProvider _endlessGameProvider;
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly INPCSpriteSheet _npcSpriteSheet;
        private readonly IGridDrawCoordinateCalculator _gridDrawCoordinateCalculator;
        private readonly IHealthBarRendererFactory _healthBarRendererFactory;
        private readonly IChatBubbleFactory _chatBubbleFactory;
        private readonly IRenderTargetFactory _renderTargetFactory;
        private readonly INPCInteractionController _npcInteractionController;
        private readonly IMapInteractionController _mapInteractionController;
        private readonly IUserInputProvider _userInputProvider;
        private readonly ISpellSlotDataProvider _spellSlotDataProvider;
        private readonly ISfxPlayer _sfxPlayer;

        public NPCRendererFactory(INativeGraphicsManager nativeGraphicsManager,
                                  IEndlessGameProvider endlessGameProvider,
                                  ICharacterRendererProvider characterRendererProvider,
                                  IENFFileProvider enfFileProvider,
                                  INPCSpriteSheet npcSpriteSheet,
                                  IGridDrawCoordinateCalculator gridDrawCoordinateCalculator,
                                  IHealthBarRendererFactory healthBarRendererFactory,
                                  IChatBubbleFactory chatBubbleFactory,
                                  IRenderTargetFactory renderTargetFactory,
                                  INPCInteractionController npcInteractionController,
                                  IMapInteractionController mapInteractionController,
                                  IUserInputProvider userInputProvider,
                                  ISpellSlotDataProvider spellSlotDataProvider,
                                  ISfxPlayer sfxPlayer)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _endlessGameProvider = endlessGameProvider;
            _characterRendererProvider = characterRendererProvider;
            _enfFileProvider = enfFileProvider;
            _npcSpriteSheet = npcSpriteSheet;
            _gridDrawCoordinateCalculator = gridDrawCoordinateCalculator;
            _healthBarRendererFactory = healthBarRendererFactory;
            _chatBubbleFactory = chatBubbleFactory;
            _renderTargetFactory = renderTargetFactory;
            _npcInteractionController = npcInteractionController;
            _mapInteractionController = mapInteractionController;
            _userInputProvider = userInputProvider;
            _spellSlotDataProvider = spellSlotDataProvider;
            _sfxPlayer = sfxPlayer;
        }

        public INPCRenderer CreateRendererFor(EOLib.Domain.NPC.NPC npc)
        {
            return new NPCRenderer(_nativeGraphicsManager,
                                   _endlessGameProvider,
                                   _characterRendererProvider,
                                   _enfFileProvider,
                                   _npcSpriteSheet,
                                   _gridDrawCoordinateCalculator,
                                   _healthBarRendererFactory,
                                   _chatBubbleFactory,
                                   _renderTargetFactory,
                                   _npcInteractionController,
                                   _mapInteractionController,
                                   _userInputProvider,
                                   _spellSlotDataProvider,
                                   _sfxPlayer,
                                   npc);
        }
    }

    public interface INPCRendererFactory
    {
        INPCRenderer CreateRendererFor(EOLib.Domain.NPC.NPC npc);
    }
}
