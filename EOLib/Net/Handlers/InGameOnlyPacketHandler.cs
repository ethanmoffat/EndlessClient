using EOLib.Domain.Login;

namespace EOLib.Net.Handlers
{
    public abstract class InGameOnlyPacketHandler : DefaultAsyncPacketHandler
    {
        private readonly IPlayerInfoProvider _playerInfoProvider;

        public override bool CanHandle => _playerInfoProvider.PlayerIsInGame;

        protected InGameOnlyPacketHandler(IPlayerInfoProvider playerInfoProvider)
        {
            _playerInfoProvider = playerInfoProvider;
        }
    }
}
