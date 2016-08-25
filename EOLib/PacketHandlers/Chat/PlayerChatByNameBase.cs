// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Chat
{
    public abstract class PlayerChatByNameBase : IPacketHandler
    {
        private readonly IPlayerInfoProvider _playerInfoProvider;

        public PacketFamily Family { get { return PacketFamily.Talk; } }

        public abstract PacketAction Action { get; }

        public bool CanHandle { get { return _playerInfoProvider.PlayerIsInGame; } }

        protected PlayerChatByNameBase(IPlayerInfoProvider playerInfoProvider)
        {
            _playerInfoProvider = playerInfoProvider;
        }

        public bool HandlePacket(IPacket packet)
        {
            var name = packet.ReadBreakString();
            var message = packet.ReadBreakString();

            name = char.ToUpper(name[0]) + name.Substring(1).ToLower();
            PostChat(name, message);

            return true;
        }

        public async Task<bool> HandlePacketAsync(IPacket packet)
        {
            return await Task.Run(() => HandlePacket(packet));
        }

        protected abstract void PostChat(string name, string message);
    }
}
