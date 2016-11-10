// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Login;

namespace EOLib.Net.Handlers
{
    public abstract class InGameOnlyPacketHandler : DefaultAsyncPacketHandler
    {
        private readonly IPlayerInfoProvider _playerInfoProvider;

        public override bool CanHandle { get { return _playerInfoProvider.PlayerIsInGame; } }

        protected InGameOnlyPacketHandler(IPlayerInfoProvider playerInfoProvider)
        {
            _playerInfoProvider = playerInfoProvider;
        }
    }
}
