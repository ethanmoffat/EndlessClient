using System;
using System.Threading.Tasks;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Protocol;
using EOLib.IO;
using EOLib.IO.Map;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;
using EOLib.IO.Services;

namespace EOLib.Net.FileTransfer
{
    [AutoMappedType]
    public class FileRequestActions : IFileRequestActions
    {
        private readonly INumberEncoderService _numberEncoderService;
        private readonly IFileRequestService _fileRequestService;
        private readonly IPubFileSaveService _pubFileSaveService;
        private readonly IMapFileSaveService _mapFileSaveService;
        private readonly ILoginFileChecksumProvider _loginFileChecksumProvider;
        private readonly IPubFileRepository _pubFileRepository;
        private readonly IMapFileRepository _mapFileRepository;
        private readonly IPlayerInfoProvider _playerInfoProvider;

        public FileRequestActions(INumberEncoderService numberEncoderService,
                                  IFileRequestService fileRequestService,
                                  IPubFileSaveService pubFileSaveService,
                                  IMapFileSaveService mapFileSaveService,
                                  ILoginFileChecksumProvider loginFileChecksumProvider,
                                  IPubFileRepository pubFileRepository,
                                  IMapFileRepository mapFileRepository,
                                  IPlayerInfoProvider playerInfoProvider)
        {
            _numberEncoderService = numberEncoderService;
            _fileRequestService = fileRequestService;
            _pubFileSaveService = pubFileSaveService;
            _mapFileSaveService = mapFileSaveService;
            _loginFileChecksumProvider = loginFileChecksumProvider;
            _pubFileRepository = pubFileRepository;
            _mapFileRepository = mapFileRepository;
            _playerInfoProvider = playerInfoProvider;
        }

        public bool NeedsFileForLogin(InitFileType fileType, short optionalID = 0)
        {
            var expectedChecksum = _numberEncoderService.DecodeNumber(_loginFileChecksumProvider.MapChecksum);
            var expectedLength = _loginFileChecksumProvider.MapLength;

            return fileType == InitFileType.Map
                ? NeedMap(optionalID, expectedChecksum, expectedLength)
                : NeedPub(fileType);
        }

        public bool NeedsMapForWarp(short mapID, byte[] mapRid, int fileSize)
        {
            var expectedChecksum = _numberEncoderService.DecodeNumber(mapRid);
            return NeedMap(mapID, expectedChecksum, fileSize);
        }

        public async Task GetMapFromServer(short mapID, short sessionID)
        {
            var mapFile = await _fileRequestService.RequestMapFileForWarp(mapID, sessionID);
            SaveAndCacheMapFile(mapID, mapFile);
        }

        public async Task GetItemFileFromServer(short sessionID)
        {
            var itemFile = await _fileRequestService.RequestFile<EIFRecord>(InitFileType.Item, sessionID);
            _pubFileSaveService.SaveFile(PubFileNameConstants.PathToEIFFile, itemFile, rewriteChecksum: false);
            _pubFileRepository.EIFFile = (EIFFile)itemFile;
        }

        public async Task GetNPCFileFromServer(short sessionID)
        {
            var npcFile = await _fileRequestService.RequestFile<ENFRecord>(InitFileType.Npc, sessionID);
            _pubFileSaveService.SaveFile(PubFileNameConstants.PathToENFFile, npcFile, rewriteChecksum: false);
            _pubFileRepository.ENFFile = (ENFFile)npcFile;
        }

        public async Task GetSpellFileFromServer(short sessionID)
        {
            var spellFile = await _fileRequestService.RequestFile<ESFRecord>(InitFileType.Spell, sessionID);
            _pubFileSaveService.SaveFile(PubFileNameConstants.PathToESFFile, spellFile, rewriteChecksum: false);
            _pubFileRepository.ESFFile = (ESFFile)spellFile;
        }

        public async Task GetClassFileFromServer(short sessionID)
        {
            var classFile = await _fileRequestService.RequestFile<ECFRecord>(InitFileType.Class, sessionID);
            _pubFileSaveService.SaveFile(PubFileNameConstants.PathToECFFile, classFile, rewriteChecksum: false);
            _pubFileRepository.ECFFile = (ECFFile)classFile;
        }

        private bool NeedMap(short mapID, int expectedChecksum, int expectedLength)
        {
            if (!_mapFileRepository.MapFiles.ContainsKey(mapID))
                return true; //map with that ID is not in the cache, need to get it from the server

            var actualChecksum = _numberEncoderService.DecodeNumber(_mapFileRepository.MapFiles[mapID].Properties.Checksum);
            var actualLength = _mapFileRepository.MapFiles[mapID].Properties.FileSize;

            return expectedChecksum != actualChecksum || expectedLength != actualLength;
        }

        private bool NeedPub(InitFileType fileType)
        {
            switch (fileType)
            {
                case InitFileType.Item:
                    return _pubFileRepository.EIFFile == null ||
                           _loginFileChecksumProvider.EIFChecksum != _pubFileRepository.EIFFile.CheckSum ||
                           _loginFileChecksumProvider.EIFLength != _pubFileRepository.EIFFile.Length;
                case InitFileType.Npc:
                    return _pubFileRepository.ENFFile == null ||
                           _loginFileChecksumProvider.ENFChecksum != _pubFileRepository.ENFFile.CheckSum ||
                           _loginFileChecksumProvider.ENFLength != _pubFileRepository.ENFFile.Length;
                case InitFileType.Spell:
                    return _pubFileRepository.ESFFile == null ||
                           _loginFileChecksumProvider.ESFChecksum != _pubFileRepository.ESFFile.CheckSum ||
                           _loginFileChecksumProvider.ESFLength != _pubFileRepository.ESFFile.Length;
                case InitFileType.Class:
                    return _pubFileRepository.ECFFile == null ||
                           _loginFileChecksumProvider.ECFChecksum != _pubFileRepository.ECFFile.CheckSum ||
                           _loginFileChecksumProvider.ECFLength != _pubFileRepository.ECFFile.Length;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
            }
        }

        private void SaveAndCacheMapFile(short mapID, IMapFile mapFile)
        {
            _mapFileSaveService.SaveFileToDefaultDirectory(mapFile, rewriteChecksum: false);

            if (_mapFileRepository.MapFiles.ContainsKey(mapID))
                _mapFileRepository.MapFiles[mapID] = mapFile;
            else
                _mapFileRepository.MapFiles.Add(mapID, mapFile);
        }
    }

    public interface IFileRequestActions
    {
        bool NeedsFileForLogin(InitFileType fileType, short optionalID = 0);

        bool NeedsMapForWarp(short mapID, byte[] mapRid, int fileSize);

        Task GetMapFromServer(short mapID, short sessionID);

        Task GetItemFileFromServer(short sessionID);

        Task GetNPCFileFromServer(short sessionID);

        Task GetSpellFileFromServer(short sessionID);

        Task GetClassFileFromServer(short sessionID);
    }
}