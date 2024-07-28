using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers.AdminInteract
{
    /// <summary>
    /// Response to $inventory <character> command.
    /// </summary>
    [AutoMappedType]
    public class AdminInteractList : InGameOnlyPacketHandler<AdminInteractListServerPacket>
    {
        private readonly IEnumerable<IUserInterfaceNotifier> _userInterfaceNotifiers;

        public override PacketFamily Family => PacketFamily.AdminInteract;

        public override PacketAction Action => PacketAction.List;

        public AdminInteractList(IPlayerInfoProvider playerInfoProvider,
                                 IEnumerable<IUserInterfaceNotifier> userInterfaceNotifiers)
            : base(playerInfoProvider)
        {
            _userInterfaceNotifiers = userInterfaceNotifiers;
        }

        public override bool HandlePacket(AdminInteractListServerPacket packet)
        {
            var inventory = packet.Inventory.Select(x => new InventoryItem(x.Id, x.Amount)).ToList();
            var bank = packet.Bank.Select(x => new InventoryItem(x.Id, x.Amount)).ToList();
            foreach (var notifier in _userInterfaceNotifiers)
            {
                notifier.NotifyCharacterInventory(packet.Name, packet.Usage, packet.GoldBank, inventory, bank);
            }

            return true;
        }
    }
}