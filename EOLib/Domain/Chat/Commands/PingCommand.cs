using AutomaticTypeMapper;
using EOLib.Domain.Protocol;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

namespace EOLib.Domain.Chat.Commands
{
    [AutoMappedType]
    public class PingCommand : IPlayerCommand
    {
        private readonly IPacketSendService _packetSendService;
        private readonly IPingTimeRepository _pingTimeRepository;

        public const string Text = "ping";

        public string CommandText => Text;

        public PingCommand(IPacketSendService packetSendService,
                           IPingTimeRepository pingTimeRepository)
        {
            _packetSendService = packetSendService;
            _pingTimeRepository = pingTimeRepository;
        }

        public bool Execute(string parameter)
        {
            _packetSendService.SendPacket(new MessagePingClientPacket());
            _pingTimeRepository.RequestTimer.Start();
            return true;
        }
    }
}
