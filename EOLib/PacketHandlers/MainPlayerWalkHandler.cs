using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Communication;
using EOLib.Net.Handlers;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class MainPlayerWalkHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IPacketSendService _packetSendService;

        public override PacketFamily Family => PacketFamily.Walk;
        public override PacketAction Action => PacketAction.Reply;

        public MainPlayerWalkHandler(IPlayerInfoProvider playerInfoProvider,
                                     ICurrentMapStateRepository currentMapStateRepository,
                                     IPacketSendService packetSendService)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _packetSendService = packetSendService;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var playerIDs = new List<short>();
            while (packet.PeekByte() != 0xFF)
            {
                playerIDs.Add(packet.ReadShort());
            }
            packet.ReadByte();

            var npcIndexes = new List<byte>();
            while (packet.PeekByte() != 0xFF)
            {
                npcIndexes.Add(packet.ReadChar());
            }
            packet.ReadByte();

            var numberOfMapItems = packet.PeekEndString().Length / 9;
            for (int i = 0; i < numberOfMapItems; ++i)
            {
                var uid = packet.ReadShort();
                var itemID = packet.ReadShort();
                var x = packet.ReadChar();
                var y = packet.ReadChar();
                var amount = packet.ReadThree();

                var newItem = new Item(uid, itemID, x, y).WithAmount(amount);
                _currentMapStateRepository.MapItems.Add(newItem);
            }

            var newPlayerIDs = playerIDs
                .Where(id => !_currentMapStateRepository.Characters.ContainsKey(id))
                .ToList();

            var newNPCIndxes = npcIndexes
                .Where(index => !_currentMapStateRepository.NPCs.Any((npc) => npc.Index == index))
                .ToList();

            if (newPlayerIDs.Count > 0 || newNPCIndxes.Count > 0)
            {
                IPacketBuilder builder = new PacketBuilder(PacketFamily.MapInfo, PacketAction.Request);

                if (newPlayerIDs.Count > 0)
                {
                    foreach (var playerId in newPlayerIDs)
                    {
                        builder = builder.AddShort(playerId);
                    }
                }

                if (newNPCIndxes.Count > 0)
                {
                    builder.AddByte(0xFF);
                    foreach (var npcIndex in newNPCIndxes)
                    {
                        builder = builder.AddChar(npcIndex);
                    }
                }

                try
                {
                    var request = builder.Build();
                    _packetSendService.SendPacket(request);
                }
                catch (NoDataSentException)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
