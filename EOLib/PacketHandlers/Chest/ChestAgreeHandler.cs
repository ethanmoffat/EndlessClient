using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Chest
{
    [AutoMappedType]
    public class ChestAgreeHandler : InGameOnlyPacketHandler<ChestAgreeServerPacket>
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

        public override bool HandlePacket(ChestAgreeServerPacket packet)
        {
            _chestDataRepository.Items = new HashSet<ChestItem>(packet.Items.Select((x, i) => new ChestItem(x.Id, x.Amount, i)));
            return true;
        }
    }
}
