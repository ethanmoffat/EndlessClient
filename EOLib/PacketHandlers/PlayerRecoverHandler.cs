// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    public class PlayerRecoverHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;

        public override PacketFamily Family => PacketFamily.Recover;
        public override PacketAction Action => PacketAction.Player;

        public PlayerRecoverHandler(IPlayerInfoProvider playerInfoProvider,
                                    ICharacterRepository characterRepository)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var newHP = packet.ReadShort();
            var newTP = packet.ReadShort();

            var stats = _characterRepository.MainCharacter.Stats
                .WithNewStat(CharacterStat.HP, newHP)
                .WithNewStat(CharacterStat.TP, newTP);

            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            return true;
        }
    }
}
