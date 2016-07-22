// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EOLib.Domain.Protocol;
using EOLib.IO;
using EOLib.IO.Map;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;
using EOLib.IO.Services;

namespace EOLib.Net.FileTransfer
{
    public class FileRequestActions : IFileRequestActions
    {
        private readonly INumberEncoderService _numberEncoderService;
        private readonly IMapStringEncoderService _mapStringEncoderService;
        private readonly IFileRequestService _fileRequestService;
        private readonly IPubFileSaveService _pubFileSaveService;
        private readonly ILoginFileChecksumProvider _loginFileChecksumProvider;
        private readonly IPubFileRepository _pubFileRepository;
        private readonly IMapFileRepository _mapFileRepository;

        public FileRequestActions(INumberEncoderService numberEncoderService,
                                  IMapStringEncoderService mapStringEncoderService,
                                  IFileRequestService fileRequestService,
                                  IPubFileSaveService pubFileSaveService,
                                  ILoginFileChecksumProvider loginFileChecksumProvider,
                                  IPubFileRepository pubFileRepository,
                                  IMapFileRepository mapFileRepository)
        {
            _numberEncoderService = numberEncoderService;
            _mapStringEncoderService = mapStringEncoderService;
            _fileRequestService = fileRequestService;
            _pubFileSaveService = pubFileSaveService;
            _loginFileChecksumProvider = loginFileChecksumProvider;
            _pubFileRepository = pubFileRepository;
            _mapFileRepository = mapFileRepository;
        }

        public bool NeedsFile(InitFileType fileType, short optionalID = 0)
        {
            if (fileType == InitFileType.Map)
                return NeedMap(optionalID);
            
            return NeedPub(fileType);
        }

        public async Task GetMapFromServer(short mapID)
        {
            var mapFile = await _fileRequestService.RequestMapFile(mapID);
            File.WriteAllBytes(string.Format(MapFile.MapFileFormatString, mapID),
                               mapFile.SerializeToByteArray(_numberEncoderService, _mapStringEncoderService));

            if (_mapFileRepository.MapFiles.ContainsKey(mapID))
                _mapFileRepository.MapFiles[mapID] = mapFile;
            else
                _mapFileRepository.MapFiles.Add(mapID, mapFile);
        }

        public async Task GetItemFileFromServer()
        {
            var itemFile = await _fileRequestService.RequestFile(InitFileType.Item);
            _pubFileSaveService.SaveFile(PubFileNameConstants.PathToEIFFile, itemFile);
            _pubFileRepository.EIFFile = (EIFFile)itemFile;
        }

        public async Task GetNPCFileFromServer()
        {
            var npcFile = await _fileRequestService.RequestFile(InitFileType.Npc);
            _pubFileSaveService.SaveFile(PubFileNameConstants.PathToENFFile, npcFile);
            _pubFileRepository.ENFFile = (ENFFile)npcFile;
        }

        public async Task GetSpellFileFromServer()
        {
            var spellFile = await _fileRequestService.RequestFile(InitFileType.Spell);
            _pubFileSaveService.SaveFile(PubFileNameConstants.PathToESFFile, spellFile);
            _pubFileRepository.ESFFile = (ESFFile)spellFile;
        }

        public async Task GetClassFileFromServer()
        {
            var classFile = await _fileRequestService.RequestFile(InitFileType.Class);
            _pubFileSaveService.SaveFile(PubFileNameConstants.PathToECFFile, classFile);
            _pubFileRepository.ECFFile = (ECFFile)classFile;
        }

        private bool NeedMap(short mapID)
        {
            try
            {
                var expectedChecksum = _numberEncoderService.DecodeNumber(_loginFileChecksumProvider.MapChecksum);
                var expectedLength = _loginFileChecksumProvider.MapLength;

                var actualChecksum = _numberEncoderService.DecodeNumber(_mapFileRepository.MapFiles[mapID].Properties.Checksum);
                var actualLength = _mapFileRepository.MapFiles[mapID].Properties.FileSize;

                return expectedChecksum != actualChecksum || expectedLength != actualLength;
            }
            catch (KeyNotFoundException) { return true; } //map id is not in the map file repository
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
                    throw new ArgumentOutOfRangeException("fileType", fileType, null);
            }
        }
    }
}