using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Quest;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Quest
{
    [AutoMappedType]
    public class QuestDialogHandler : InGameOnlyPacketHandler
    {
        private readonly IQuestDataRepository _questDataRepository;
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;

        private enum DialogEntryType : byte
        {
            Text = 1,
            Link
        }

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

        public override bool HandlePacket(IPacket packet)
        {
            var numDialogs = packet.ReadChar();
            var vendorID = packet.ReadShort();
            var questID = packet.ReadShort();
            var sessionID = packet.ReadShort();
            var dialogID = packet.ReadShort();

            if (packet.ReadByte() != 255)
                return false;

            var questData = new QuestDialogData.Builder
            {
                VendorID = vendorID,
                QuestID = questID,
                SessionID = sessionID, // not used by eoserv
                DialogID = dialogID, // not used by eoserv
            };

            var dialogTitles = new Dictionary<int, string>(numDialogs);
            for (int i = 0; i < numDialogs; i++)
                dialogTitles.Add(packet.ReadShort(), packet.ReadBreakString());

            var pages = new List<string>();
            var links = new List<(int, string)>();
            while (packet.ReadPosition < packet.Length)
            {
                var entryType = (DialogEntryType)packet.ReadShort();
                switch (entryType)
                {
                    case DialogEntryType.Text: pages.Add(packet.ReadBreakString()); break;
                    case DialogEntryType.Link: links.Add((packet.ReadShort(), packet.ReadBreakString())); break;
                    default: return false;
                }
            }

            questData.DialogTitles = dialogTitles;
            questData.PageText = pages;
            questData.Actions = links;

            _questDataRepository.QuestDialogData = Option.Some(questData.ToImmutable());

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyInteractionFromNPC(IO.NPCType.Quest);

            return true;
        }
    }
}
