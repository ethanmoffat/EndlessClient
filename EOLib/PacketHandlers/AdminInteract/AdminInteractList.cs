using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.AdminInteract
{
    /// <summary>
    /// Response to $inventory <character> command.
    /// </summary>
    [AutoMappedType]
    public class AdminInteractList: InGameOnlyPacketHandler
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

        public override bool HandlePacket(IPacket packet)
        {
            var name = packet.ReadBreakString();
            
            var usage = packet.ReadInt();
            if (packet.ReadByte() != 255)
                return false;

            var gold_bank = packet.ReadInt();
            if (packet.ReadByte() != 255)
                return false;

            var inventory = new List<InventoryItem>();
            while (packet.PeekByte() != 255)
            {
                var id = packet.ReadShort();
                var amount = packet.ReadInt();
                inventory.Add(new InventoryItem(id, amount));
            }
            packet.ReadByte();

            var bank = new List<InventoryItem>();
            while (packet.ReadPosition < packet.Length)
            {
                var id = packet.ReadShort();
                var amount = packet.ReadThree();
                bank.Add(new InventoryItem(id, amount));
            }

            foreach (var notifier in _userInterfaceNotifiers)
            {
                notifier.NotifyCharacterInventory(name, usage, gold_bank, inventory, bank);
            }

            return true;
        }
    }
}
