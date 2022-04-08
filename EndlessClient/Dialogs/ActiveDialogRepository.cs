using AutomaticTypeMapper;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public interface IActiveDialogProvider : IDisposable
    {
        Option<ScrollingListDialog> FriendIgnoreDialog { get; }

        Option<SessionExpDialog> SessionExpDialog { get; }

        Option<QuestStatusDialog> QuestStatusDialog { get; }

        Option<PaperdollDialog> PaperdollDialog { get; }

        Option<ShopDialog> ShopDialog { get; }

        Option<QuestDialog> QuestDialog { get; }

        IReadOnlyList<Option<IXNADialog>> ActiveDialogs { get; }
    }

    public interface IActiveDialogRepository : IDisposable
    {
        Option<ScrollingListDialog> FriendIgnoreDialog { get; set; }

        Option<SessionExpDialog> SessionExpDialog { get; set;  }

        Option<QuestStatusDialog> QuestStatusDialog { get; set;  }

        Option<PaperdollDialog> PaperdollDialog { get; set; }

        Option<ShopDialog> ShopDialog { get; set; }

        Option<QuestDialog> QuestDialog { get; set; }

        IReadOnlyList<Option<IXNADialog>> ActiveDialogs { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class ActiveDialogRepository : IActiveDialogRepository, IActiveDialogProvider
    {
        public Option<ScrollingListDialog> FriendIgnoreDialog { get; set; }

        public Option<SessionExpDialog> SessionExpDialog { get; set; }

        public Option<QuestStatusDialog> QuestStatusDialog { get; set; }

        public Option<PaperdollDialog> PaperdollDialog { get; set; }

        public Option<ShopDialog> ShopDialog { get; set; }

        public Option<QuestDialog> QuestDialog { get; set; }

        IReadOnlyList<Option<IXNADialog>> ActiveDialogs
        {
            get
            {
                return new[]
                {
                    FriendIgnoreDialog.Map(d => (IXNADialog)d),
                    SessionExpDialog.Map(d => (IXNADialog)d),
                    QuestStatusDialog.Map(d => (IXNADialog)d),
                    PaperdollDialog.Map(d => (IXNADialog)d),
                    ShopDialog.Map(d => (IXNADialog)d),
                    QuestDialog.Map(d => (IXNADialog)d),
                }.ToList();
            }
        }

        IReadOnlyList<Option<IXNADialog>> IActiveDialogRepository.ActiveDialogs => ActiveDialogs;

        IReadOnlyList<Option<IXNADialog>> IActiveDialogProvider.ActiveDialogs => ActiveDialogs;

        public void Dispose()
        {
            foreach (var dlg in ActiveDialogs)
                dlg.MatchSome(d => d.Dispose());

            FriendIgnoreDialog = Option.None<ScrollingListDialog>();
            SessionExpDialog = Option.None<SessionExpDialog>();
            QuestStatusDialog = Option.None<QuestStatusDialog>();
            PaperdollDialog = Option.None<PaperdollDialog>();
            ShopDialog = Option.None<ShopDialog>();
            QuestDialog = Option.None<QuestDialog>();
        }
    }
}
