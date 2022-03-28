using EndlessClient.GameExecution;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Communication;
using Microsoft.Xna.Framework;
using System;

namespace EndlessClient.Network
{
    public class UnknownEntitiesRequester : GameComponent
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IPacketSendService _packetSendService;
        private DateTime _lastRequestTime;
        private const double REQUEST_INTERVAL_SECONDS = 1;

        public UnknownEntitiesRequester(IEndlessGameProvider gameProvider,
                                      ICurrentMapStateRepository currentMapStateRepository,
                                      IPacketSendService packetSendService)
            : base((Game) gameProvider.Game)
        {
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
                .AddChar((byte)_currentMapStateRepository.UnknownNPCIndexes.Count)
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
    }
}
