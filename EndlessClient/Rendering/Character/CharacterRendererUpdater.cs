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
            var actual = _characterProvider.MainCharacter;
            var cached = _characterStateCache.MainCharacter;

            if (!cached.HasValue)
            {
                _characterStateCache.UpdateMainCharacterState(actual);

                var renderer = InitializeRendererForCharacter(_characterProvider.MainCharacter);
                _characterRendererRepository.MainCharacterRenderer = renderer;
                _characterRendererRepository.MainCharacterRenderer.SetToCenterScreenPosition();
            }
            else if (cached.Value != actual)
            {
                _characterRendererRepository.MainCharacterRenderer.Character = _characterProvider.MainCharacter;
                _characterStateCache.UpdateMainCharacterState(actual);
            }
        }

        private void CreateOtherCharacterRenderersAndCacheProperties()
        {
            foreach (var character in _currentMapStateRepository.Characters)
            {
                var id = character.ID;
                var cached = _characterStateCache.HasCharacterWithID(id)
                    ? new Optional<ICharacter>(_characterStateCache.OtherCharacters[id])
                    : Optional<ICharacter>.Empty;

                if (!cached.HasValue)
                {
                    _characterStateCache.UpdateCharacterState(id, character);

                    var renderer = InitializeRendererForCharacter(character);

                    if (_characterRendererRepository.CharacterRenderers.ContainsKey(id))
                    {
                        _characterRendererRepository.CharacterRenderers[id].Dispose();
                        _characterRendererRepository.CharacterRenderers.Remove(id);
                    }
                    _characterRendererRepository.CharacterRenderers.Add(id, renderer);
                }
                else if (cached.Value != character)
                {
                    _characterRendererRepository.CharacterRenderers[id].Character = character;
                    _characterStateCache.UpdateCharacterState(id, character);
                }

                if (_characterRendererRepository.NeedsWarpArriveAnimation.Contains(id))
                {
                    _characterRendererRepository.CharacterRenderers[id].ShowWarpArrive();
                    _characterRendererRepository.NeedsWarpArriveAnimation.Remove(id);
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
            foreach (var kvp in _characterStateCache.OtherCharacters)
            {
                if (_currentMapStateRepository.Characters.Any(x => x.ID == kvp.Key))
                    continue;
                staleIDs.Add(kvp.Key);
            }

            foreach (var id in staleIDs)
            {
                if (_characterRendererRepository.CharacterRenderers[id].EffectIsPlaying())
                {
                    _characterRendererRepository.CharacterRenderers[id].Visible = false;
                    continue;
                }

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

            _currentMapStateRepository.Characters.RemoveWhere(deadCharacters.Contains);
        }

        private ICharacterRenderer InitializeRendererForCharacter(ICharacter character)
        {
            var renderer = _characterRendererFactory.CreateCharacterRenderer(character);
            renderer.Initialize();
            return renderer;
        }
    }

    public interface ICharacterRendererUpdater
    {
        void UpdateCharacters(GameTime gameTime);
    }
}