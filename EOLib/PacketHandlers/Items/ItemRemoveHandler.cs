﻿using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Items
{
    /// <summary>
    /// Sent when an item is removed from the map by someone other than the main character
    /// </summary>
    [AutoMappedType]
    public class ItemRemoveHandler : InGameOnlyPacketHandler
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

        public override bool HandlePacket(IPacket packet)
        {
            var uid = packet.ReadShort();
            if (_currentMapStateRepository.MapItems.ContainsKey(uid))
            {
                _currentMapStateRepository.MapItems.Remove(_currentMapStateRepository.MapItems[uid]);
            }
            return true;
        }
    }
}
