using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Online;
using EOLib.IO;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    internal class PaperdollReplyHandler : InGameOnlyPacketHandler
    {
        private readonly IPaperdollRepository _paperdollRepository;

        public override PacketFamily Family => PacketFamily.PaperDoll;

        public override PacketAction Action => PacketAction.Reply;

        public PaperdollReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                     IPaperdollRepository paperdollRepository)
            : base(playerInfoProvider)
        {
            _paperdollRepository = paperdollRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var name = packet.ReadBreakString();
            var home = packet.ReadBreakString();
            var partner = packet.ReadBreakString();
            var title = packet.ReadBreakString();
            var guild = packet.ReadBreakString();
            var rank = packet.ReadBreakString();

            var playerID = packet.ReadShort();
            var clas = packet.ReadChar();
            var gender = packet.ReadChar();

            if (packet.ReadChar() != 0)
                return false;

            var paperdoll = new short[(int)EquipLocation.PAPERDOLL_MAX];
            for (int i = 0; i < (int)EquipLocation.PAPERDOLL_MAX; ++i)
                paperdoll[i] = packet.ReadShort();

            var iconType = (OnlineIcon)packet.ReadChar();

            var paperdollData = new PaperdollData()
                .WithName(name)
                .WithHome(home)
                .WithPartner(partner)
                .WithTitle(title)
                .WithGuild(guild)
                .WithRank(rank)
                .WithPlayerID(playerID)
                .WithClass(clas)
                .WithGender(gender)
                .WithPaperdoll(paperdoll)
                .WithIcon(iconType);

            _paperdollRepository.VisibleCharacterPaperdolls[playerID] = paperdollData;

            return true;
    }
    }
}
