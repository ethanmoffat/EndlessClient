using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Recover
{
    /// <summary>
    /// Sent when the main character's stats are updated by a quest (or stat change command)
    /// </summary>
    [AutoMappedType]
    public class RecoverListHandler : InGameOnlyPacketHandler<RecoverListServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;

        public override PacketFamily Family => PacketFamily.Recover;
        public override PacketAction Action => PacketAction.List;

        public RecoverListHandler(IPlayerInfoProvider playerInfoProvider,
                                  ICharacterRepository characterRepository)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
        }

        public override bool HandlePacket(RecoverListServerPacket packet)
        {
            var stats = _characterRepository.MainCharacter.Stats
                .Merge(CharacterStats.FromStatUpdate(packet.Stats));

            _characterRepository.MainCharacter = _characterRepository.MainCharacter
                .WithClassID(packet.ClassId)
                .WithStats(stats);

            return true;
        }
    }
}
