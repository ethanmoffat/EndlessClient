using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Online;
using EOLib.Net;
using EOLib.Net.Handlers;
using System;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Paperdoll
{
    /// <summary>
    /// Sets book information for a given player
    /// </summary>
    [AutoMappedType]
    internal class BookReplyHandler : InGameOnlyPacketHandler
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

            var iconType = (OnlineIcon)packet.ReadChar();

            if (packet.ReadByte() != 255)
                return false;

            var questNames = new List<string>();
            while (packet.ReadPosition < packet.Length)
                questNames.Add(packet.ReadBreakString());

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
                .WithIcon(iconType)
                .WithQuestNames(questNames);

            _paperdollRepository.VisibleCharacterPaperdolls[playerID] = paperdollData;

            return true;
        }
    }
}
