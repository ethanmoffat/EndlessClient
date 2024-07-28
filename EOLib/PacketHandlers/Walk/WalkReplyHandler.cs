using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Linq;

namespace EOLib.PacketHandlers.Walk
{
    /// <summary>
    /// Sent in response to the main character walking successfully
    /// </summary>
    [AutoMappedType]
    public class WalkReplyHandler : InGameOnlyPacketHandler<WalkReplyServerPacket>
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public override PacketFamily Family => PacketFamily.Walk;
        public override PacketAction Action => PacketAction.Reply;

        public WalkReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
        }

        public override bool HandlePacket(WalkReplyServerPacket packet)
        {
            foreach (var unknownPlayer in packet.PlayerIds.Where(x => !_currentMapStateRepository.Characters.ContainsKey(x)))
                _currentMapStateRepository.UnknownPlayerIDs.Add(unknownPlayer);
            foreach (var unknownNpc in packet.NpcIndexes.Where(x => !_currentMapStateRepository.NPCs.ContainsKey(x)))
                _currentMapStateRepository.UnknownNPCIndexes.Add(unknownNpc);

            foreach (var item in packet.Items)
            {
                _currentMapStateRepository.MapItems.Add(new MapItem.Builder
                {
                    UniqueID = item.Id,
                    ItemID = item.Id,
                    X = item.Coords.X,
                    Y = item.Coords.Y,
                    Amount = item.Amount,
                }.ToImmutable());
            }

            return true;
        }
    }
}