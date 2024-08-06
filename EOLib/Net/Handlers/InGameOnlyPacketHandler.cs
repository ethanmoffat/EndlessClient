using EOLib.Domain.Login;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.Net.Handlers
{
    public abstract class InGameOnlyPacketHandler<TPacket> : DefaultAsyncPacketHandler<TPacket>
        where TPacket : IPacket
    {
        private readonly IPlayerInfoProvider _playerInfoProvider;

        public override bool CanHandle => _playerInfoProvider.PlayerIsInGame;

        protected InGameOnlyPacketHandler(IPlayerInfoProvider playerInfoProvider)
        {
            _playerInfoProvider = playerInfoProvider;
        }
    }
}
