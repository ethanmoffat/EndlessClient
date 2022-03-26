using AutomaticTypeMapper;
using EOLib.Domain.Protocol;
using EOLib.IO.Map;
using EOLib.IO.Pub;
using EOLib.IO.Services.Serializers;
using EOLib.Net.Communication;
using System;
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

        public async Task<IMapFile> RequestMapFile(short mapID, short sessionID)
        {
            var request = new PacketBuilder(PacketFamily.Welcome, PacketAction.Agree)
                .AddChar((byte)InitFileType.Map)
                .AddShort((ushort)sessionID)
                .AddShort(mapID)
                .Build();

            return await GetMapFile(request, mapID);
        }

        public async Task<IMapFile> RequestMapFileForWarp(short mapID, short sessionID)
        {
            var request = new PacketBuilder(PacketFamily.Warp, PacketAction.Take)
                .AddShort(mapID)
                .AddShort((ushort)sessionID)
                .Build();

            return await GetMapFile(request, mapID);
        }

        public async Task<IPubFile<TRecord>> RequestFile<TRecord>(InitFileType fileType, short sessionID)
            where TRecord : class, IPubRecord, new()
        {
            var request = new PacketBuilder(PacketFamily.Welcome, PacketAction.Agree)
                .AddChar((byte)fileType)
                .AddShort((ushort)sessionID)
                .AddChar(1) // file id (for chunking oversize pub files)
                .Build();

            var response = await _packetSendService.SendEncodedPacketAndWaitAsync(request);
            if (!PacketIsValid(response))
                throw new EmptyPacketReceivedException();

            var responseFileType = (InitReply) response.ReadByte();
            
            var extraByte = response.ReadChar();
            if (extraByte != 1)
                throw new MalformedPacketException("Missing extra single byte in file transfer packet", response);

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

            return _pubFileDeserializer.DeserializeFromByteArray(responseBytes, factory);
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
        Task<IMapFile> RequestMapFile(short mapID, short sessionID);

        Task<IMapFile> RequestMapFileForWarp(short mapID, short sessionID);

        Task<IPubFile<TRecord>> RequestFile<TRecord>(InitFileType fileType, short sessionID)
            where TRecord : class, IPubRecord, new();
    }
}