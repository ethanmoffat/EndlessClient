using AutomaticTypeMapper;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.Effects;
using EndlessClient.Rendering.Factories;
using EndlessClient.Rendering.Sprites;
using EOLib.IO.Repositories;

namespace EndlessClient.Rendering.NPC
{
    [AutoMappedType]
    public class NPCRendererFactory : INPCRendererFactory
    {
        private readonly IEndlessGameProvider _endlessGameProvider;
        private readonly IClientWindowSizeProvider _clientWindowSizeProvider;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly INPCSpriteSheet _npcSpriteSheet;
        private readonly INPCSpriteDataCache _npcSpriteDataCache;
        private readonly IGridDrawCoordinateCalculator _gridDrawCoordinateCalculator;
        private readonly IHealthBarRendererFactory _healthBarRendererFactory;
        private readonly IChatBubbleFactory _chatBubbleFactory;
        private readonly IRenderTargetFactory _renderTargetFactory;
        private readonly IUserInputProvider _userInputProvider;
        private readonly IEffectRendererFactory _effectRendererFactory;

        public NPCRendererFactory(IEndlessGameProvider endlessGameProvider,
                                  IClientWindowSizeProvider clientWindowSizeProvider,
                                  IENFFileProvider enfFileProvider,
                                  INPCSpriteSheet npcSpriteSheet,
                                  INPCSpriteDataCache npcSpriteDataCache,
                                  IGridDrawCoordinateCalculator gridDrawCoordinateCalculator,
                                  IHealthBarRendererFactory healthBarRendererFactory,
                                  IChatBubbleFactory chatBubbleFactory,
                                  IRenderTargetFactory renderTargetFactory,
                                  IUserInputProvider userInputProvider,
                                  IEffectRendererFactory effectRendererFactory)
        {
            _endlessGameProvider = endlessGameProvider;
            _clientWindowSizeProvider = clientWindowSizeProvider;
            _enfFileProvider = enfFileProvider;
            _npcSpriteSheet = npcSpriteSheet;
            _npcSpriteDataCache = npcSpriteDataCache;
            _gridDrawCoordinateCalculator = gridDrawCoordinateCalculator;
            _healthBarRendererFactory = healthBarRendererFactory;
            _chatBubbleFactory = chatBubbleFactory;
            _renderTargetFactory = renderTargetFactory;
            _userInputProvider = userInputProvider;
            _effectRendererFactory = effectRendererFactory;
        }

        public INPCRenderer CreateRendererFor(EOLib.Domain.NPC.NPC npc)
        {
            return new NPCRenderer(_endlessGameProvider,
                                   _clientWindowSizeProvider,
                                   _enfFileProvider,
                                   _npcSpriteSheet,
                                   _npcSpriteDataCache,
                                   _gridDrawCoordinateCalculator,
                                   _healthBarRendererFactory,
                                   _chatBubbleFactory,
                                   _renderTargetFactory,
                                   _userInputProvider,
                                   _effectRendererFactory,
                                   npc);
        }
    }

    public interface INPCRendererFactory
    {
        INPCRenderer CreateRendererFor(EOLib.Domain.NPC.NPC npc);
    }
}
