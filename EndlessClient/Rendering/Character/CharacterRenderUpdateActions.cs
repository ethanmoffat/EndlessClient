// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Factories;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Character
{
    public class CharacterRenderUpdateActions : ICharacterRenderUpdateActions
    {
        private readonly ICharacterProvider _characterProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly ICharacterRendererFactory _characterRendererFactory;
        private readonly ICharacterRendererRepository _characterRendererRepository;
        private readonly ICharacterStateCache _characterStateCache;

        public CharacterRenderUpdateActions(ICharacterProvider characterProvider,
                                            ICurrentMapStateProvider currentMapStateProvider,
                                            ICharacterRendererFactory characterRendererFactory,
                                            ICharacterRendererRepository characterRendererRepository,
                                            ICharacterStateCache characterStateCache)
        {
            _characterProvider = characterProvider;
            _currentMapStateProvider = currentMapStateProvider;
            _characterRendererFactory = characterRendererFactory;
            _characterRendererRepository = characterRendererRepository;
            _characterStateCache = characterStateCache;
        }

        public void UpdateCharacters(GameTime gameTime)
        {
            CacheMainCharacterRenderProperties();
            CacheOtherCharacterRenderProperties();
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
                _characterRendererRepository.MainCharacterRenderer.SetToCenterScreenPosition();

                return;
            }

            if (cachedProperties.Value == actualProperties)
                return;

            _characterRendererRepository.MainCharacterRenderer.RenderProperties = actualProperties;
            _characterStateCache.UpdateMainCharacterState(actualProperties);
        }

        //todo: refactor to make this more easily readable and less branch-y
        private void CacheOtherCharacterRenderProperties()
        {
            foreach (var character in _currentMapStateProvider.Characters)
            {
                var id = character.ID;

                var actualProperties = character.RenderProperties;
                var cachedProperties = _characterStateCache.HasCharacterWithID(id)
                    ? new Optional<ICharacterRenderProperties>(_characterStateCache.CharacterRenderProperties[id])
                    : Optional<ICharacterRenderProperties>.Empty;

                if (!cachedProperties.HasValue)
                {
                    _characterStateCache.UpdateCharacterState(id, actualProperties);

                    var renderer = _characterRendererFactory.CreateCharacterRenderer(actualProperties);

                    //todo: figure out how to clean up existing renderers
                    //if (_characterRendererRepository.CharacterRenderers.ContainsKey(id))
                    //    _characterRendererRepository.CharacterRenderers[id].Dispose();
                    _characterRendererRepository.CharacterRenderers.Add(id, renderer);
                    _characterRendererRepository.CharacterRenderers[id].Initialize();

                    continue;
                }

                if (cachedProperties.Value == actualProperties)
                    continue;

                _characterRendererRepository.CharacterRenderers[id].RenderProperties = actualProperties;
                _characterStateCache.UpdateCharacterState(id, actualProperties);
            }
        }

        private void UpdateAllCharacters(GameTime gameTime)
        {
            _characterRendererRepository.MainCharacterRenderer.Update(gameTime);
            foreach (var renderer in _characterRendererRepository.CharacterRenderers.Values)
                renderer.Update(gameTime);
        }
    }
}