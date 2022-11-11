using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.Effects;
using EndlessClient.Rendering.Sprites;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Factories
{
    [MappedType(BaseType = typeof(ICharacterRendererFactory))]
    public class CharacterRendererFactory : ICharacterRendererFactory
    {
        private readonly IEndlessGameProvider _gameProvider;
        private readonly IMapInteractionController _mapInteractionController;
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
        private readonly IUserInputProvider _userInputProvider;
        private readonly IEffectRendererFactory _effectRendererFactory;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IFixedTimeStepRepository _fixedTimeStepRepository;

        public CharacterRendererFactory(IEndlessGameProvider gameProvider,
                                        IMapInteractionController mapInteractionController,
                                        IRenderTargetFactory renderTargetFactory,
                                        IHealthBarRendererFactory healthBarRendererFactory,
                                        IChatBubbleFactory chatBubbleFactory,
                                        ICharacterProvider characterProvider,
                                        IRenderOffsetCalculator renderOffsetCalculator,
                                        ICharacterPropertyRendererBuilder characterPropertyRendererBuilder,
                                        ICharacterTextures characterTextures,
                                        ICharacterSpriteCalculator characterSpriteCalculator,
                                        IGameStateProvider gameStateProvider,
                                        ICurrentMapProvider currentMapProvider,
                                        IUserInputProvider userInputProvider,
                                        IEffectRendererFactory effectRendererFactory,
                                        ISfxPlayer sfxPlayer,
                                        IFixedTimeStepRepository fixedTimeStepRepository)
        {
            _gameProvider = gameProvider;
            _mapInteractionController = mapInteractionController;
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
            _userInputProvider = userInputProvider;
            _effectRendererFactory = effectRendererFactory;
            _sfxPlayer = sfxPlayer;
            _fixedTimeStepRepository = fixedTimeStepRepository;
        }

        public ICharacterRenderer CreateCharacterRenderer(EOLib.Domain.Character.Character character)
        {
            return new CharacterRenderer(
                (Game) _gameProvider.Game,
                _mapInteractionController,
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
                _currentMapProvider,
                _userInputProvider,
                _effectRendererFactory,
                _sfxPlayer,
                _fixedTimeStepRepository);
        }
    }
}