using AutomaticTypeMapper;
using EOLib.Domain.Online;
using EOLib.Domain.Protocol;
using EOLib.IO.Repositories;
using EOLib.Net;

namespace EOLib.PacketHandlers.Init
{
    [AutoMappedType]
    public class OnlinePlayerListHandler : BasePlayersListHandler
    {
        private readonly IECFFileProvider _classFileProvider;

        public override InitReply Reply => InitReply.AllPlayersList;

        public OnlinePlayerListHandler(IOnlinePlayerRepository onlinePlayerRepository,
                                       IECFFileProvider classFileProvider)
            : base(onlinePlayerRepository)
        {
            _classFileProvider = classFileProvider;
        }

        protected override OnlinePlayerInfo GetNextRecord(IPacket packet)
        {
            string name = packet.ReadBreakString();

            var title = packet.ReadBreakString();
            if (packet.ReadChar() != 0)
                throw new MalformedPacketException("Expected 0 char after online entry title", packet);

            var iconType = (OnlineIcon)packet.ReadChar();
            int clsId = packet.ReadChar();
            var guild = packet.ReadBreakString();

            name = char.ToUpper(name[0]) + name.Substring(1);

            if (string.IsNullOrWhiteSpace(title))
                title = "-";
            else
                title = char.ToUpper(title[0]) + title.Substring(1);

            var className = _classFileProvider.ECFFile.Length >= clsId
                ? _classFileProvider.ECFFile[clsId].Name
                : "-";

            if (string.IsNullOrWhiteSpace(guild))
                guild = "-";

            return new OnlinePlayerInfo(name, title, guild, className, iconType);
        }
    }
}
