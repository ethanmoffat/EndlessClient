// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using AutomaticTypeMapper;
using EndlessClient.GameExecution;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Sprites;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Factories
{
    [MappedType(BaseType = typeof(ICharacterRendererFactory))]
    public class CharacterRendererFactory : ICharacterRendererFactory
    {
        private readonly IEndlessGameProvider _gameProvider;
        private readonly IRenderTargetFactory _renderTargetFactory;
        private readonly ICharacterProvider _characterProvider;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;
        private readonly ICharacterPropertyRendererBuilder _characterPropertyRendererBuilder;
        private readonly ICharacterTextures _characterTextures;
        private readonly ICharacterSpriteCalculator _characterSpriteCalculator;
        private readonly IGameStateProvider _gameStateProvider;

        public CharacterRendererFactory(IEndlessGameProvider gameProvider,
                                        IRenderTargetFactory renderTargetFactory,
                                        ICharacterProvider characterProvider,
                                        IRenderOffsetCalculator renderOffsetCalculator,
                                        ICharacterPropertyRendererBuilder characterPropertyRendererBuilder,
                                        ICharacterTextures characterTextures,
                                        ICharacterSpriteCalculator characterSpriteCalculator,
                                        IGameStateProvider gameStateProvider)
        {
            _gameProvider = gameProvider;
            _renderTargetFactory = renderTargetFactory;
            _characterProvider = characterProvider;
            _renderOffsetCalculator = renderOffsetCalculator;
            _characterPropertyRendererBuilder = characterPropertyRendererBuilder;
            _characterTextures = characterTextures;
            _characterSpriteCalculator = characterSpriteCalculator;
            _gameStateProvider = gameStateProvider;
        }

        public ICharacterRenderer CreateCharacterRenderer(ICharacterRenderProperties initialRenderProperties)
        {
            return new CharacterRenderer((Game) _gameProvider.Game,
                _renderTargetFactory,
                _characterProvider,
                _renderOffsetCalculator,
                _characterPropertyRendererBuilder,
                _characterTextures,
                _characterSpriteCalculator,
                initialRenderProperties,
                _gameStateProvider);
        }
    }
}