using System;
using AutomaticTypeMapper;
using EOLib.Domain.Extensions;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Character
{
    [AutoMappedType]
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

        public void Attack()
        {
            var packet = new PacketBuilder(PacketFamily.Attack, PacketAction.Use)
                .AddChar((byte) _characterProvider.MainCharacter.RenderProperties.Direction)
                .AddThree(DateTime.Now.ToEOTimeStamp())
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void Sit()
        {
            var sitAction = _characterProvider.MainCharacter.RenderProperties.SitState == SitState.Standing
                ? SitAction.Sit
                : SitAction.Stand;

            var packet = new PacketBuilder(PacketFamily.Sit, PacketAction.Request)
                .AddChar((byte)sitAction)
                .Build();

            _packetSendService.SendPacket(packet);
        }
    }

    public interface ICharacterActions
    {
        void Face(EODirection direction);

        void Walk();

        void Attack();

        void Sit();
    }
}
