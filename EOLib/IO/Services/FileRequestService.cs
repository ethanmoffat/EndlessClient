// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EOLib.Domain.Protocol;
using EOLib.IO.Map;
using EOLib.IO.Old;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.IO.Services
{
    public class FileRequestService : IFileRequestService
    {
        private readonly IPacketSendService _packetSendService;

        public FileRequestService(IPacketSendService packetSendService)
        {
            _packetSendService = packetSendService;
        }

        public async Task<IMapFile> RequestMapFile(short mapID)
        {
            var request = new PacketBuilder(PacketFamily.Welcome, PacketAction.Agree)
                .AddChar((byte) InitFileType.Map)
                .Build();

            var response = await _packetSendService.SendEncodedPacketAndWaitAsync(request);
            if (!PacketIsValid(response))
                throw new EmptyPacketReceivedException();

            var fileType = (InitReply)response.ReadChar();
            if (fileType != InitReply.MapFile)
                throw new EmptyPacketReceivedException();

            var fileData = response.ReadBytes(response.Length - response.ReadPosition);
            return MapFile.FromBytes(mapID, fileData);
        }

        public async Task<IModifiableDataFile<T>> RequestFile<T>(InitFileType fileType) where T : IDataRecord
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

            var responseBytes = response.ReadBytes(response.Length - response.ReadPosition);
            switch (responseFileType)
            {
                case InitReply.ItemFile: return (IModifiableDataFile<T>)ItemFile.FromBytes(responseBytes);
                case InitReply.NpcFile: return (IModifiableDataFile<T>)NPCFile.FromBytes(responseBytes);
                case InitReply.SpellFile: return (IModifiableDataFile<T>)SpellFile.FromBytes(responseBytes);
                case InitReply.ClassFile: return (IModifiableDataFile<T>)ClassFile.FromBytes(responseBytes);
                default: throw new EmptyPacketReceivedException();
            }
        }

        private bool PacketIsValid(IPacket packet)
        {
            return packet.Family == PacketFamily.Init && packet.Action == PacketAction.Init;
        }
    }
}