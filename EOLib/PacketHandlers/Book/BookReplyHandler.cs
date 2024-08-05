using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Paperdoll
{
    /// <summary>
    /// Sets book information for a given player
    /// </summary>
    [AutoMappedType]
    internal class BookReplyHandler : InGameOnlyPacketHandler<BookReplyServerPacket>
    {
        private readonly IPaperdollRepository _paperdollRepository;

        public override PacketFamily Family => PacketFamily.Book;

        public override PacketAction Action => PacketAction.Reply;

        public BookReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                IPaperdollRepository paperdollRepository)
            : base(playerInfoProvider)
        {
            _paperdollRepository = paperdollRepository;
        }

        public override bool HandlePacket(BookReplyServerPacket packet)
        {
            var paperdollData = _paperdollRepository.VisibleCharacterPaperdolls.ContainsKey(packet.Details.PlayerId)
                ? _paperdollRepository.VisibleCharacterPaperdolls[packet.Details.PlayerId]
                : new PaperdollData();

            paperdollData = paperdollData
                .WithName(packet.Details.Name)
                .WithHome(packet.Details.Home)
                .WithPartner(packet.Details.Partner)
                .WithTitle(packet.Details.Title)
                .WithGuild(packet.Details.Guild)
                .WithRank(packet.Details.GuildRank)
                .WithPlayerID(packet.Details.PlayerId)
                .WithClass(packet.Details.ClassId)
                .WithGender((int)packet.Details.Gender)
                .WithAdminLevel(packet.Details.Admin)
                .WithIcon(packet.Icon)
                .WithQuestNames(packet.QuestNames);

            _paperdollRepository.VisibleCharacterPaperdolls[packet.Details.PlayerId] = paperdollData;

            return true;
        }
    }
}