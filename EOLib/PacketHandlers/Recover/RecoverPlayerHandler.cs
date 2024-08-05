using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Recover
{
    /// <summary>
    /// Sent when the main character gains HP/TP
    /// </summary>
    [AutoMappedType]
    public class RecoverPlayerHandler : InGameOnlyPacketHandler<RecoverPlayerServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;

        public override PacketFamily Family => PacketFamily.Recover;
        public override PacketAction Action => PacketAction.Player;

        public RecoverPlayerHandler(IPlayerInfoProvider playerInfoProvider,
                                    ICharacterRepository characterRepository)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
        }

        public override bool HandlePacket(RecoverPlayerServerPacket packet)
        {
            var stats = _characterRepository.MainCharacter.Stats
                .WithNewStat(CharacterStat.HP, packet.Hp)
                .WithNewStat(CharacterStat.TP, packet.Tp);

            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            return true;
        }
    }
}