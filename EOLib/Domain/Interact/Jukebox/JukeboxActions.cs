using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.IO;
using EOLib.IO.Repositories;
using EOLib.Net;
using EOLib.Net.Communication;
using Optional.Collections;
using System;

namespace EOLib.Domain.Interact.Jukebox
{
    [AutoMappedType]
    public class JukeboxActions : IJukeboxActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly ICharacterProvider _characterProvider;
        private readonly IEIFFileProvider _eifFileProvider;

        public JukeboxActions(IPacketSendService packetSendService,
                              ICharacterProvider characterProvider,
                              IEIFFileProvider eifFileProvider)
        {
            _packetSendService = packetSendService;
            _characterProvider = characterProvider;
            _eifFileProvider = eifFileProvider;
        }

        public void PlayNote(int noteIndex)
        {
            if (noteIndex < 0 || noteIndex >= 36)
                throw new ArgumentOutOfRangeException(nameof(noteIndex));

            var weapon = _characterProvider.MainCharacter.RenderProperties.WeaponGraphic;
            _eifFileProvider.EIFFile.SingleOrNone(x => x.Type == ItemType.Weapon && x.DollGraphic == weapon)
                .MatchSome(rec =>
                {
                    var packet = new PacketBuilder(PacketFamily.JukeBox, PacketAction.Use)
                        .AddChar(rec.DollGraphic) // todo: determine what GameServer expects; eoserv sends DollGraphic as a response in Character::PlayBard
                        .AddChar(noteIndex + 1)
                        .Build();

                    _packetSendService.SendPacket(packet);
                });
        }
    }

    public interface IJukeboxActions
    {
        void PlayNote(int noteIndex);
    }
}
