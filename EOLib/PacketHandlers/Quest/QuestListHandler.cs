using AutomaticTypeMapper;
using EOLib.Domain.Interact.Quest;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Quest
{
    [AutoMappedType]
    public class QuestListHandler : InGameOnlyPacketHandler
    {
        private readonly IQuestDataRepository _questDataRepository;

        public override PacketFamily Family => PacketFamily.Quest;

        public override PacketAction Action => PacketAction.List;

        public QuestListHandler(IPlayerInfoProvider playerInfoProvider,
                                IQuestDataRepository questDataRepository)
            : base(playerInfoProvider)
        {
            _questDataRepository = questDataRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var page = (QuestPage)packet.ReadChar();
            var numQuests = packet.ReadShort();

            switch (page)
            {
                case QuestPage.Progress:
                    {
                        var progress = new List<IQuestProgressData>(numQuests);

                        for (int i = 0; packet.ReadPosition < packet.Length && i < numQuests; i++)
                        {
                            var progressData = new QuestProgressData()
                                .WithName(packet.ReadBreakString())
                                .WithDescription(packet.ReadBreakString())
                                .WithIcon((BookIcon)packet.ReadShort())
                                .WithProgress(packet.ReadShort())
                                .WithTarget(packet.ReadShort());

                            progress.Add(progressData);

                            if (packet.ReadByte() != 255)
                                return false;
                        }

                        _questDataRepository.QuestProgress = progress;
                    }
                    break;
                case QuestPage.History:
                    {
                        var completedQuests = new List<string>(numQuests);

                        for (int i = 0; packet.ReadPosition < packet.Length && i < numQuests; i++)
                            completedQuests.Add(packet.ReadBreakString());

                        _questDataRepository.QuestHistory = completedQuests;
                    }
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
