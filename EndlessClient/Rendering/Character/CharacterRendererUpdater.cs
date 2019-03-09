// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EndlessClient.Rendering.Factories;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Character
{
    [MappedType(BaseType = typeof(ICharacterRendererUpdater))]
    public class CharacterRendererUpdater : ICharacterRendererUpdater
    {
        private readonly ICharacterProvider _characterProvider;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICharacterRendererFactory _characterRendererFactory;
        private readonly ICharacterRendererRepository _characterRendererRepository;
        private readonly ICharacterStateCache _characterStateCache;

        public CharacterRendererUpdater(ICharacterProvider characterProvider,
                                        ICurrentMapStateRepository currentMapStateRepository,
                                        ICharacterRendererFactory characterRendererFactory,
                                        ICharacterRendererRepository characterRendererRepository,
                                        ICharacterStateCache characterStateCache)
        {
            _characterProvider = characterProvider;
            _currentMapStateRepository = currentMapStateRepository;
            _characterRendererFactory = characterRendererFactory;
            _characterRendererRepository = characterRendererRepository;
            _characterStateCache = characterStateCache;
        }

        public void UpdateCharacters(GameTime gameTime)
        {
            CreateMainCharacterRendererAndCacheProperties();
            CreateOtherCharacterRenderersAndCacheProperties();
            UpdateAllCharacters(gameTime);

            RemoveStaleCharacters();
            UpdateDeadCharacters();
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
            foreach (var character in _currentMapStateRepository.Characters)
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

        private void RemoveStaleCharacters()
        {
            var staleIDs = new List<int>();
            foreach (var kvp in _characterStateCache.CharacterRenderProperties)
            {
                if (_currentMapStateRepository.Characters.Any(x => x.ID == kvp.Key))
                    continue;
                staleIDs.Add(kvp.Key);
            }

            foreach (var id in staleIDs)
            {
                _characterStateCache.RemoveCharacterState(id);
                _characterRendererRepository.CharacterRenderers[id].Dispose();
                _characterRendererRepository.CharacterRenderers.Remove(id);
            }
        }

        private void UpdateDeadCharacters()
        {
            var now = DateTime.Now;
            var deadCharacters = new List<ICharacter>();

            foreach (var character in _currentMapStateRepository.Characters.Where(x => x.RenderProperties.IsDead))
            {
                var actionTime = _characterStateCache.DeathStartTimes.SingleOrDefault(x => x.UniqueID == character.ID);
                if (actionTime == null)
                {
                    _characterStateCache.AddDeathStartTime(character.ID, DateTime.Now);
                }
                else if ((now - actionTime.ActionStartTime).TotalSeconds > 2)
                {
                    _characterStateCache.RemoveDeathStartTime(character.ID);
                    _characterStateCache.RemoveCharacterState(character.ID);

                    _characterRendererRepository.CharacterRenderers[character.ID].Dispose();
                    _characterRendererRepository.CharacterRenderers.Remove(character.ID);
                    deadCharacters.Add(character);
                }
            }

            _currentMapStateRepository.Characters.RemoveAll(deadCharacters.Contains);
        }

        private ICharacterRenderer InitializeRendererForCharacter(ICharacterRenderProperties renderProperties)
        {
            var renderer = _characterRendererFactory.CreateCharacterRenderer(renderProperties);
            renderer.Initialize();
            return renderer;
        }
    }

    public interface ICharacterRendererUpdater
    {
        void UpdateCharacters(GameTime gameTime);
    }
}