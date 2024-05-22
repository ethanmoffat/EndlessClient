using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.StatSkill
{
    /// <summary>
    /// Sent when the main character spends their stat points
    /// </summary>
    [AutoMappedType]
    public class StatskillPlayerHandler : InGameOnlyPacketHandler<StatSkillPlayerServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;

        public override PacketFamily Family => PacketFamily.StatSkill;
        public override PacketAction Action => PacketAction.Player;

        public StatskillPlayerHandler(IPlayerInfoProvider playerInfoProvider,
                                      ICharacterRepository characterRepository)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
        }

        public override bool HandlePacket(StatSkillPlayerServerPacket packet)
        {
            var stats = _characterRepository.MainCharacter.Stats
                .WithNewStat(CharacterStat.StatPoints, packet.StatPoints)
                .Merge(CharacterStats.FromStatUpdate(packet.Stats));

            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            return true;
        }
    }
}
