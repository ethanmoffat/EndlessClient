// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.GameExecution;
using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Sprites;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Factories
{
    public class CharacterRendererFactory : ICharacterRendererFactory
    {
        private readonly IEndlessGameProvider _gameProvider;
        private readonly IRenderTargetFactory _renderTargetFactory;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICharacterRenderOffsetCalculator _characterRenderOffsetCalculator;
        private readonly ICharacterPropertyRendererBuilder _characterPropertyRendererBuilder;
        private readonly ICharacterTextures _characterTextures;
        private readonly ICharacterSpriteCalculator _characterSpriteCalculator;

        public CharacterRendererFactory(IEndlessGameProvider gameProvider,
                                        IRenderTargetFactory renderTargetFactory,
                                        ICharacterProvider characterProvider,
                                        ICharacterRenderOffsetCalculator characterRenderOffsetCalculator,
                                        ICharacterPropertyRendererBuilder characterPropertyRendererBuilder,
                                        ICharacterTextures characterTextures,
                                        ICharacterSpriteCalculator characterSpriteCalculator)
        {
            _gameProvider = gameProvider;
            _renderTargetFactory = renderTargetFactory;
            _characterProvider = characterProvider;
            _characterRenderOffsetCalculator = characterRenderOffsetCalculator;
            _characterPropertyRendererBuilder = characterPropertyRendererBuilder;
            _characterTextures = characterTextures;
            _characterSpriteCalculator = characterSpriteCalculator;
        }

        public ICharacterRenderer CreateCharacterRenderer(ICharacterRenderProperties initialRenderProperties)
        {
            return new CharacterRenderer((Game) _gameProvider.Game,
                _renderTargetFactory,
                _characterProvider,
                _characterRenderOffsetCalculator,
                _characterPropertyRendererBuilder,
                _characterTextures,
                _characterSpriteCalculator,
                initialRenderProperties);
        }
    }
}