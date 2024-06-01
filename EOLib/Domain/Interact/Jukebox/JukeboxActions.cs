using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.IO;
using EOLib.IO.Repositories;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
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
                    // todo: determine what GameServer expects; eoserv sends DollGraphic as a response in Character::PlayBard
                    var packet = new JukeboxUseClientPacket
                    {
                        InstrumentId = rec.DollGraphic,
                        NoteId = noteIndex + 1,
                    };
                    _packetSendService.SendPacket(packet);
                });
        }

        public void RequestSong(int songIndex)
        {
            var packet = new JukeboxMsgClientPacket { TrackId = songIndex };
            _packetSendService.SendPacket(packet);
        }
    }

    public interface IJukeboxActions
    {
        void PlayNote(int noteIndex);

        void RequestSong(int songIndex);
    }
}
