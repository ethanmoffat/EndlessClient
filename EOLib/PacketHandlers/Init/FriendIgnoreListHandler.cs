using AutomaticTypeMapper;
using EOLib.Domain.Online;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers.Init
{
    [AutoMappedType]
    public class FriendIgnoreListHandler : BaseInGameInitPacketHandler<InitInitServerPacket.ReplyCodeDataPlayersListFriends>
    {
        private readonly IOnlinePlayerRepository _onlinePlayerRepository;

        public override InitReply Reply => InitReply.PlayersListFriends;

        public Type DataType => typeof(InitInitServerPacket.ReplyCodeDataPlayersListFriends);

        public FriendIgnoreListHandler(IOnlinePlayerRepository onlinePlayerRepository)
        {
            _onlinePlayerRepository = onlinePlayerRepository;
        }

        public override bool HandleData(InitInitServerPacket.ReplyCodeDataPlayersListFriends data)
        {
            _onlinePlayerRepository.OnlinePlayers = new HashSet<OnlinePlayerInfo>(data.PlayersList.Players.Select(x => new OnlinePlayerInfo(x)));
            return true;
        }
    }
}