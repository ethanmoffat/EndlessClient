// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.GameExecution;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Factories
{
    public class CharacterRendererFactory : ICharacterRendererFactory
    {
        private readonly IEndlessGameProvider _gameProvider;
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEIFFileProvider _eifFileProvider;

        public CharacterRendererFactory(IEndlessGameProvider gameProvider,
                                        INativeGraphicsManager nativeGraphicsManager,
                                        IEIFFileProvider eifFileProvider)
        {
            _gameProvider = gameProvider;
            _nativeGraphicsManager = nativeGraphicsManager;
            _eifFileProvider = eifFileProvider;
        }

        public ICharacterRenderer CreateCharacterRenderer(ICharacterRenderProperties initialRenderProperties)
        {
            return new CharacterRenderer((Game)_gameProvider.Game,
                _nativeGraphicsManager,
                _eifFileProvider,
                initialRenderProperties);
        }
    }
}