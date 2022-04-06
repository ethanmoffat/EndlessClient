using AutomaticTypeMapper;
using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Quest;
using EOLib.Domain.Interact.Shop;
using EOLib.Domain.NPC;
using Optional;

namespace EndlessClient.Dialogs.Actions
{
    [AutoMappedType]
    public class InGameDialogActions : IInGameDialogActions
    {
        private readonly IFriendIgnoreListDialogFactory _friendIgnoreListDialogFactory;
        private readonly IPaperdollDialogFactory _paperdollDialogFactory;
        private readonly IActiveDialogRepository _activeDialogRepository;
        private readonly IShopDataRepository _shopDataRepository;
        private readonly IQuestDataRepository _questDataRepository;
        private readonly IShopDialogFactory _shopDialogFactory;
        private readonly IQuestDialogFactory _questDialogFactory;

        public InGameDialogActions(IFriendIgnoreListDialogFactory friendIgnoreListDialogFactory,
                                   IPaperdollDialogFactory paperdollDialogFactory,
                                   IActiveDialogRepository activeDialogRepository,
                                   IShopDataRepository shopDataRepository,
                                   IQuestDataRepository questDataRepository,
                                   IShopDialogFactory shopDialogFactory,
                                   IQuestDialogFactory questDialogFactory)
        {
            _friendIgnoreListDialogFactory = friendIgnoreListDialogFactory;
            _paperdollDialogFactory = paperdollDialogFactory;
            _activeDialogRepository = activeDialogRepository;
            _shopDataRepository = shopDataRepository;
            _questDataRepository = questDataRepository;
            _shopDialogFactory = shopDialogFactory;
            _questDialogFactory = questDialogFactory;
        }

        public void ShowFriendListDialog()
        {
            _activeDialogRepository.FriendIgnoreDialog.MatchNone(() =>
            {
                var dlg = _friendIgnoreListDialogFactory.Create(isFriendList: true);
                dlg.DialogClosed += (_, _) => _activeDialogRepository.FriendIgnoreDialog = Option.None<ScrollingListDialog>();
                _activeDialogRepository.FriendIgnoreDialog = Option.Some(dlg);

                dlg.Show();
            });
        }

        public void ShowIgnoreListDialog()
        {
            _activeDialogRepository.FriendIgnoreDialog.MatchNone(() =>
            {
                var dlg = _friendIgnoreListDialogFactory.Create(isFriendList: false);
                dlg.DialogClosed += (_, _) => _activeDialogRepository.FriendIgnoreDialog = Option.None<ScrollingListDialog>();
                _activeDialogRepository.FriendIgnoreDialog = Option.Some(dlg);

                dlg.Show();
            });
        }

        public void ShowPaperdollDialog(ICharacter character, bool isMainCharacter)
        {
            _activeDialogRepository.PaperdollDialog.MatchNone(() =>
            {
                var dlg = _paperdollDialogFactory.Create(character, isMainCharacter);
                dlg.DialogClosed += (_, _) => _activeDialogRepository.PaperdollDialog = Option.None<PaperdollDialog>();
                _activeDialogRepository.PaperdollDialog = Option.Some(dlg);

                dlg.Show();
            });
        }

        public void ShowShopDialog()
        {
            _activeDialogRepository.ShopDialog.MatchNone(() =>
            {
                var dlg = _shopDialogFactory.Create();
                dlg.DialogClosed += (_, _) =>
                {
                    _activeDialogRepository.ShopDialog = Option.None<ShopDialog>();
                    _shopDataRepository.ResetState();
                };
                _activeDialogRepository.ShopDialog = Option.Some(dlg);

                dlg.Show();
            });
        }

        public void ShowQuestDialog(INPC npc)
        {
            _activeDialogRepository.QuestDialog.MatchNone(() =>
            {
                var dlg = _questDialogFactory.Create(npc);
                dlg.DialogClosed += (_, _) =>
                {
                    _activeDialogRepository.QuestDialog = Option.None<QuestDialog>();
                    _questDataRepository.ResetState();
                };
                _activeDialogRepository.QuestDialog = Option.Some(dlg);

                dlg.Show();
            });
        }
    }

    public interface IInGameDialogActions
    {
        void ShowFriendListDialog();

        void ShowIgnoreListDialog();

        void ShowPaperdollDialog(ICharacter character, bool isMainCharacter);

        void ShowShopDialog();

        void ShowQuestDialog(INPC npc);
    }
}
