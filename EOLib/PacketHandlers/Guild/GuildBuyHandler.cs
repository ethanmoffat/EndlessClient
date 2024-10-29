using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Guild;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional.Collections;

namespace EOLib.PacketHandlers.Guild
{
    [AutoMappedType]

    public class GuildBuyHandler : InGameOnlyPacketHandler<GuildBuyServerPacket>
    {
        private readonly IGuildSessionProvider _guildSessionProvider;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly IEnumerable<IGuildNotifier> _guildNotifiers;

        public override PacketFamily Family => PacketFamily.Guild;

        public override PacketAction Action => PacketAction.Buy;

        public GuildBuyHandler(IPlayerInfoProvider playerInfoProvider,
                               IGuildSessionProvider guildSessionProvider,
                               ICharacterInventoryRepository characterInventoryRepository,
                               IEnumerable<IGuildNotifier> guildNotifiers)
            : base(playerInfoProvider)
        {
            _guildSessionProvider = guildSessionProvider;
            _characterInventoryRepository = characterInventoryRepository;
            _guildNotifiers = guildNotifiers;
        }

        public override bool HandlePacket(GuildBuyServerPacket packet)
        {
            var oldAmount = _characterInventoryRepository.ItemInventory
                .SingleOrNone(x => x.ItemID == 1)
                .ValueOr(new InventoryItem(1, 0))
                .Amount;

            _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == 1);
            _characterInventoryRepository.ItemInventory.Add(new InventoryItem(1, packet.GoldAmount));

            var newBalance = _guildSessionProvider.GuildBalance + (oldAmount - packet.GoldAmount);

            foreach (var notifier in _guildNotifiers)
            {
                notifier.NotifyNewGuildBankBalance(newBalance);
            }

            return true;
        }
    }
}
