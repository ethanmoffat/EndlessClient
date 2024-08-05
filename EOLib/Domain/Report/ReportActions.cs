using AutomaticTypeMapper;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

namespace EOLib.Domain.Report
{
    [AutoMappedType]
    public class ReportActions : IReportActions
    {
        private readonly IPacketSendService _packetSendService;

        public ReportActions(IPacketSendService packetSendService)
        {
            _packetSendService = packetSendService;
        }

        public void ReportPlayer(string player, string message)
        {
            var packet = new AdminInteractReportClientPacket
            {
                Reportee = player,
                Message = message
            };
            _packetSendService.SendPacket(packet);
        }

        public void SpeakToAdmin(string message)
        {
            var packet = new AdminInteractTellClientPacket { Message = message };
            _packetSendService.SendPacket(packet);
        }
    }

    public interface IReportActions
    {
        void ReportPlayer(string player, string message);

        void SpeakToAdmin(string message);
    }
}