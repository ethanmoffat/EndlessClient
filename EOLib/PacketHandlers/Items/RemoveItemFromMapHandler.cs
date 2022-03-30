using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Items
{
    [AutoMappedType]
    public class RemoveItemFromMapHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public override PacketFamily Family => PacketFamily.Item;

        public override PacketAction Action => PacketAction.Remove;

        public RemoveItemFromMapHandler(IPlayerInfoProvider playerInfoProvider,
                                        ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var uid = packet.ReadShort();
            _currentMapStateRepository.MapItems.RemoveWhere(x => x.UniqueID == uid);
            return true;
        }
    }
}
