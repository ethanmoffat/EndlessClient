using AutomaticTypeMapper;
using EndlessClient.Rendering.Factories;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework;
using Optional;
using Optional.Collections;
using System.Collections.Generic;
using System.Linq;

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

            cached.Match(
                some: c =>
                {
                    if (c != actual)
                    {
                        _characterRendererRepository.MainCharacterRenderer.MatchSome(r => r.Character = _characterProvider.MainCharacter);
                        _characterStateCache.UpdateMainCharacterState(actual);
                    }
                },
                none: () =>
                {
                    _characterStateCache.UpdateMainCharacterState(actual);

                    var renderer = InitializeRendererForCharacter(_characterProvider.MainCharacter);
                    renderer.SetToCenterScreenPosition();
                    _characterRendererRepository.MainCharacterRenderer = Option.Some(renderer);
                });
        }

        private void CreateOtherCharacterRenderersAndCacheProperties()
        {
            foreach (var id in _currentMapStateRepository.Characters.Keys)
            {
                var character = _currentMapStateRepository.Characters[id];

                _characterStateCache.HasCharacterWithID(id)
                    .SomeWhen(b => b)
                    .Map(_ => _characterStateCache.OtherCharacters[id])
                    .Match(
                        some: cached =>
                        {
                            if (cached != character)
                            {
                                if (_characterRendererRepository.CharacterRenderers.ContainsKey(id))
                                    _characterRendererRepository.CharacterRenderers[id].Character = character;
                                _characterStateCache.UpdateCharacterState(id, character);
                            }
                        },
                        none: () =>
                        {
                            _characterStateCache.UpdateCharacterState(id, character);

                            var renderer = InitializeRendererForCharacter(character);

                            if (_characterRendererRepository.CharacterRenderers.ContainsKey(id))
                            {
                                _characterRendererRepository.CharacterRenderers[id].Dispose();
                                _characterRendererRepository.CharacterRenderers.Remove(id);
                            }
                            _characterRendererRepository.CharacterRenderers.Add(id, renderer);
                        });

                if (_characterRendererRepository.NeedsWarpArriveAnimation.Contains(id) &&
                    _characterRendererRepository.CharacterRenderers.ContainsKey(id))
                {
                    _characterRendererRepository.CharacterRenderers[id].ShowWarpArrive();
                    _characterRendererRepository.NeedsWarpArriveAnimation.Remove(id);
                }
            }
        }

        private void UpdateAllCharacters(GameTime gameTime)
        {
            _characterRendererRepository.MainCharacterRenderer.MatchSome(x => x.Update(gameTime));
            foreach (var renderer in _characterRendererRepository.CharacterRenderers.Values)
                renderer.Update(gameTime);
        }

        private void RemoveStaleCharacters()
        {
            var staleIDs = _characterStateCache.OtherCharacters.Keys
                .Where(x => !_currentMapStateRepository.Characters.ContainsKey(x));

            foreach (var id in staleIDs)
            {
                _characterStateCache.RemoveCharacterState(id);

                if (_characterRendererRepository.CharacterRenderers.ContainsKey(id))
                {
                    if (_characterRendererRepository.CharacterRenderers[id].EffectIsPlaying())
                    {
                        _characterRendererRepository.CharacterRenderers[id].Visible = false;
                        continue;
                    }

                    _characterRendererRepository.CharacterRenderers[id].Dispose();
                    _characterRendererRepository.CharacterRenderers.Remove(id);
                }
            }
        }

        private void UpdateDeadCharacters()
        {
            var deadCharacters = new List<int>();

            foreach (var character in _currentMapStateRepository.Characters.Values.Where(x => x.RenderProperties.IsDead))
            {
                _characterStateCache.DeathStartTimes.SingleOrNone(x => x.UniqueID == character.ID)
                    .Match(
                        none: () => _characterStateCache.AddDeathStartTime(character.ID),
                        some: actionTime =>
                        {
                            if (actionTime.ActionTimer.ElapsedMilliseconds >= 2)
                            {
                                _characterStateCache.RemoveDeathStartTime(character.ID);
                                _characterStateCache.RemoveCharacterState(character.ID);

                                if (_characterRendererRepository.CharacterRenderers.ContainsKey(character.ID))
                                {
                                    _characterRendererRepository.CharacterRenderers[character.ID].Dispose();
                                    _characterRendererRepository.CharacterRenderers.Remove(character.ID);
                                }

                                deadCharacters.Add(character.ID);
                            }
                        });
            }

            foreach (var id in deadCharacters)
                _currentMapStateRepository.Characters.Remove(id);
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