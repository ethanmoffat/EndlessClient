using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.Effects;
using EndlessClient.Rendering.Metadata;
using EndlessClient.Rendering.Metadata.Models;
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
        private readonly IRenderTargetFactory _renderTargetFactory;
        private readonly IHealthBarRendererFactory _healthBarRendererFactory;
        private readonly IChatBubbleFactory _chatBubbleFactory;
        private readonly ICharacterProvider _characterProvider;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;
        private readonly ICharacterPropertyRendererBuilder _characterPropertyRendererBuilder;
        private readonly ICharacterTextures _characterTextures;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IUserInputProvider _userInputProvider;
        private readonly IEffectRendererFactory _effectRendererFactory;
        private readonly IMetadataProvider<HatMetadata> _hatMetadataProvider;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IClientWindowSizeRepository _clientWindowSizeRepository;

        public CharacterRendererFactory(IEndlessGameProvider gameProvider,
                                        IRenderTargetFactory renderTargetFactory,
                                        IHealthBarRendererFactory healthBarRendererFactory,
                                        IChatBubbleFactory chatBubbleFactory,
                                        ICharacterProvider characterProvider,
                                        IRenderOffsetCalculator renderOffsetCalculator,
                                        ICharacterPropertyRendererBuilder characterPropertyRendererBuilder,
                                        ICharacterTextures characterTextures,
                                        IGameStateProvider gameStateProvider,
                                        ICurrentMapProvider currentMapProvider,
                                        IUserInputProvider userInputProvider,
                                        IEffectRendererFactory effectRendererFactory,
                                        IMetadataProvider<HatMetadata> hatMetadataProvider,
                                        ISfxPlayer sfxPlayer,
                                        IClientWindowSizeRepository clientWindowSizeRepository)
        {
            _gameProvider = gameProvider;
            _renderTargetFactory = renderTargetFactory;
            _healthBarRendererFactory = healthBarRendererFactory;
            _chatBubbleFactory = chatBubbleFactory;
            _characterProvider = characterProvider;
            _renderOffsetCalculator = renderOffsetCalculator;
            _characterPropertyRendererBuilder = characterPropertyRendererBuilder;
            _characterTextures = characterTextures;
            _gameStateProvider = gameStateProvider;
            _currentMapProvider = currentMapProvider;
            _userInputProvider = userInputProvider;
            _effectRendererFactory = effectRendererFactory;
            _hatMetadataProvider = hatMetadataProvider;
            _sfxPlayer = sfxPlayer;
            _clientWindowSizeRepository = clientWindowSizeRepository;
        }

        public ICharacterRenderer CreateCharacterRenderer(EOLib.Domain.Character.Character character)
        {
            return new CharacterRenderer(
                (Game) _gameProvider.Game,
                _renderTargetFactory,
                _healthBarRendererFactory,
                _chatBubbleFactory,
                _characterProvider,
                _renderOffsetCalculator,
                _characterPropertyRendererBuilder,
                _characterTextures,
                character,
                _gameStateProvider,
                _currentMapProvider,
                _userInputProvider,
                _effectRendererFactory,
                _hatMetadataProvider,
                _sfxPlayer,
                _clientWindowSizeRepository);
        }
    }
}