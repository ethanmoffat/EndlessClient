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
            CreateMainCharacterRendererAndCacheProperties();
            CreateOtherCharacterRenderersAndCacheProperties();
            UpdateAllCharacters(gameTime);
        }

        private void CreateMainCharacterRendererAndCacheProperties()
        {
            var actualProperties = _characterProvider.MainCharacter.RenderProperties;
            var cachedProperties = _characterStateCache.MainCharacterRenderProperties;

            if (!cachedProperties.HasValue)
            {
                _characterStateCache.UpdateMainCharacterState(actualProperties);

                var renderer = InitializeRendererForCharacter(actualProperties);
                _characterRendererRepository.MainCharacterRenderer = renderer;
                _characterRendererRepository.MainCharacterRenderer.SetToCenterScreenPosition();
            }
            else if (cachedProperties.Value != actualProperties)
            {
                _characterRendererRepository.MainCharacterRenderer.RenderProperties = actualProperties;
                _characterStateCache.UpdateMainCharacterState(actualProperties);
            }
        }

        private void CreateOtherCharacterRenderersAndCacheProperties()
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

                    var renderer = InitializeRendererForCharacter(actualProperties);

                    if (_characterRendererRepository.CharacterRenderers.ContainsKey(id))
                    {
                        _characterRendererRepository.CharacterRenderers[id].Dispose();
                        _characterRendererRepository.CharacterRenderers.Remove(id);
                    }
                    _characterRendererRepository.CharacterRenderers.Add(id, renderer);
                }
                else if (cachedProperties.Value != actualProperties)
                {
                    _characterRendererRepository.CharacterRenderers[id].RenderProperties = actualProperties;
                    _characterStateCache.UpdateCharacterState(id, actualProperties);
                }
            }
        }

        private void UpdateAllCharacters(GameTime gameTime)
        {
            _characterRendererRepository.MainCharacterRenderer.Update(gameTime);
            foreach (var renderer in _characterRendererRepository.CharacterRenderers.Values)
                renderer.Update(gameTime);
        }

        private ICharacterRenderer InitializeRendererForCharacter(ICharacterRenderProperties renderProperties)
        {
            var renderer = _characterRendererFactory.CreateCharacterRenderer(renderProperties);
            renderer.Initialize();
            return renderer;
        }
    }

    public interface ICharacterRenderUpdateActions
    {
        void UpdateCharacters(GameTime gameTime);
    }
}