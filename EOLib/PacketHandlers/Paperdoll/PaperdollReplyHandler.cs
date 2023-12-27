using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Online;
using EOLib.IO;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Paperdoll
{
    /// <summary>
    /// Sets paperdoll information for a given player
    /// </summary>
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

            var adminLevel = packet.ReadChar();

            var paperdoll = new Dictionary<EquipLocation, int>((int)EquipLocation.PAPERDOLL_MAX);
            for (var loc = (EquipLocation)0; loc < EquipLocation.PAPERDOLL_MAX; ++loc)
                paperdoll[loc] = packet.ReadShort();

            var iconType = (OnlineIcon)packet.ReadChar();


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
                .WithGender(gender)
                .WithAdminLevel((AdminLevel)adminLevel)
                .WithPaperdoll(paperdoll)
                .WithIcon(iconType);

            _paperdollRepository.VisibleCharacterPaperdolls[playerID] = paperdollData;

            return true;
        }
    }
}
