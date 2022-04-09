using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Chest
{
    [AutoMappedType]
    public class ChestAgreeHandler : InGameOnlyPacketHandler
    {
        private readonly IChestDataRepository _chestDataRepository;

        public override PacketFamily Family => PacketFamily.Chest;

        public override PacketAction Action => PacketAction.Agree;

        public ChestAgreeHandler(IPlayerInfoProvider playerInfoProvider,
                                 IChestDataRepository chestDataRepository)
            : base(playerInfoProvider)
        {
            _chestDataRepository = chestDataRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            _chestDataRepository.Items.Clear();

            int i = 0;
            while (packet.ReadPosition < packet.Length)
                _chestDataRepository.Items.Add(new ChestItem(packet.ReadShort(), packet.ReadThree(), i++));

            return true;
        }
    }
}
