using System;
using System.IO;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.IO.Actions;
using EOLib.IO.Repositories;
using EOLib.Net;
using EOLib.Net.Communication;
using EOLib.Net.FileTransfer;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class BeginPlayerWarpHandler : InGameOnlyPacketHandler
    {
        private const int WARP_SAME_MAP = 1, WARP_NEW_MAP = 2;

        private readonly IPacketSendService _packetSendService;
        private readonly IFileRequestActions _fileRequestActions;
        private readonly IMapFileLoadActions _mapFileLoadActions;
        private readonly ICurrentMapStateRepository _mapStateRepository;
        private readonly IMapFileProvider _mapFileProvider;

        public override PacketFamily Family => PacketFamily.Warp;

        public override PacketAction Action => PacketAction.Request;

        public BeginPlayerWarpHandler(IPlayerInfoProvider playerInfoProvider,
                                      IPacketSendService packetSendService,
                                      IFileRequestActions fileRequestActions,
                                      IMapFileLoadActions mapFileLoadActions,
                                      ICurrentMapStateRepository mapStateRepository,
                                      IMapFileProvider mapFileProvider)
            : base(playerInfoProvider)
        {
            _packetSendService = packetSendService;
            _fileRequestActions = fileRequestActions;
            _mapFileLoadActions = mapFileLoadActions;
            _mapStateRepository = mapStateRepository;
            _mapFileProvider = mapFileProvider;
        }

        public override bool HandlePacket(IPacket packet)
        {
            if (_mapStateRepository.MapWarpState != WarpState.None)
                throw new InvalidOperationException("Attempted to warp while another warp was in progress");
            _mapStateRepository.MapWarpState = WarpState.WarpStarted;

            var warpType = packet.ReadChar();
            var mapID = packet.ReadShort();
            short sessionID;
            switch (warpType)
            {
                case WARP_SAME_MAP:
                    sessionID = packet.ReadShort();
                    SendWarpAcceptToServer(mapID, sessionID);
                    break;
                case WARP_NEW_MAP:
                    var mapRid = packet.ReadBytes(4).ToArray();
                    var fileSize = packet.ReadThree();
                    sessionID = packet.ReadShort();

                    var mapIsDownloaded = true;
                    try
                    {
                        if (!_mapFileProvider.MapFiles.ContainsKey(mapID))
                            _mapFileLoadActions.LoadMapFileByID(mapID);
                    }
                    catch (IOException) { mapIsDownloaded = false; }

                    if (!mapIsDownloaded || _fileRequestActions.NeedsMapForWarp(mapID, mapRid, fileSize))
                        _fileRequestActions.GetMapFromServer(mapID, sessionID).Wait(5000);

                    SendWarpAcceptToServer(mapID, sessionID);
                    break;
                default:
                    _mapStateRepository.MapWarpState = WarpState.None;
                    return false;
            }

            return true;
        }

        private void SendWarpAcceptToServer(short mapID, short sessionID)
        {
            var response = new PacketBuilder(PacketFamily.Warp, PacketAction.Accept)
                .AddShort(mapID)
                .AddShort((ushort)sessionID)
                .Build();
            _packetSendService.SendPacket(response);
        }
    }
}
