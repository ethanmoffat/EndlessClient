using System;
using System.IO;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.IO.Actions;
using EOLib.IO.Repositories;
using EOLib.Net.Communication;
using EOLib.Net.FileTransfer;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Warp
{
    /// <summary>
    /// Sent when the server requests warp for the main character
    /// </summary>
    [AutoMappedType]
    public class WarpRequestHandler : InGameOnlyPacketHandler<WarpRequestServerPacket>
    {
        private readonly IPacketSendService _packetSendService;
        private readonly IFileRequestActions _fileRequestActions;
        private readonly IMapFileLoadActions _mapFileLoadActions;
        private readonly ICurrentMapStateRepository _mapStateRepository;
        private readonly IMapFileProvider _mapFileProvider;

        public override PacketFamily Family => PacketFamily.Warp;

        public override PacketAction Action => PacketAction.Request;

        public WarpRequestHandler(IPlayerInfoProvider playerInfoProvider,
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

        public override bool HandlePacket(WarpRequestServerPacket packet)
        {
            if (_mapStateRepository.MapWarpState != WarpState.None)
                throw new InvalidOperationException("Attempted to warp while another warp was in progress");

            _mapStateRepository.MapWarpState = WarpState.WarpStarted;

            var warpType = packet.WarpType;
            var mapID = packet.MapId;

            switch (warpType)
            {
                case WarpType.Local:
                    SendWarpAcceptToServer(mapID, packet.SessionId);
                    break;
                case WarpType.MapSwitch:
                    {
                        var data = (WarpRequestServerPacket.WarpTypeDataMapSwitch)packet.WarpTypeData;

                        var mapIsDownloaded = true;
                        try
                        {
                            if (!_mapFileProvider.MapFiles.ContainsKey(mapID))
                                _mapFileLoadActions.LoadMapFileByID(mapID);
                        }
                        catch (IOException) { mapIsDownloaded = false; }

                        if (!mapIsDownloaded || _fileRequestActions.NeedsMapForWarp(mapID, data.MapRid, data.MapFileSize))
                        {
                            _fileRequestActions.RequestMapForWarp(mapID, packet.SessionId);
                        }
                        else
                        {
                            SendWarpAcceptToServer(mapID, packet.SessionId);
                        }
                    }
                    break;
                default:
                    _mapStateRepository.MapWarpState = WarpState.None;
                    return false;
            }

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
