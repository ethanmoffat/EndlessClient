using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Chat.Commands
{
    [AutoMappedType]
    public class FindCommand : IPlayerCommand
    {
        private readonly IPacketSendService _packetSendService;
        public string CommandText => "find";

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