using Amadevus.RecordGenerator;
using System;

namespace EOLib.Domain.Interact.Quest
{
    [Record]
    public sealed partial class QuestProgressData
    {
        public string Name { get; }

        public string Description { get; }

        public BookIcon Icon { get; }

        public int IconIndex
        {
            get
            {
                //these are probably wrong. can't really tell what it's supposed to be from original
                switch (Icon)
                {
                    case BookIcon.Item:
                        return 2;
                    case BookIcon.Talk:
                        return 1;
                    case BookIcon.Kill:
                        return 3;
                    case BookIcon.Step:
                        return 4;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public int Progress { get; }

        public int Target { get; }

    }
}
