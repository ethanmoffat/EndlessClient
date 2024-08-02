using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;

namespace EOLib.PacketHandlers.NPC
{
    /// <summary>
    /// Sent when despawning child NPCs after a boss is killed
    /// </summary>
    [AutoMappedType]
    public class NPCJunkHandler : InGameOnlyPacketHandler<NpcJunkServerPacket>
    {
        private readonly ICharacterProvider _characterProvider;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<INPCActionNotifier> _npcActionNotifiers;

        public override PacketFamily Family => PacketFamily.Npc;

        public override PacketAction Action => PacketAction.Junk;

        public NPCJunkHandler(IPlayerInfoProvider playerInfoProvider,
                              ICharacterProvider characterProvider,
                              ICurrentMapStateRepository currentMapStateRepository,
                              IEnumerable<INPCActionNotifier> npcActionNotifiers)
            : base(playerInfoProvider)
        {
            _characterProvider = characterProvider;
            _currentMapStateRepository = currentMapStateRepository;
            _npcActionNotifiers = npcActionNotifiers;
        }

        public override bool HandlePacket(NpcJunkServerPacket packet)
        {
            var indexes = _currentMapStateRepository.NPCs
                .Where(npc => npc.ID == packet.NpcId)
                .Select(x => x.Index);

            foreach (var index in indexes)
            {
                foreach (var notifier in _npcActionNotifiers)
                    notifier.RemoveNPCFromView(index,
                        _characterProvider.MainCharacter.ID,
                        spellId: Option.None<int>(),
                        damage: Option.None<int>(),
                        showDeathAnimation: true);
            }

            foreach (var npc in _currentMapStateRepository.NPCs.Where(npc => npc.ID == packet.NpcId))
                _currentMapStateRepository.NPCs.Remove(npc);

            return true;
        }
    }
}