using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Quest;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;

namespace EOLib.PacketHandlers.Quest
{
    [AutoMappedType]
    public class QuestDialogHandler : InGameOnlyPacketHandler<QuestDialogServerPacket>
    {
        private readonly IQuestDataRepository _questDataRepository;
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;

        public override PacketFamily Family => PacketFamily.Quest;

        public override PacketAction Action => PacketAction.Dialog;

        public QuestDialogHandler(IPlayerInfoProvider playerInfoProvider,
                                  IQuestDataRepository questDataRepository,
                                  IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers)
            : base(playerInfoProvider)
        {
            _questDataRepository = questDataRepository;
            _npcInteractionNotifiers = npcInteractionNotifiers;
        }

        public override bool HandlePacket(QuestDialogServerPacket packet)
        {
            var pages = new List<string>();
            var links = new List<(int, string)>();
            foreach (var entry in packet.DialogEntries)
            {
                if (entry.EntryType == DialogEntryType.Link)
                {
                    var data = (DialogEntry.EntryTypeDataLink)entry.EntryTypeData;
                    links.Add((data.LinkId, entry.Line));
                }
                else
                {
                    pages.Add(entry.Line);
                }
            }

            var questData = new QuestDialogData.Builder
            {
                VendorID = packet.BehaviorId,
                QuestID = packet.QuestId,
                SessionID = packet.SessionId,
                DialogID = packet.DialogId,
                DialogTitles = packet.QuestEntries.ToDictionary(k => k.QuestId, v => v.QuestName),
                PageText = pages,
                Actions = links
            };

            _questDataRepository.QuestDialogData = Option.Some(questData.ToImmutable());

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyInteractionFromNPC(IO.NPCType.Quest);

            return true;
        }
    }
}