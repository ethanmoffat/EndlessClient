﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EndlessClient.GameExecution;
using EndlessClient.Rendering;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.NPC;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;

namespace EndlessClient.Network
{
    public class UnknownEntitiesRequester : GameComponent
    {
        private const int UPPER_SEE_DISTANCE = 12;
        private const int LOWER_SEE_DISTANCE = 15;

        private const int REQUEST_INTERVAL_MS = 1000;

        private readonly IClientWindowSizeProvider _clientWindowSizeProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly INPCRendererProvider _npcRendererProvider;
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly IUnknownEntitiesRequestActions _unknownEntitiesRequestActions;

        private readonly Stopwatch _requestTimer;

        public UnknownEntitiesRequester(IEndlessGameProvider gameProvider,
                                        IClientWindowSizeProvider clientWindowSizeProvider,
                                        ICharacterProvider characterProvider,
                                        ICurrentMapStateRepository currentMapStateRepository,
                                        INPCRendererProvider npcRendererProvider,
                                        ICharacterRendererProvider characterRendererProvider,
                                        IUnknownEntitiesRequestActions unknownEntitiesRequestActions)
            : base((Game)gameProvider.Game)
        {
            _clientWindowSizeProvider = clientWindowSizeProvider;
            _characterProvider = characterProvider;
            _currentMapStateRepository = currentMapStateRepository;
            _npcRendererProvider = npcRendererProvider;
            _characterRendererProvider = characterRendererProvider;
            _unknownEntitiesRequestActions = unknownEntitiesRequestActions;
            _requestTimer = Stopwatch.StartNew();
        }

        public override void Update(GameTime gameTime)
        {
            if (_requestTimer.ElapsedMilliseconds >= REQUEST_INTERVAL_MS)
            {
                ClearOutOfRangeActors();

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

                _requestTimer.Restart();
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
