using AutomaticTypeMapper;
using EOLib.Domain.Interact.Quest;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Linq;

namespace EOLib.PacketHandlers.Quest
{
    [AutoMappedType]
    public class QuestListHandler : InGameOnlyPacketHandler<QuestListServerPacket>
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

        public override bool HandlePacket(QuestListServerPacket packet)
        {
            switch (packet.Page)
            {
                case QuestPage.Progress:
                    _questDataRepository.QuestProgress = ((QuestListServerPacket.PageDataProgress)packet.PageData)
                        .QuestProgressEntries
                        .Select(x => new QuestProgressData.Builder
                        {
                            Name = x.Name,
                            Description = x.Description,
                            Icon = x.Icon,
                            Progress = x.Progress,
                            Target = x.Target,
                        }.ToImmutable())
                        .ToList();
                    break;
                case QuestPage.History:
                    _questDataRepository.QuestHistory = ((QuestListServerPacket.PageDataHistory)packet.PageData).CompletedQuests;
                    break;
            }

            return true;
        }
    }
}