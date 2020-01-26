// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using AutomaticTypeMapper;
using EOLib.Domain.Protocol;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Chat.Commands
{
    [AutoMappedType]
    public class PingCommand : IPlayerCommand
    {
        private readonly IPacketSendService _packetSendService;
        private readonly IPingTimeRepository _pingTimeRepository;
        private readonly Random _random;

        public string CommandText => "ping";

        public PingCommand(IPacketSendService packetSendService,
                           IPingTimeRepository pingTimeRepository)
        {
            _packetSendService = packetSendService;
            _pingTimeRepository = pingTimeRepository;
            _random = new Random();
        }

        public bool Execute(string parameter)
        {
            short requestID;
            do
            {
                requestID = (short) _random.Next(0, short.MaxValue - 1);
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
