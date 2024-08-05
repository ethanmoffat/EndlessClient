using AutomaticTypeMapper;
using EOLib.Domain.Map;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using EOLib.IO.Services;
using EOLib.IO.Services.Serializers;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Init
{
    [AutoMappedType]
    public class MapWarpFileDownloadHandler : BaseInGameInitPacketHandler<InitInitServerPacket.ReplyCodeDataWarpMap>
    {
        private readonly IMapFileRepository _mapFileRepository;
        private readonly IMapDeserializer<IMapFile> _mapFileDeserializer;
        private readonly IMapFileSaveService _mapFileSaveService;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly IPacketSendService _packetSendService;

        public override InitReply Reply => InitReply.WarpMap;

        public MapWarpFileDownloadHandler(IMapFileRepository mapFileRepository,
                                          IMapDeserializer<IMapFile> mapFileDeserializer,
                                          IMapFileSaveService mapFileSaveService,
                                          ICurrentMapStateProvider currentMapStateProvider,
                                          IPacketSendService packetSendService)
        {
            _mapFileRepository = mapFileRepository;
            _mapFileDeserializer = mapFileDeserializer;
            _mapFileSaveService = mapFileSaveService;
            _currentMapStateProvider = currentMapStateProvider;
            _packetSendService = packetSendService;
        }

        public override bool HandleData(InitInitServerPacket.ReplyCodeDataWarpMap packet)
        {
            if (_currentMapStateProvider.MapWarpState != WarpState.WarpStarted)
                return false;

            if (!_currentMapStateProvider.MapWarpID.HasValue ||
                !_currentMapStateProvider.MapWarpSession.HasValue)
                return false;

            _currentMapStateProvider.MapWarpID.MatchSome(
                mapID =>
                {
                    var mapFile = _mapFileDeserializer
                        .DeserializeFromByteArray(packet.MapFile.Content)
                        .WithMapID(mapID);

                    _mapFileRepository.MapFiles[mapID] = mapFile;
                    _mapFileSaveService.SaveFileToDefaultDirectory(mapFile, rewriteChecksum: false);

                    _currentMapStateProvider.MapWarpSession.MatchSome(sessionID => SendWarpAcceptToServer(mapID, sessionID));
                });

            return true;
        }

        private void SendWarpAcceptToServer(int mapID, int sessionID)
        {
            _packetSendService.SendPacket(new WarpAcceptClientPacket
            {
                MapId = mapID,
                SessionId = sessionID
            });
        }
    }
}