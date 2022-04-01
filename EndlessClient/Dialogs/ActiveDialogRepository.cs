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

        Option<PaperdollDialog> PaperdollDialog { get; }

        IReadOnlyList<Option<IXNADialog>> ActiveDialogs { get; }
    }

    public interface IActiveDialogRepository : IDisposable
    {
        Option<ScrollingListDialog> FriendIgnoreDialog { get; set; }

        Option<PaperdollDialog> PaperdollDialog { get; set;  }

        IReadOnlyList<Option<IXNADialog>> ActiveDialogs { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class ActiveDialogRepository : IActiveDialogRepository, IActiveDialogProvider
    {
        public Option<ScrollingListDialog> FriendIgnoreDialog { get; set; }

        public Option<PaperdollDialog> PaperdollDialog { get; set; }

        IReadOnlyList<Option<IXNADialog>> ActiveDialogs
        {
            get
            {
                return new[]
                {
                    FriendIgnoreDialog.Map(d => (IXNADialog)d),
                    PaperdollDialog.Map(d => (IXNADialog)d),
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
            PaperdollDialog = Option.None<PaperdollDialog>();
        }
    }
}
