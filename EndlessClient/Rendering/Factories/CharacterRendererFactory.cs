using AutomaticTypeMapper;
using EndlessClient.GameExecution;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.Sprites;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Factories
{
    [MappedType(BaseType = typeof(ICharacterRendererFactory))]
    public class CharacterRendererFactory : ICharacterRendererFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEndlessGameProvider _gameProvider;
        private readonly IRenderTargetFactory _renderTargetFactory;
        private readonly IHealthBarRendererFactory _healthBarRendererFactory;
        private readonly IChatBubbleFactory _chatBubbleFactory;
        private readonly ICharacterProvider _characterProvider;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;
        private readonly ICharacterPropertyRendererBuilder _characterPropertyRendererBuilder;
        private readonly ICharacterTextures _characterTextures;
        private readonly ICharacterSpriteCalculator _characterSpriteCalculator;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly ICurrentMapProvider _currentMapProvider;

        public CharacterRendererFactory(INativeGraphicsManager nativeGraphicsManager,
                                        IEndlessGameProvider gameProvider,
                                        IRenderTargetFactory renderTargetFactory,
                                        IHealthBarRendererFactory healthBarRendererFactory,
                                        IChatBubbleFactory chatBubbleFactory,
                                        ICharacterProvider characterProvider,
                                        IRenderOffsetCalculator renderOffsetCalculator,
                                        ICharacterPropertyRendererBuilder characterPropertyRendererBuilder,
                                        ICharacterTextures characterTextures,
                                        ICharacterSpriteCalculator characterSpriteCalculator,
                                        IGameStateProvider gameStateProvider,
                                        ICurrentMapProvider currentMapProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _gameProvider = gameProvider;
            _renderTargetFactory = renderTargetFactory;
            _healthBarRendererFactory = healthBarRendererFactory;
            _chatBubbleFactory = chatBubbleFactory;
            _characterProvider = characterProvider;
            _renderOffsetCalculator = renderOffsetCalculator;
            _characterPropertyRendererBuilder = characterPropertyRendererBuilder;
            _characterTextures = characterTextures;
            _characterSpriteCalculator = characterSpriteCalculator;
            _gameStateProvider = gameStateProvider;
            _currentMapProvider = currentMapProvider;
        }

        public ICharacterRenderer CreateCharacterRenderer(ICharacter character)
        {
            return new CharacterRenderer(
                _nativeGraphicsManager,
                (Game) _gameProvider.Game,
                _renderTargetFactory,
                _healthBarRendererFactory,
                _chatBubbleFactory,
                _characterProvider,
                _renderOffsetCalculator,
                _characterPropertyRendererBuilder,
                _characterTextures,
                _characterSpriteCalculator,
                character,
                _gameStateProvider,
                _currentMapProvider);
        }
    }
}