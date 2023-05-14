using EndlessClient.GameExecution;
using EndlessClient.Rendering;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;
using EOLib.IO.Map;
using EOLib.Net;
using EOLib.Net.Communication;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

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
        private readonly IPacketSendService _packetSendService;

        private DateTime _lastRequestTime;
        

        // todo: create actions in EOLib.Domain for requesting unknown entities, instead of using packetsendservice directly
        public UnknownEntitiesRequester(IEndlessGameProvider gameProvider,
                                        IClientWindowSizeProvider clientWindowSizeProvider,
                                        ICharacterProvider characterProvider,
                                        ICurrentMapStateRepository currentMapStateRepository,
                                        IPacketSendService packetSendService)
            : base((Game) gameProvider.Game)
        {
            _clientWindowSizeProvider = clientWindowSizeProvider;
            _characterProvider = characterProvider;
            _currentMapStateRepository = currentMapStateRepository;
            _packetSendService = packetSendService;
            _lastRequestTime = DateTime.Now;
        }

        public override void Update(GameTime gameTime)
        {
            if ((DateTime.Now - _lastRequestTime).TotalSeconds >= REQUEST_INTERVAL_SECONDS)
            {
                IPacket request = null;
                if (_currentMapStateRepository.UnknownNPCIndexes.Count > 0 && _currentMapStateRepository.UnknownPlayerIDs.Count > 0)
                {
                    request = CreateRequestForBoth();
                }
                else if (_currentMapStateRepository.UnknownNPCIndexes.Count > 0)
                {
                    request = CreateRequestForNPCs();
                }
                else if (_currentMapStateRepository.UnknownPlayerIDs.Count > 0)
                {
                    request = CreateRequestForPlayers();
                }

                try
                {
                    if (request != null)
                    {
                        _packetSendService.SendPacket(request);
                        _currentMapStateRepository.UnknownNPCIndexes.Clear();
                        _currentMapStateRepository.UnknownPlayerIDs.Clear();
                    }
                }
                catch (NoDataSentException)
                { } // Swallow error. Will try again on next interval
                finally
                {
                    _lastRequestTime = DateTime.Now;
                }

                ClearOutOfRangeActors();
            }

            base.Update(gameTime);
        }

        private IPacket CreateRequestForBoth()
        {
            IPacketBuilder builder = new PacketBuilder(PacketFamily.MapInfo, PacketAction.Request);
            foreach (var id in _currentMapStateRepository.UnknownPlayerIDs)
            {
                builder = builder.AddShort(id);
            }
            builder = builder.AddByte(0xFF);
            foreach (var index in _currentMapStateRepository.UnknownNPCIndexes)
            {
                builder = builder.AddChar(index);
            }

            return builder.Build();
        }

        private IPacket CreateRequestForNPCs()
        {
            IPacketBuilder builder = new PacketBuilder(PacketFamily.NPCMapInfo, PacketAction.Request)
                .AddChar(_currentMapStateRepository.UnknownNPCIndexes.Count)
                .AddByte(0xFF);

            foreach (var index in _currentMapStateRepository.UnknownNPCIndexes)
            {
                builder = builder.AddChar(index);
            }
            return builder.Build();
        }

        private IPacket CreateRequestForPlayers()
        {
            IPacketBuilder builder = new PacketBuilder(PacketFamily.CharacterMapInfo, PacketAction.Request);
            foreach (var id in _currentMapStateRepository.UnknownPlayerIDs)
            {
                builder = builder.AddShort(id);
            }
            return builder.Build();
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
                if (entity is Character c)
                    _currentMapStateRepository.Characters.Remove(c);
                else if (entity is NPC n)
                    _currentMapStateRepository.NPCs.Remove(n);
                else if (entity is MapItem i)
                    _currentMapStateRepository.MapItems.Remove(i);
            }
        }
    }
}
