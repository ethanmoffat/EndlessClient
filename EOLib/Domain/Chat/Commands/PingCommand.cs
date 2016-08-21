// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Domain.Protocol;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Chat.Commands
{
    public class PingCommand : IPlayerCommand
    {
        private readonly IPacketSendService _packetSendService;
        private readonly IPingTimeRepository _pingTimeRepository;
        private readonly Random _random;

        public string CommandText { get { return "ping"; } }

        public PingCommand(IPacketSendService packetSendService,
                           IPingTimeRepository pingTimeRepository)
        {
            _packetSendService = packetSendService;
            _pingTimeRepository = pingTimeRepository;
            _random = new Random();
        }

        public bool Execute(string parameter)
        {
            ushort requestID;
            do
            {
                requestID = (ushort) _random.Next(ushort.MinValue, ushort.MaxValue - 1);
            } while (_pingTimeRepository.PingRequests.ContainsKey(requestID));

            _pingTimeRepository.PingRequests.Add(requestID, DateTime.Now);

            var packet = new PacketBuilder(PacketFamily.Message, PacketAction.Ping)
                .AddShort((short)requestID)
                .Build();

            _packetSendService.SendPacketAsync(packet);
            return true;
        }
    }
}
