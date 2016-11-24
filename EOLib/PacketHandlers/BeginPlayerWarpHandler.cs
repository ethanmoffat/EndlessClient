// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.IO;
using System.Linq;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.IO.Actions;
using EOLib.Net;
using EOLib.Net.Communication;
using EOLib.Net.FileTransfer;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    public class BeginPlayerWarpHandler : InGameOnlyPacketHandler
    {
        private const int WARP_SAME_MAP = 1, WARP_NEW_MAP = 2;

        private readonly IPacketSendService _packetSendService;
        private readonly IFileRequestActions _fileRequestActions;
        private readonly IMapFileLoadActions _mapFileLoadActions;
        private readonly ICurrentMapStateRepository _mapStateRepository;

        public override PacketFamily Family { get { return PacketFamily.Warp; } }

        public override PacketAction Action { get { return PacketAction.Request; } }

        public BeginPlayerWarpHandler(IPlayerInfoProvider playerInfoProvider,
                                      IPacketSendService packetSendService,
                                      IFileRequestActions fileRequestActions,
                                      IMapFileLoadActions mapFileLoadActions,
                                      ICurrentMapStateRepository mapStateRepository)
            : base(playerInfoProvider)
        {
            _packetSendService = packetSendService;
            _fileRequestActions = fileRequestActions;
            _mapFileLoadActions = mapFileLoadActions;
            _mapStateRepository = mapStateRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            if (_mapStateRepository.MapWarpState != WarpState.None)
                throw new InvalidOperationException("Attempted to warp while another warp was in progress");
            _mapStateRepository.MapWarpState = WarpState.WarpStarted;

            var warpType = packet.ReadChar();
            switch (warpType)
            {
                case WARP_SAME_MAP:
                    SendWarpAcceptToServer(packet);
                    break;
                case WARP_NEW_MAP:
                    var mapID = packet.ReadShort();
                    var mapRid = packet.ReadBytes(4).ToArray();
                    var fileSize = packet.ReadThree();

                    var mapIsDownloaded = true;
                    try
                    {
                        _mapFileLoadActions.LoadMapFileByID(mapID);
                    }
                    catch (IOException) { mapIsDownloaded = false; }

                    if (!mapIsDownloaded || _fileRequestActions.NeedsMapForWarp(mapID, mapRid, fileSize))
                        _fileRequestActions.GetMapForWarp(mapID).Wait(5000);

                    SendWarpAcceptToServer(packet);
                    break;
                default:
                    _mapStateRepository.MapWarpState = WarpState.None;
                    return false;
            }

            return true;
        }

        private void SendWarpAcceptToServer(IPacket packet)
        {
            var response = new PacketBuilder(PacketFamily.Warp, PacketAction.Accept)
                .AddShort(packet.ReadShort())
                .Build();
            _packetSendService.SendPacket(response);
        }
    }
}
