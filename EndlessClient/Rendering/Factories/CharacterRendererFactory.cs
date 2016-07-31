// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.GameExecution;
using EndlessClient.Rendering.CharacterProperties;
using EOLib.Domain.Character;
using EOLib.Graphics;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Factories
{
    public class CharacterRendererFactory : ICharacterRendererFactory
    {
        private readonly IEndlessGameProvider _gameProvider;
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICharacterRenderOffsetCalculator _characterRenderOffsetCalculator;
        private readonly ICharacterPropertyRendererBuilder _characterPropertyRendererBuilder;
        private readonly ICharacterTextures _characterTextures;

        public CharacterRendererFactory(IEndlessGameProvider gameProvider,
                                        INativeGraphicsManager nativeGraphicsManager,
                                        ICharacterProvider characterProvider,
                                        ICharacterRenderOffsetCalculator characterRenderOffsetCalculator,
                                        ICharacterPropertyRendererBuilder characterPropertyRendererBuilder,
                                        ICharacterTextures characterTextures)
        {
            _gameProvider = gameProvider;
            _nativeGraphicsManager = nativeGraphicsManager;
            _characterProvider = characterProvider;
            _characterRenderOffsetCalculator = characterRenderOffsetCalculator;
            _characterPropertyRendererBuilder = characterPropertyRendererBuilder;
            _characterTextures = characterTextures;
        }

        public ICharacterRenderer CreateCharacterRenderer(ICharacterRenderProperties initialRenderProperties)
        {
            return new CharacterRenderer((Game) _gameProvider.Game,
                _nativeGraphicsManager,
                _characterProvider,
                _characterRenderOffsetCalculator,
                _characterPropertyRendererBuilder,
                _characterTextures,
                initialRenderProperties);
        }
    }
}