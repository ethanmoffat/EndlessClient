using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Notifiers;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using EOLib.IO.Services;
using EOLib.IO.Services.Serializers;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Init
{
    [AutoMappedType]
    public class MapMutationHandler : BaseInGameInitPacketHandler<InitInitServerPacket.ReplyCodeDataMapMutation>
    {
        private readonly IMapFileRepository _mapFileRepository;
        private readonly IMapDeserializer<IMapFile> _mapFileDeserializer;
        private readonly IMapFileSaveService _mapFileSaveService;
        private readonly ICharacterProvider _characterProvider;
        private readonly IEnumerable<IMapChangedNotifier> _mapChangedNotifiers;

        public override InitReply Reply => InitReply.MapMutation;

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

        public override bool HandleData(InitInitServerPacket.ReplyCodeDataMapMutation packet)
        {
            var mapID = _characterProvider.MainCharacter.MapID;
            var mapFile = _mapFileDeserializer
                .DeserializeFromByteArray(packet.MapFile.Content)
                .WithMapID(mapID);

            _mapFileRepository.MapFiles[mapID] = mapFile;
            _mapFileSaveService.SaveFileToDefaultDirectory(mapFile, rewriteChecksum: false);

            foreach (var notifier in _mapChangedNotifiers)
                notifier.NotifyMapMutation();

            return true;
        }
    }
}