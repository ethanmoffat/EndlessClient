// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Chat
{
    public class MuteHandler : IPacketHandler
    {
        private readonly IPlayerInfoProvider _playerInfoProvider;
        private readonly ChatEventManager _chatEventManager;

        public PacketFamily Family { get { return PacketFamily.Talk; } }

        public PacketAction Action { get { return PacketAction.Spec; } }

        public bool CanHandle { get { return _playerInfoProvider.PlayerIsInGame; } }

        public MuteHandler(IPlayerInfoProvider playerInfoProvider,
                           ChatEventManager chatEventManager)
        {
            _playerInfoProvider = playerInfoProvider;
            _chatEventManager = chatEventManager;
        }

        public bool HandlePacket(IPacket packet)
        {
            var adminName = packet.ReadEndString();
            adminName = char.ToUpper(adminName[0]) + adminName.Substring(1).ToLower();

            _chatEventManager.FirePlayerMuted(adminName);

            return true;
        }

        public async Task<bool> HandlePacketAsync(IPacket packet)
        {
            return await Task.Run(() => HandlePacket(packet));
        }
    }
}
