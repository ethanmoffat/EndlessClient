using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class NPCChildDespawnHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterProvider _characterProvider;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<INPCActionNotifier> _npcActionNotifiers;

        public override PacketFamily Family => PacketFamily.NPC;

        public override PacketAction Action => PacketAction.Junk;

        public NPCChildDespawnHandler(IPlayerInfoProvider playerInfoProvider,
                                      ICharacterProvider characterProvider,
                                      ICurrentMapStateRepository currentMapStateRepository,
                                      IEnumerable<INPCActionNotifier> npcActionNotifiers)
            : base(playerInfoProvider)
        {
            _characterProvider = characterProvider;
            _currentMapStateRepository = currentMapStateRepository;
            _npcActionNotifiers = npcActionNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var childNpcId = packet.ReadShort();

            var indexes = _currentMapStateRepository.NPCs.Where(npc => npc.ID == childNpcId).Select(x => x.Index).ToList();
            foreach (var notifier in _npcActionNotifiers)
            {
                foreach (var index in indexes)
                {
                    notifier.RemoveNPCFromView(index,
                        _characterProvider.MainCharacter.ID,
                        spellId: Option.None<short>(),
                        damage: Option.None<int>(),
                        showDeathAnimation: true);
                }
            }

            _currentMapStateRepository.NPCs.RemoveWhere(npc => npc.ID == childNpcId);

            return true;
        }
    }
}
