using AutomaticTypeMapper;
using EOLib.IO.Map;
using EOLib.IO.Pub;
using EOLib.IO.Services.Serializers;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System;
using System.Collections.Generic;
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
            var request = new WelcomeAgreeClientPacket
            {
                FileType = FileType.Emf,
                FileTypeData = new WelcomeAgreeClientPacket.FileTypeDataEmf { FileId = mapID },
                SessionId = sessionID
            };

            return await GetMapFile(request, mapID);
        }

        public void RequestMapFileForWarp(int mapID, int sessionID)
        {
            var request = new WarpTakeClientPacket
            {
                MapId = mapID,
                SessionId = sessionID
            };

            _packetSendService.SendPacket(request);
        }

        public async Task<List<IPubFile<TRecord>>> RequestFile<TRecord>(FileType fileType, int sessionID)
            where TRecord : class, IPubRecord, new()
        {
            var retList = new List<IPubFile<TRecord>>(4);

            int fileId = 1;
            int expectedRecords = 0, actualRecords = 0;

            do
            {
                var request = new WelcomeAgreeClientPacket
                {
                    FileType = fileType,
                    FileTypeData = GetPubRequestData(fileType, fileId),
                    SessionId = sessionID,
                };

                var response = await _packetSendService.SendEncodedPacketAndWaitAsync(request);
                if (!PacketIsValid(response, out var responsePacket))
                    throw new EmptyPacketReceivedException();

                var responseFileType = responsePacket.ReplyCode;

                if (!PubFileIdMatches(fileId, responsePacket.ReplyCodeData, out var responseFileId))
                    throw new InvalidOperationException($"Unexpected fileId (actual={responseFileId}, expected={fileId})");

                Func<IPubFile<TRecord>> factory;
                switch (responseFileType)
                {
                    case InitReply.FileEif: factory = () => (IPubFile<TRecord>)new EIFFile(); break;
                    case InitReply.FileEnf: factory = () => (IPubFile<TRecord>)new ENFFile(); break;
                    case InitReply.FileEsf: factory = () => (IPubFile<TRecord>)new ESFFile(); break;
                    case InitReply.FileEcf: factory = () => (IPubFile<TRecord>)new ECFFile(); break;
                    default: throw new EmptyPacketReceivedException();
                }

                var responseBytes = GetFileData(responsePacket.ReplyCodeData);
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
            if (!PacketIsValid(response, out var responsePacket))
                throw new EmptyPacketReceivedException();

            var fileType = responsePacket.ReplyCode;
            if (fileType != InitReply.FileEmf && fileType != InitReply.WarpMap)
                throw new InvalidOperationException($"Invalid file type {fileType} when requesting a map file");

            var mapFile = _mapFileSerializer
                .DeserializeFromByteArray(GetFileData(responsePacket.ReplyCodeData))
                .WithMapID(mapID);

            return mapFile;
        }

        private static bool PacketIsValid(IPacket packet, out InitInitServerPacket responsePacket)
        {
            responsePacket = packet as InitInitServerPacket;
            return responsePacket != null && packet.Family == PacketFamily.Init && packet.Action == PacketAction.Init;
        }

        private static byte[] GetFileData(InitInitServerPacket.IReplyCodeData replyCodeData)
        {
            if (replyCodeData is InitInitServerPacket.ReplyCodeDataWarpMap wm)
                return wm.MapFile.Content;
            else if (replyCodeData is InitInitServerPacket.ReplyCodeDataFileEmf emf)
                return emf.MapFile.Content;
            else if (replyCodeData is InitInitServerPacket.ReplyCodeDataFileEif eif)
                return eif.PubFile.Content;
            else if (replyCodeData is InitInitServerPacket.ReplyCodeDataFileEnf enf)
                return enf.PubFile.Content;
            else if (replyCodeData is InitInitServerPacket.ReplyCodeDataFileEsf esf)
                return esf.PubFile.Content;
            else if (replyCodeData is InitInitServerPacket.ReplyCodeDataFileEcf ecf)
                return ecf.PubFile.Content;
            else
                throw new ArgumentException("Unexpected reply code data type when requesting file");
        }

        private static WelcomeAgreeClientPacket.IFileTypeData GetPubRequestData(FileType fileType, int fileId)
        {
            return fileType switch
            {
                FileType.Eif => new WelcomeAgreeClientPacket.FileTypeDataEif { FileId = fileId },
                FileType.Enf => new WelcomeAgreeClientPacket.FileTypeDataEnf { FileId = fileId },
                FileType.Esf => new WelcomeAgreeClientPacket.FileTypeDataEsf { FileId = fileId },
                FileType.Ecf => new WelcomeAgreeClientPacket.FileTypeDataEcf { FileId = fileId },
                _ => throw new ArgumentOutOfRangeException(nameof(fileType)),
            };
        }
    
        private static bool PubFileIdMatches(int requestedFileId, InitInitServerPacket.IReplyCodeData replyCodeData, out int responseFileId)
        {
            if (replyCodeData is InitInitServerPacket.ReplyCodeDataFileEif eif)
            {
                responseFileId = eif.PubFile.FileId;
                return responseFileId == requestedFileId;
            }
            else if (replyCodeData is InitInitServerPacket.ReplyCodeDataFileEnf enf)
            {
                responseFileId = enf.PubFile.FileId;
                return responseFileId == requestedFileId;
            }
            else if (replyCodeData is InitInitServerPacket.ReplyCodeDataFileEsf esf)
            {
                responseFileId = esf.PubFile.FileId;
                return responseFileId == requestedFileId;
            }
            else if (replyCodeData is InitInitServerPacket.ReplyCodeDataFileEcf ecf)
            {
                responseFileId = ecf.PubFile.FileId;
                return responseFileId == requestedFileId;
            }

            throw new ArgumentException("Unexpected reply code data type when requesting pub file");
        }
    }

    public interface IFileRequestService
    {
        Task<IMapFile> RequestMapFile(int mapID, int sessionID);

        void RequestMapFileForWarp(int mapID, int sessionID);

        Task<List<IPubFile<TRecord>>> RequestFile<TRecord>(FileType fileType, int sessionID)
            where TRecord : class, IPubRecord, new();
    }
}