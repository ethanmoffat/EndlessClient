using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Protocol;
using EOLib.IO;
using EOLib.IO.Extensions;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;
using EOLib.IO.Services;
using Optional;
using System;
using System.Threading.Tasks;

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
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IPlayerInfoProvider _playerInfoProvider;

        public FileRequestActions(INumberEncoderService numberEncoderService,
                                  IFileRequestService fileRequestService,
                                  IPubFileSaveService pubFileSaveService,
                                  IMapFileSaveService mapFileSaveService,
                                  ILoginFileChecksumProvider loginFileChecksumProvider,
                                  IPubFileRepository pubFileRepository,
                                  IMapFileRepository mapFileRepository,
                                  ICurrentMapStateRepository currentMapStateRepository,
                                  IPlayerInfoProvider playerInfoProvider)
        {
            _numberEncoderService = numberEncoderService;
            _fileRequestService = fileRequestService;
            _pubFileSaveService = pubFileSaveService;
            _mapFileSaveService = mapFileSaveService;
            _loginFileChecksumProvider = loginFileChecksumProvider;
            _pubFileRepository = pubFileRepository;
            _mapFileRepository = mapFileRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _playerInfoProvider = playerInfoProvider;
        }

        public bool NeedsFileForLogin(InitFileType fileType, int optionalID = 0)
        {
            var expectedChecksum = _numberEncoderService.DecodeNumber(_loginFileChecksumProvider.MapChecksum);
            var expectedLength = _loginFileChecksumProvider.MapLength;

            return fileType == InitFileType.Map
                ? NeedMap(optionalID, expectedChecksum, expectedLength)
                : NeedPub(fileType);
        }

        public bool NeedsMapForWarp(int mapID, byte[] mapRid, int fileSize)
        {
            var expectedChecksum = _numberEncoderService.DecodeNumber(mapRid);
            return NeedMap(mapID, expectedChecksum, fileSize);
        }

        public void RequestMapForWarp(int mapID, int sessionID)
        {
            _fileRequestService.RequestMapFileForWarp(mapID, sessionID);
            _currentMapStateRepository.MapWarpSession = Option.Some(sessionID);
            _currentMapStateRepository.MapWarpID = Option.Some(mapID);
        }

        public async Task GetMapFromServer(int mapID, int sessionID)
        {
            var mapFile = await _fileRequestService.RequestMapFile(mapID, sessionID);
            _mapFileSaveService.SaveFileToDefaultDirectory(mapFile, rewriteChecksum: false);

            if (_mapFileRepository.MapFiles.ContainsKey(mapID))
                _mapFileRepository.MapFiles[mapID] = mapFile;
            else
                _mapFileRepository.MapFiles.Add(mapID, mapFile);
        }

        public async Task GetItemFileFromServer(int sessionID)
        {
            var itemFiles = await _fileRequestService.RequestFile<EIFRecord>(InitFileType.Item, sessionID);
            foreach (var file in itemFiles)
                _pubFileSaveService.SaveFile(string.Format(PubFileNameConstants.EIFFormat, file.ID), file, rewriteChecksum: false);
            _pubFileRepository.EIFFiles = itemFiles;
            _pubFileRepository.EIFFile = PubFileExtensions.Merge(itemFiles);
        }

        public async Task GetNPCFileFromServer(int sessionID)
        {
            var npcFiles = await _fileRequestService.RequestFile<ENFRecord>(InitFileType.Npc, sessionID);
            foreach (var file in npcFiles)
                _pubFileSaveService.SaveFile(string.Format(PubFileNameConstants.ENFFormat, file.ID), file, rewriteChecksum: false);
            _pubFileRepository.ENFFiles = npcFiles;
            _pubFileRepository.ENFFile = PubFileExtensions.Merge(npcFiles);
        }

        public async Task GetSpellFileFromServer(int sessionID)
        {
            var spellFiles = await _fileRequestService.RequestFile<ESFRecord>(InitFileType.Spell, sessionID);
            foreach (var file in spellFiles)
                _pubFileSaveService.SaveFile(string.Format(PubFileNameConstants.ESFFormat, file.ID), file, rewriteChecksum: false);
            _pubFileRepository.ESFFiles = spellFiles;
            _pubFileRepository.ESFFile = PubFileExtensions.Merge(spellFiles);
        }

        public async Task GetClassFileFromServer(int sessionID)
        {
            var classFiles = await _fileRequestService.RequestFile<ECFRecord>(InitFileType.Class, sessionID);
            foreach (var file in classFiles)
                _pubFileSaveService.SaveFile(string.Format(PubFileNameConstants.ECFFormat, file.ID), file, rewriteChecksum: false);
            _pubFileRepository.ECFFiles = classFiles;
            _pubFileRepository.ECFFile = PubFileExtensions.Merge(classFiles);
        }

        private bool NeedMap(int mapID, int expectedChecksum, int expectedLength)
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
    }

    public interface IFileRequestActions
    {
        bool NeedsFileForLogin(InitFileType fileType, int optionalID = 0);

        bool NeedsMapForWarp(int mapID, byte[] mapRid, int fileSize);

        void RequestMapForWarp(int mapID, int sessionID);

        Task GetMapFromServer(int mapID, int sessionID);

        Task GetItemFileFromServer(int sessionID);

        Task GetNPCFileFromServer(int sessionID);

        Task GetSpellFileFromServer(int sessionID);

        Task GetClassFileFromServer(int sessionID);
    }
}