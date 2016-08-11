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
            var actualProperties = _characterProvider.MainCharacter.RenderProperties;
            var cachedProperties = _characterStateCache.MainCharacterRenderProperties;

            if (!cachedProperties.HasValue)
            {
                _characterStateCache.UpdateMainCharacterState(actualProperties);

                var renderer = _characterRendererFactory.CreateCharacterRenderer(actualProperties);
                _characterRendererRepository.MainCharacterRenderer = renderer;
                _characterRendererRepository.MainCharacterRenderer.Initialize();

                return;
            }

            if (cachedProperties.Value == actualProperties)
                return;

            _characterRendererRepository.MainCharacterRenderer.RenderProperties = actualProperties;
            _characterStateCache.UpdateMainCharacterState(actualProperties);
        }

        private void UpdateAllCharacters(GameTime gameTime)
        {
            _characterRendererRepository.MainCharacterRenderer.Update(gameTime);
            foreach (var renderer in _characterRendererRepository.CharacterRenderers.Values)
                renderer.Update(gameTime);
        }
    }
}