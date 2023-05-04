using EndlessClient.GameExecution;
using EndlessClient.Rendering;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;
using EOLib.Net;
using EOLib.Net.Communication;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace EndlessClient.Network
{
    public class UnknownEntitiesRequester : GameComponent
    {
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
            // todo: the server should communicate the "seedistance" to clients
            // for now, disable auto remove of entities in Resizable mode
            if (_clientWindowSizeProvider.Resizable)
            {
                return;
            }

            var mc = _characterProvider.MainCharacter;

            var idsToRemove = new List<int>();
            foreach (var id in _currentMapStateRepository.Characters.Keys)
            {
                var c = _currentMapStateRepository.Characters[id];

                var xDiff = Math.Abs(mc.X - c.X);
                var yDiff = Math.Abs(mc.Y - c.Y);

                if (c.X < mc.X || c.Y < mc.Y)
                {
                    if (xDiff + yDiff > 11)
                        idsToRemove.Add(id);
                }
                else if (xDiff + yDiff > 14)
                {
                    idsToRemove.Add(id);
                }
            }

            foreach (var id in idsToRemove)
                _currentMapStateRepository.Characters.Remove(id);

            var npcsToRemove = new List<NPC>();
            foreach (var npc in _currentMapStateRepository.NPCs)
            {
                var xDiff = Math.Abs(mc.X - npc.X);
                var yDiff = Math.Abs(mc.Y - npc.Y);

                if (npc.X < mc.X || npc.Y < mc.Y)
                {
                    if (xDiff + yDiff > 11)
                        npcsToRemove.Add(npc);
                }
                else if (xDiff + yDiff > 14)
                {
                    npcsToRemove.Add(npc);
                }
            }

            _currentMapStateRepository.NPCs.RemoveWhere(npcsToRemove.Contains);
        }
    }
}
