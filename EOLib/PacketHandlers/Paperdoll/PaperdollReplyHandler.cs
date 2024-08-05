using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Paperdoll
{
    /// <summary>
    /// Sets paperdoll information for a given player
    /// </summary>
    [AutoMappedType]
    internal class PaperdollReplyHandler : InGameOnlyPacketHandler<PaperdollReplyServerPacket>
    {
        private readonly IPaperdollRepository _paperdollRepository;

        public override PacketFamily Family => PacketFamily.Paperdoll;

        public override PacketAction Action => PacketAction.Reply;

        public PaperdollReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                     IPaperdollRepository paperdollRepository)
            : base(playerInfoProvider)
        {
            _paperdollRepository = paperdollRepository;
        }

        public override bool HandlePacket(PaperdollReplyServerPacket packet)
        {
            var name = packet.Details.Name;
            var home = packet.Details.Home;
            var partner = packet.Details.Partner;
            var title = packet.Details.Title;
            var guild = packet.Details.Guild;
            var rank = packet.Details.GuildRank;

            var playerID = packet.Details.PlayerId;
            var clas = packet.Details.ClassId;
            var gender = packet.Details.Gender;

            var adminLevel = packet.Details.Admin;

            var paperdoll = packet.Equipment.GetPaperdoll();

            var iconType = packet.Icon;

            var paperdollData = _paperdollRepository.VisibleCharacterPaperdolls.ContainsKey(playerID)
                ? _paperdollRepository.VisibleCharacterPaperdolls[playerID]
                : new PaperdollData();

            paperdollData = paperdollData
                .WithName(name)
                .WithHome(home)
                .WithPartner(partner)
                .WithTitle(title)
                .WithGuild(guild)
                .WithRank(rank)
                .WithPlayerID(playerID)
                .WithClass(clas)
                .WithGender((int)gender)
                .WithAdminLevel(adminLevel)
                .WithPaperdoll(paperdoll)
                .WithIcon(iconType);

            _paperdollRepository.VisibleCharacterPaperdolls[playerID] = paperdollData;

            return true;
        }
    }
}