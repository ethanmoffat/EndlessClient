using AutomaticTypeMapper;
using EOLib.Domain.Protocol;
using EOLib.IO.Map;
using EOLib.IO.Pub;
using EOLib.IO.Services.Serializers;
using EOLib.Net.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EOLib.Net.FileTransfer
{
    [AutoMappedType]
    public class FileRequestService : IFileRequestService
    {
        private readonly IPacketSendService _packetSendService;
        private readonly IMapDeserializer<IMapFile> _mapFileSerializer;
        private readonly IPubFileDeserializer _pubFileDeserializer;

        public FileRequestService(IPacketSendService packetSendService,
                                  IMapDeserializer<IMapFile> mapFileSerializer,
                                  IPubFileDeserializer pubFileDeserializer)
        {
            _packetSendService = packetSendService;
            _mapFileSerializer = mapFileSerializer;
            _pubFileDeserializer = pubFileDeserializer;
        }

        public async Task<IMapFile> RequestMapFile(int mapID, int sessionID)
        {
            var request = new PacketBuilder(PacketFamily.Welcome, PacketAction.Agree)
                .AddChar((int)InitFileType.Map)
                .AddShort(sessionID)
                .AddShort(mapID)
                .Build();

            return await GetMapFile(request, mapID);
        }

        public void RequestMapFileForWarp(int mapID, int sessionID)
        {
            var request = new PacketBuilder(PacketFamily.Warp, PacketAction.Take)
                .AddShort(mapID)
                .AddShort(sessionID)
                .Build();

            _packetSendService.SendPacket(request);
        }

        public async Task<List<IPubFile<TRecord>>> RequestFile<TRecord>(InitFileType fileType, int sessionID)
            where TRecord : class, IPubRecord, new()
        {
            var retList = new List<IPubFile<TRecord>>(4);

            int fileId = 1;
            int expectedRecords = 0, actualRecords = 0;

            do
            {
                var request = new PacketBuilder(PacketFamily.Welcome, PacketAction.Agree)
                    .AddChar((int)fileType)
                    .AddShort(sessionID)
                    .AddChar(fileId) // file id (for chunking oversize pub files)
                    .Build();

                var response = await _packetSendService.SendEncodedPacketAndWaitAsync(request);
                if (!PacketIsValid(response))
                    throw new EmptyPacketReceivedException();

                var responseFileType = (InitReply)response.ReadByte();

                var fileIdResponse = response.ReadChar();
                if (fileIdResponse != fileId)
                    throw new MalformedPacketException($"Unexpected fileId (actual={fileIdResponse}, expected={fileId})", response);

                Func<IPubFile<TRecord>> factory;
                switch (responseFileType)
                {
                    case InitReply.ItemFile: factory = () => (IPubFile<TRecord>)new EIFFile(); break;
                    case InitReply.NpcFile: factory = () => (IPubFile<TRecord>)new ENFFile(); break;
                    case InitReply.SpellFile: factory = () => (IPubFile<TRecord>)new ESFFile(); break;
                    case InitReply.ClassFile: factory = () => (IPubFile<TRecord>)new ECFFile(); break;
                    default: throw new EmptyPacketReceivedException();
                }

                var responseBytes = response
                    .ReadBytes(response.Length - response.ReadPosition)
                    .ToArray();

                var resultFile = _pubFileDeserializer.DeserializeFromByteArray(fileId, responseBytes, factory);
                retList.Add(resultFile);

                if (fileId == 1)
                {
                    expectedRecords = resultFile.TotalLength;
                }
                actualRecords += resultFile.Length;

                fileId++;

            } while (expectedRecords > actualRecords);

            return retList;
        }

        private async Task<IMapFile> GetMapFile(IPacket request, int mapID)
        {
            var response = await _packetSendService.SendEncodedPacketAndWaitAsync(request);
            if (!PacketIsValid(response))
                throw new EmptyPacketReceivedException();

            var fileType = (InitReply)response.ReadByte();
            if (fileType != InitReply.MapFile && fileType != InitReply.WarpMap)
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
        Task<IMapFile> RequestMapFile(int mapID, int sessionID);

        void RequestMapFileForWarp(int mapID, int sessionID);

        Task<List<IPubFile<TRecord>>> RequestFile<TRecord>(InitFileType fileType, int sessionID)
            where TRecord : class, IPubRecord, new();
    }
}