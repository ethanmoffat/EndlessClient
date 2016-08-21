// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using System.Threading.Tasks;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    public abstract class PlayerChatByIDHandler : IPacketHandler
    {
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly IPlayerInfoProvider _playerInfoProvider;

        public PacketFamily Family { get { return PacketFamily.Talk; } }

        public abstract PacketAction Action { get; }

        public bool CanHandle { get { return _playerInfoProvider.PlayerIsInGame; } }

        protected PlayerChatByIDHandler(ICurrentMapStateProvider currentMapStateProvider,
                                        IPlayerInfoProvider playerInfoProvider)
        {
            _currentMapStateProvider = currentMapStateProvider;
            _playerInfoProvider = playerInfoProvider;
        }

        public bool HandlePacket(IPacket packet)
        {
            var fromPlayerID = packet.ReadShort();
            if (_currentMapStateProvider.Characters.All(x => x.ID != fromPlayerID))
                return true;

            DoTalk(packet, _currentMapStateProvider.Characters.Single(x => x.ID == fromPlayerID));

            return true;
        }

        public async Task<bool> HandlePacketAsync(IPacket packet)
        {
            return await Task.Run(() => HandlePacket(packet));
        }

        protected abstract void DoTalk(IPacket packet, ICharacter character);
    }
}
