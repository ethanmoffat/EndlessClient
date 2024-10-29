using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Guild;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Extensions;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;

namespace EOLib.PacketHandlers.Guild
{
    [AutoMappedType]

    public class GuildCreateHandler : InGameOnlyPacketHandler<GuildCreateServerPacket>
    {
        private const byte JoinGuildSfx = 18;

        private readonly ICharacterRepository _characterRepository;
        private readonly IGuildSessionRepository _guildSessionRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly IEnumerable<ISoundNotifier> _soundNotifiers;

        public override PacketFamily Family => PacketFamily.Guild;

        public override PacketAction Action => PacketAction.Create;

        public GuildCreateHandler(IPlayerInfoProvider playerInfoProvider,
                                 ICharacterRepository characterRepository,
                                 IGuildSessionRepository guildSessionRepository,
                                 ICharacterInventoryRepository characterInventoryRepository,
                                 IEnumerable<ISoundNotifier> soundNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _guildSessionRepository = guildSessionRepository;
            _characterInventoryRepository = characterInventoryRepository;
            _soundNotifiers = soundNotifiers;
        }

        public override bool HandlePacket(GuildCreateServerPacket packet)
        {
            var gold = new InventoryItem(1, packet.GoldAmount);
            _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == 1);
            _characterInventoryRepository.ItemInventory.Add(gold);

            _characterRepository.MainCharacter = _characterRepository.MainCharacter
                .WithGuildTag(packet.GuildTag.ToUpper())
                .WithGuildName(packet.GuildName.Capitalize())
                .WithGuildRank(packet.RankName.Capitalize());

            _guildSessionRepository.CreationSession = Option.None<GuildCreationSession>();

            foreach (var notifier in _soundNotifiers)
                notifier.NotifySoundEffect(JoinGuildSfx);

            return true;
        }
    }
}
