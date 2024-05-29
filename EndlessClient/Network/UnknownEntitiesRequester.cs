using EndlessClient.GameExecution;
using EndlessClient.Rendering;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.NPC;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EndlessClient.Network
{
    public class UnknownEntitiesRequester : GameComponent
    {
        private const int UPPER_SEE_DISTANCE = 11;
        private const int LOWER_SEE_DISTANCE = 14;

        private const double REQUEST_INTERVAL_SECONDS = 1.0;

        private readonly IClientWindowSizeProvider _clientWindowSizeProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly INPCRendererProvider _npcRendererProvider;
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly IUnknownEntitiesRequestActions _unknownEntitiesRequestActions;

        private DateTime _lastRequestTime;
        

        public UnknownEntitiesRequester(IEndlessGameProvider gameProvider,
                                        IClientWindowSizeProvider clientWindowSizeProvider,
                                        ICharacterProvider characterProvider,
                                        ICurrentMapStateRepository currentMapStateRepository,
                                        INPCRendererProvider npcRendererProvider,
                                        ICharacterRendererProvider characterRendererProvider,
                                        IUnknownEntitiesRequestActions unknownEntitiesRequestActions)
            : base((Game) gameProvider.Game)
        {
            _clientWindowSizeProvider = clientWindowSizeProvider;
            _characterProvider = characterProvider;
            _currentMapStateRepository = currentMapStateRepository;
            _npcRendererProvider = npcRendererProvider;
            _characterRendererProvider = characterRendererProvider;
            _unknownEntitiesRequestActions = unknownEntitiesRequestActions;
            _lastRequestTime = DateTime.Now;
        }

        public override void Update(GameTime gameTime)
        {
            if ((DateTime.Now - _lastRequestTime).TotalSeconds >= REQUEST_INTERVAL_SECONDS)
            {
                if (_currentMapStateRepository.UnknownNPCIndexes.Count > 0 && _currentMapStateRepository.UnknownPlayerIDs.Count > 0)
                {
                    _unknownEntitiesRequestActions.RequestAll();
                }
                else if (_currentMapStateRepository.UnknownNPCIndexes.Count > 0)
                {
                    _unknownEntitiesRequestActions.RequestUnknownNPCs();
                }
                else if (_currentMapStateRepository.UnknownPlayerIDs.Count > 0)
                {
                    _unknownEntitiesRequestActions.RequestUnknownPlayers();
                }

                ClearOutOfRangeActors();
            }

            base.Update(gameTime);
        }

        private void ClearOutOfRangeActors()
        {
            var mc = _characterProvider.MainCharacter;

            var entities = new List<IMapEntity>(_currentMapStateRepository.Characters)
                .Concat(_currentMapStateRepository.NPCs)
                .Concat(_currentMapStateRepository.MapItems);

            var seeDistanceUpper = (int)(_clientWindowSizeProvider.Height / 480.0 * UPPER_SEE_DISTANCE);
            var seeDistanceLower = (int)(_clientWindowSizeProvider.Height / 480.0 * LOWER_SEE_DISTANCE);

            var entitiesToRemove = new List<IMapEntity>();
            foreach (var entity in entities)
            {
                var xDiff = Math.Abs(mc.X - entity.X);
                var yDiff = Math.Abs(mc.Y - entity.Y);

                if (entity.X < mc.X || entity.Y < mc.Y)
                {
                    if (xDiff + yDiff > seeDistanceUpper)
                        entitiesToRemove.Add(entity);
                }
                else if (xDiff + yDiff > seeDistanceLower)
                {
                    entitiesToRemove.Add(entity);
                }
            }

            foreach (var entity in entitiesToRemove)
            {
                if (entity is Character c && _characterRendererProvider.CharacterRenderers.ContainsKey(c.ID))
                    _currentMapStateRepository.Characters.Remove(c);
                else if (entity is NPC n && _npcRendererProvider.NPCRenderers.ContainsKey(n.Index))
                    _currentMapStateRepository.NPCs.Remove(n);
                else if (entity is MapItem i)
                    _currentMapStateRepository.MapItems.Remove(i);
            }
        }
    }
}
