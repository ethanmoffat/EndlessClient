using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Protocol;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using EOLib.IO.Services;
using EOLib.IO.Services.Serializers;
using EOLib.Net;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers.Init
{
    [AutoMappedType]
    public class MapMutationHandler : IInitPacketHandler
    {
        private readonly IMapFileRepository _mapFileRepository;
        private readonly IMapDeserializer<IMapFile> _mapFileDeserializer;
        private readonly IMapFileSaveService _mapFileSaveService;
        private readonly ICharacterProvider _characterProvider;
        private readonly IEnumerable<IMapChangedNotifier> _mapChangedNotifiers;

        public InitReply Reply => InitReply.MapMutation;

        public MapMutationHandler(IMapFileRepository mapFileRepository,
                                  IMapDeserializer<IMapFile> mapFileDeserializer,
                                  IMapFileSaveService mapFileSaveService,
                                  ICharacterProvider characterProvider,
                                  IEnumerable<IMapChangedNotifier> mapChangedNotifiers)
        {
            _mapFileRepository = mapFileRepository;
            _mapFileDeserializer = mapFileDeserializer;
            _mapFileSaveService = mapFileSaveService;
            _characterProvider = characterProvider;
            _mapChangedNotifiers = mapChangedNotifiers;
        }

        public bool HandlePacket(IPacket packet)
        {
            var mapID = _characterProvider.MainCharacter.MapID;
            var fileData = packet.ReadBytes(packet.Length - packet.ReadPosition);
            var mapFile = _mapFileDeserializer
                .DeserializeFromByteArray(fileData.ToArray())
                .WithMapID(mapID);

            _mapFileRepository.MapFiles[mapID] = mapFile;
            _mapFileSaveService.SaveFileToDefaultDirectory(mapFile, rewriteChecksum: false);

            foreach (var notifier in _mapChangedNotifiers)
                notifier.NotifyMapMutation();

            return true;
        }
    }
}
