using AutomaticTypeMapper;
using EOLib.Domain.Map;
using EOLib.Domain.Protocol;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using EOLib.IO.Services;
using EOLib.IO.Services.Serializers;
using EOLib.Net;
using EOLib.Net.Communication;
using System.Linq;

namespace EOLib.PacketHandlers.Init
{
    [AutoMappedType]
    public class MapWarpFileDownloadHandler : IInitPacketHandler
    {
        private readonly IMapFileRepository _mapFileRepository;
        private readonly IMapDeserializer<IMapFile> _mapFileDeserializer;
        private readonly IMapFileSaveService _mapFileSaveService;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly IPacketSendService _packetSendService;

        public InitReply Reply => InitReply.WarpMap;

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

        public bool HandlePacket(IPacket packet)
        {
            if (_currentMapStateProvider.MapWarpState != WarpState.WarpStarted)
                return false;

            if (!_currentMapStateProvider.MapWarpID.HasValue ||
                !_currentMapStateProvider.MapWarpSession.HasValue)
                return false;

            _currentMapStateProvider.MapWarpID.MatchSome(
                mapID =>
                {
                    var fileData = packet.ReadBytes(packet.Length - packet.ReadPosition);
                    var mapFile = _mapFileDeserializer
                        .DeserializeFromByteArray(fileData.ToArray())
                        .WithMapID(mapID);

                    _mapFileRepository.MapFiles[mapID] = mapFile;
                    _mapFileSaveService.SaveFileToDefaultDirectory(mapFile, rewriteChecksum: false);

                    _currentMapStateProvider.MapWarpSession.MatchSome(sessionID => SendWarpAcceptToServer(mapID, sessionID));
                });

            return true;
        }

        private void SendWarpAcceptToServer(int mapID, int sessionID)
        {
            var response = new PacketBuilder(PacketFamily.Warp, PacketAction.Accept)
                .AddShort(mapID)
                .AddShort(sessionID)
                .Build();
            _packetSendService.SendPacket(response);
        }
    }
}
