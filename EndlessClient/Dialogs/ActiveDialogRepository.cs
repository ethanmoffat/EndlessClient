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
        Option<FriendIgnoreListDialog> FriendIgnoreDialog { get; }

        Option<SessionExpDialog> SessionExpDialog { get; }

        Option<QuestStatusDialog> QuestStatusDialog { get; }

        Option<PaperdollDialog> PaperdollDialog { get; }

        Option<ShopDialog> ShopDialog { get; }

        Option<QuestDialog> QuestDialog { get; }

        Option<ChestDialog> ChestDialog { get; }

        Option<LockerDialog> LockerDialog { get; }

        Option<BankAccountDialog> BankAccountDialog { get; }

        Option<SkillmasterDialog> SkillmasterDialog { get; }

        Option<BardDialog> BardDialog { get; }

        Option<ScrollingListDialog> MessageDialog { get; }

        IReadOnlyList<Option<IXNADialog>> ActiveDialogs { get; }
    }

    public interface IActiveDialogRepository : IDisposable
    {
        Option<FriendIgnoreListDialog> FriendIgnoreDialog { get; set; }

        Option<SessionExpDialog> SessionExpDialog { get; set;  }

        Option<QuestStatusDialog> QuestStatusDialog { get; set;  }

        Option<PaperdollDialog> PaperdollDialog { get; set; }

        Option<ShopDialog> ShopDialog { get; set; }

        Option<QuestDialog> QuestDialog { get; set; }

        Option<ChestDialog> ChestDialog { get; set; }

        Option<LockerDialog> LockerDialog { get; set;  }

        Option<BankAccountDialog> BankAccountDialog { get; set; }

        Option<SkillmasterDialog> SkillmasterDialog { get; set; }

        Option<BardDialog> BardDialog { get; set; }

        Option<ScrollingListDialog> MessageDialog { get; set; }

        IReadOnlyList<Option<IXNADialog>> ActiveDialogs { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class ActiveDialogRepository : IActiveDialogRepository, IActiveDialogProvider
    {
        public Option<FriendIgnoreListDialog> FriendIgnoreDialog { get; set; }

        public Option<SessionExpDialog> SessionExpDialog { get; set; }

        public Option<QuestStatusDialog> QuestStatusDialog { get; set; }

        public Option<PaperdollDialog> PaperdollDialog { get; set; }

        public Option<ShopDialog> ShopDialog { get; set; }

        public Option<QuestDialog> QuestDialog { get; set; }

        public Option<ChestDialog> ChestDialog { get; set; }

        public Option<LockerDialog> LockerDialog { get; set; }

        public Option<BankAccountDialog> BankAccountDialog { get; set; }

        public Option<SkillmasterDialog> SkillmasterDialog { get; set; }

        public Option<BardDialog> BardDialog { get; set; }

        public Option<ScrollingListDialog> MessageDialog { get; set; }

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
                    ChestDialog.Map(d => (IXNADialog)d),
                    LockerDialog.Map(d => (IXNADialog)d),
                    BankAccountDialog.Map(d => (IXNADialog)d),
                    SkillmasterDialog.Map(d => (IXNADialog)d),
                    BardDialog.Map(d => (IXNADialog)d),
                    MessageDialog.Map(d => (IXNADialog)d),
                }.ToList();
            }
        }

        IReadOnlyList<Option<IXNADialog>> IActiveDialogRepository.ActiveDialogs => ActiveDialogs;

        IReadOnlyList<Option<IXNADialog>> IActiveDialogProvider.ActiveDialogs => ActiveDialogs;

        public void Dispose()
        {
            foreach (var dlg in ActiveDialogs)
                dlg.MatchSome(d => d.Dispose());

            FriendIgnoreDialog = Option.None<FriendIgnoreListDialog>();
            SessionExpDialog = Option.None<SessionExpDialog>();
            QuestStatusDialog = Option.None<QuestStatusDialog>();
            PaperdollDialog = Option.None<PaperdollDialog>();
            ShopDialog = Option.None<ShopDialog>();
            QuestDialog = Option.None<QuestDialog>();
            ChestDialog = Option.None<ChestDialog>();
            LockerDialog = Option.None<LockerDialog>();
            BankAccountDialog = Option.None<BankAccountDialog>();
            SkillmasterDialog = Option.None<SkillmasterDialog>();
            BardDialog = Option.None<BardDialog>();
            MessageDialog = Option.None<ScrollingListDialog>();
        }
    }
}
