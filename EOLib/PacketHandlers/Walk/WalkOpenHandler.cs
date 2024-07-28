using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Walk
{
	[AutoMappedType]
    public class WalkOpenHandler : InGameOnlyPacketHandler<WalkOpenServerPacket>
	{
		private readonly ICurrentMapStateRepository _currentMapStateRepository;

		private readonly IPlayerInfoProvider _playerInfoProvider;

		public override PacketFamily Family => PacketFamily.Walk;

		public override PacketAction Action => PacketAction.Open;

        public WalkOpenHandler(IPlayerInfoProvider playerInfoProvider,
								 ICurrentMapStateRepository currentMapStateRepository) : base(playerInfoProvider)
		{
            _playerInfoProvider = playerInfoProvider;
        }

		public override bool HandlePacket(WalkOpenServerPacket packet)
		{
            _playerInfoProvider.IsPlayerFrozen = false;
            return true;
		}
	}
}
