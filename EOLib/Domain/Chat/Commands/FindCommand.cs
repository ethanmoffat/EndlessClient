using AutomaticTypeMapper;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

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

            _packetSendService.SendPacketAsync(new PlayersAcceptClientPacket { Name = parameter });

            return true;
        }
    }
}