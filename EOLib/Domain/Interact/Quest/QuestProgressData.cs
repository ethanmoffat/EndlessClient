using Amadevus.RecordGenerator;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System;

namespace EOLib.Domain.Interact.Quest
{
    [Record]
    public sealed partial class QuestProgressData
    {
        public string Name { get; }

        public string Description { get; }

        public QuestRequirementIcon Icon { get; }

        //these are probably wrong. can't really tell what it's supposed to be from original
        public int IconIndex =>
                Icon switch
                {
                    QuestRequirementIcon.Item => 2,
                    QuestRequirementIcon.Talk => 1,
                    QuestRequirementIcon.Kill => 3,
                    QuestRequirementIcon.Step => 4,
                    _ => 1,
                };

        public int Progress { get; }

        public int Target { get; }

    }
}