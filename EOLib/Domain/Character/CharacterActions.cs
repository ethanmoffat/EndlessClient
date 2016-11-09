// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Domain.Extensions;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Character
{
    public class CharacterActions : ICharacterActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly ICharacterProvider _characterProvider;

        public CharacterActions(IPacketSendService packetSendService,
                                ICharacterProvider characterProvider)
        {
            _packetSendService = packetSendService;
            _characterProvider = characterProvider;
        }

        public void Face(EODirection direction)
        {
            var packet = new PacketBuilder(PacketFamily.Face, PacketAction.Player)
                .AddChar((byte) direction)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void Walk()
        {
            var admin = _characterProvider.MainCharacter.NoWall &&
                        _characterProvider.MainCharacter.AdminLevel != AdminLevel.Player;
            var renderProperties = _characterProvider.MainCharacter.RenderProperties;

            var packet = new PacketBuilder(PacketFamily.Walk, admin ? PacketAction.Admin : PacketAction.Player)
                .AddChar((byte) renderProperties.Direction)
                .AddThree(DateTime.Now.ToEOTimeStamp())
                .AddChar((byte)renderProperties.GetDestinationX())
                .AddChar((byte)renderProperties.GetDestinationY())
                .Build();

            _packetSendService.SendPacket(packet);
        }
    }

    public interface ICharacterActions
    {
        void Face(EODirection direction);

        void Walk();
    }
}
