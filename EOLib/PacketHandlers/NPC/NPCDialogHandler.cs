using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.IO.Repositories;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional.Collections;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.NPC
{
    /// <summary>
    /// Special dialog packet for NPC speech. Sent by GameServer when the priest talks.
    /// </summary>
    [AutoMappedType]
    public class NPCDialogHandler : InGameOnlyPacketHandler<NpcDialogServerPacket>
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly IChatRepository _chatRepository;
        private readonly IEnumerable<INPCActionNotifier> _npcActionNotifiers;

        public override PacketFamily Family => PacketFamily.Npc;

        public override PacketAction Action => PacketAction.Dialog;

        public NPCDialogHandler(IPlayerInfoProvider playerInfoProvider,
                                ICurrentMapStateRepository currentMapStateRepository,
                                IENFFileProvider enfFileProvider,
                                IChatRepository chatRepository,
                                IEnumerable<INPCActionNotifier> npcActionNotifiers)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _enfFileProvider = enfFileProvider;
            _chatRepository = chatRepository;
            _npcActionNotifiers = npcActionNotifiers;
        }

        // note: this is the same implementation as NPCPlayerHandler::HandleNPCTalk
        public override bool HandlePacket(NpcDialogServerPacket packet)
        {
            _currentMapStateRepository.NPCs
                .SingleOrNone(n => n.Index == packet.NpcIndex)
                .Match(
                    some: n =>
                    {
                        var npcData = _enfFileProvider.ENFFile[n.ID];

                        var chatData = new ChatData(ChatTab.Local, npcData.Name, packet.Message, ChatIcon.Note, filter: false);
                        _chatRepository.AllChat[ChatTab.Local].Add(chatData);

                        foreach (var notifier in _npcActionNotifiers)
                            notifier.ShowNPCSpeechBubble(packet.NpcIndex, packet.Message);
                    },
                    none: () => _currentMapStateRepository.UnknownNPCIndexes.Add(packet.NpcIndex));

            return true;
        }
    }
}