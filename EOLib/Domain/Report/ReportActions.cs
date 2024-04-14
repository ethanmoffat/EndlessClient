using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Report
{
    public class ReportActions : IReportActions
    {
        private readonly IPacketSendService _packetSendService;

        public ReportActions(IPacketSendService packetSendService)
        {
            _packetSendService = packetSendService;
        }

        public void ReportPlayer(string player, string message)
        {
            var packet = new PacketBuilder(PacketFamily.AdminInteract, PacketAction.Report)
                .AddString(player)
                .AddByte(255)
                .AddString(message)
                .Build();
            _packetSendService.SendPacket(packet);
        }

        public void SpeakToAdmin(string message)
        {
            var packet = new PacketBuilder(PacketFamily.AdminInteract, PacketAction.Tell)
                .AddString(message)
                .Build();
            _packetSendService.SendPacket(packet);
        }
    }

    public interface IReportActions
    {
        void ReportPlayer(string player, string message);

        void SpeakToAdmin(string message);
    }
}
