// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Chat.Commands
{
    public class FindCommand : IPlayerCommand
    {
        private readonly IPacketSendService _packetSendService;
        public string CommandText { get { return "find"; } }

        public FindCommand(IPacketSendService packetSendService)
        {
            _packetSendService = packetSendService;
        }

        public bool Execute(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
                return false;

            var packet = new PacketBuilder(PacketFamily.Players, PacketAction.Accept)
                .AddString(parameter)
                .Build();

            _packetSendService.SendPacketAsync(packet);
            return true;
        }
    }
}