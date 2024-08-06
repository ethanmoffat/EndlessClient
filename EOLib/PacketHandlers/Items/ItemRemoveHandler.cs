using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Items
{
    /// <summary>
    /// Sent when an item is removed from the map by someone other than the main character
    /// </summary>
    [AutoMappedType]
    public class ItemRemoveHandler : InGameOnlyPacketHandler<ItemRemoveServerPacket>
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public override PacketFamily Family => PacketFamily.Item;

        public override PacketAction Action => PacketAction.Remove;

        public ItemRemoveHandler(IPlayerInfoProvider playerInfoProvider,
                                 ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
        }

        public override bool HandlePacket(ItemRemoveServerPacket packet)
        {
            if (_currentMapStateRepository.MapItems.TryGetValue(packet.ItemIndex, out var item))
                _currentMapStateRepository.MapItems.Remove(item);
            return true;
        }
    }
}
