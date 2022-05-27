using AutomaticTypeMapper;
using EOLib.Domain.Online;
using EOLib.Domain.Protocol;
using EOLib.Net;

namespace EOLib.PacketHandlers.Init
{
    [AutoMappedType]
    public class FriendIgnoreListHandler : BasePlayersListHandler
    {
        public override InitReply Reply => InitReply.FriendPlayersList;

        public FriendIgnoreListHandler(IOnlinePlayerRepository onlinePlayerRepository)
            : base(onlinePlayerRepository)
        {
        }

        protected override OnlinePlayerInfo GetNextRecord(IPacket packet)
        {
            string name = packet.ReadBreakString();
            return new OnlinePlayerInfo(name);
        }
    }
}
