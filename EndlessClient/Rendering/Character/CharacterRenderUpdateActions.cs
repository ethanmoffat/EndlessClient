// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Factories;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Character
{
    public class CharacterRenderUpdateActions : ICharacterRenderUpdateActions
    {
        private readonly ICharacterProvider _characterProvider;
        private readonly ICharacterRendererFactory _characterRendererFactory;
        private readonly ICharacterRendererRepository _characterRendererRepository;
        private readonly ICharacterStateCache _characterStateCache;

        public CharacterRenderUpdateActions(ICharacterProvider characterProvider,
            ICharacterRendererFactory characterRendererFactory,
            ICharacterRendererRepository characterRendererRepository,
            ICharacterStateCache characterStateCache)
        {
            _characterProvider = characterProvider;
            _characterRendererFactory = characterRendererFactory;
            _characterRendererRepository = characterRendererRepository;
            _characterStateCache = characterStateCache;
        }

        public void UpdateCharacters(GameTime gameTime)
        {
            CacheMainCharacterRenderProperties();
            UpdateAllCharacters(gameTime);
        }

        private void CacheMainCharacterRenderProperties()
        {
            var actualProperties = _characterProvider.ActiveCharacter.RenderProperties;
            var cachedProperties = _characterStateCache.ActiveCharacterRenderProperties;

            if (!cachedProperties.HasValue)
            {
                _characterStateCache.UpdateActiveCharacterState(actualProperties);

                var renderer = _characterRendererFactory.CreateCharacterRenderer(actualProperties);
                _characterRendererRepository.ActiveCharacterRenderer = renderer;
                _characterRendererRepository.ActiveCharacterRenderer.Initialize();

                return;
            }

            if (cachedProperties.Value == actualProperties)
                return;

            _characterRendererRepository.ActiveCharacterRenderer.RenderProperties = actualProperties;
            _characterStateCache.UpdateActiveCharacterState(actualProperties);
        }

        private void UpdateAllCharacters(GameTime gameTime)
        {
            _characterRendererRepository.ActiveCharacterRenderer.Update(gameTime);
            foreach (var renderer in _characterRendererRepository.CharacterRenderers)
                renderer.Update(gameTime);
        }
    }
}