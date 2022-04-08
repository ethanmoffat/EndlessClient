using System;

namespace EOLib.Domain.Interact.Quest
{
    public class QuestProgressData : IQuestProgressData
    {
        public string Name { get; private set; }

        public string Description { get; private set; }

        public BookIcon Icon { get; private set; }

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

        public short Progress { get; private set; }

        public short Target { get; private set; }

        public QuestProgressData() { }

        private QuestProgressData(string name,
                                  string description,
                                  BookIcon icon,
                                  short progress,
                                  short target)
        {
            Name = name;
            Description = description;
            Icon = icon;
            Progress = progress;
            Target = target;
        }

        public IQuestProgressData WithName(string name)
        {
            return new QuestProgressData(name, Description, Icon, Progress, Target);
        }

        public IQuestProgressData WithDescription(string description)
        {
            return new QuestProgressData(Name, description, Icon, Progress, Target);
        }

        public IQuestProgressData WithIcon(BookIcon icon)
        {
            return new QuestProgressData(Name, Description, icon, Progress, Target);
        }

        public IQuestProgressData WithProgress(short progress)
        {
            return new QuestProgressData(Name, Description, Icon, progress, Target);
        }

        public IQuestProgressData WithTarget(short target)
        {
            return new QuestProgressData(Name, Description, Icon, Progress, target);
        }
    }

    public interface IQuestProgressData
    {
        string Name { get; }

        string Description { get; }

        BookIcon Icon { get; }

        int IconIndex { get; }

        short Progress { get; }

        short Target { get; }

        IQuestProgressData WithName(string name);

        IQuestProgressData WithDescription(string description);

        IQuestProgressData WithIcon(BookIcon icon);

        IQuestProgressData WithProgress(short progress);

        IQuestProgressData WithTarget(short target);
    }
}
