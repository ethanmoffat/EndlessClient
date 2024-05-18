using AutomaticTypeMapper;
using EOLib.Domain.Online;
using EOLib.IO.Repositories;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers.Init
{
    [AutoMappedType]
    public class OnlinePlayerListHandler : BaseInGameInitPacketHandler<InitInitServerPacket.ReplyCodeDataPlayersList>
    {
        private readonly IOnlinePlayerRepository _onlinePlayerRepository;
        private readonly IECFFileProvider _classFileProvider;

        public override InitReply Reply => InitReply.PlayersList;

        public OnlinePlayerListHandler(IOnlinePlayerRepository onlinePlayerRepository,
                                       IECFFileProvider classFileProvider)
        {
            _onlinePlayerRepository = onlinePlayerRepository;
            _classFileProvider = classFileProvider;
        }

        public override bool HandleData(InitInitServerPacket.ReplyCodeDataPlayersList packet)
        {
            _onlinePlayerRepository.OnlinePlayers = new HashSet<OnlinePlayerInfo>(packet.PlayersList.Players.Select(x =>
            {
                var name = char.ToUpper(x.Name[0]) + x.Name.Substring(1);
                var title = string.IsNullOrEmpty(x.Title)
                    ? "-"
                    : char.ToUpper(x.Title[0]) + x.Title.Substring(1);

                var className = _classFileProvider.ECFFile.Length < x.ClassId || x.ClassId == 0
                    ? "-"
                    : _classFileProvider.ECFFile[x.ClassId].Name;

                var guild = string.IsNullOrWhiteSpace(x.GuildTag)
                    ? "-"
                    : x.GuildTag;

                return new OnlinePlayerInfo(name, title, guild, className, x.Icon);

            }));

            return true;
        }
    }
}
