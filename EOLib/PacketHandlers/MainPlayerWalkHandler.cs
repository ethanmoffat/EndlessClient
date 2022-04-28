using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Linq;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class MainPlayerWalkHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public override PacketFamily Family => PacketFamily.Walk;
        public override PacketAction Action => PacketAction.Reply;

        public MainPlayerWalkHandler(IPlayerInfoProvider playerInfoProvider,
                                     ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            while (packet.PeekByte() != 0xFF)
            {
                var playerID = packet.ReadShort();
                if (!_currentMapStateRepository.Characters.ContainsKey(playerID))
                {
                    _currentMapStateRepository.UnknownPlayerIDs.Add(playerID);
                }
            }
            packet.ReadByte();

            while (packet.PeekByte() != 0xFF)
            {
                var index = packet.ReadChar();
                if (!_currentMapStateRepository.NPCs.Any((npc) => npc.Index == index))
                {
                    _currentMapStateRepository.UnknownNPCIndexes.Add(index);
                }
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

                var newItem = new MapItem(uid, itemID, x, y, amount);
                _currentMapStateRepository.MapItems.Add(newItem);
            }

            return true;
        }
    }
}
