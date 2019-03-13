// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using System.Threading.Tasks;
using AutomaticTypeMapper;
using EOLib.Domain.Protocol;
using EOLib.IO.Map;
using EOLib.IO.Pub;
using EOLib.IO.Services;
using EOLib.IO.Services.Serializers;
using EOLib.Net.Communication;

namespace EOLib.Net.FileTransfer
{
    [AutoMappedType]
    public class FileRequestService : IFileRequestService
    {
        private readonly IPacketSendService _packetSendService;
        private readonly INumberEncoderService _numberEncoderService;
        private readonly ISerializer<IMapFile> _mapFileSerializer;

        public FileRequestService(IPacketSendService packetSendService,
                                  INumberEncoderService numberEncoderService,
                                  ISerializer<IMapFile> mapFileSerializer)
        {
            _packetSendService = packetSendService;
            _numberEncoderService = numberEncoderService;
            _mapFileSerializer = mapFileSerializer;
        }

        public async Task<IMapFile> RequestMapFile(short mapID)
        {
            var request = new PacketBuilder(PacketFamily.Welcome, PacketAction.Agree)
                .AddChar((byte) InitFileType.Map)
                .Build();

            return await GetMapFile(request, mapID, false);
        }

        public async Task<IMapFile> RequestMapFileForWarp(short mapID)
        {
            var request = new PacketBuilder(PacketFamily.Warp, PacketAction.Take).Build();
            return await GetMapFile(request, mapID, true);
        }

        public async Task<IPubFile> RequestFile(InitFileType fileType)
        {
            var request = new PacketBuilder(PacketFamily.Welcome, PacketAction.Agree)
                .AddChar((byte) fileType)
                .Build();

            var response = await _packetSendService.SendEncodedPacketAndWaitAsync(request);
            if (!PacketIsValid(response))
                throw new EmptyPacketReceivedException();

            var responseFileType = (InitReply) response.ReadChar();
            
            var extraByte = response.ReadChar();
            if (extraByte != 1)
                throw new MalformedPacketException("Missing extra single byte in file transfer packet", response);

            IPubFile retFile;
            switch (responseFileType)
            {
                case InitReply.ItemFile: retFile = new EIFFile(); break;
                case InitReply.NpcFile: retFile = new ENFFile(); break;
                case InitReply.SpellFile: retFile = new ESFFile(); break;
                case InitReply.ClassFile: retFile = new ECFFile(); break;
                default: throw new EmptyPacketReceivedException();
            }

            var responseBytes = response.ReadBytes(response.Length - response.ReadPosition)
                                        .ToArray();
            retFile.DeserializeFromByteArray(responseBytes, _numberEncoderService);

            return retFile;
        }

        private async Task<IMapFile> GetMapFile(IPacket request, int mapID, bool isWarp)
        {
            var response = await _packetSendService.SendEncodedPacketAndWaitAsync(request);
            if (!PacketIsValid(response))
                throw new EmptyPacketReceivedException();

            var fileType = (InitReply)(isWarp ? response.ReadByte() : response.ReadChar());
            if (fileType != InitReply.MapFile)
                throw new MalformedPacketException("Invalid file type " + fileType + " when requesting a map file", response);

            var fileData = response.ReadBytes(response.Length - response.ReadPosition);

            var mapFile = _mapFileSerializer
                .DeserializeFromByteArray(fileData.ToArray())
                .WithMapID(mapID);

            return mapFile;
        }

        private static bool PacketIsValid(IPacket packet)
        {
            return packet.Family == PacketFamily.Init && packet.Action == PacketAction.Init;
        }
    }

    public interface IFileRequestService
    {
        Task<IMapFile> RequestMapFile(short mapID);

        Task<IMapFile> RequestMapFileForWarp(short mapID);

        Task<IPubFile> RequestFile(InitFileType fileType);
    }
}