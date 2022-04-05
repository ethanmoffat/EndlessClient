using AutomaticTypeMapper;
using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Shop;
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
        private readonly IShopDialogFactory _shopDialogFactory;

        public InGameDialogActions(IFriendIgnoreListDialogFactory friendIgnoreListDialogFactory,
                                   IPaperdollDialogFactory paperdollDialogFactory,
                                   IActiveDialogRepository activeDialogRepository,
                                   IShopDataRepository shopDataRepository,
                                   IShopDialogFactory shopDialogFactory)
        {
            _friendIgnoreListDialogFactory = friendIgnoreListDialogFactory;
            _paperdollDialogFactory = paperdollDialogFactory;
            _activeDialogRepository = activeDialogRepository;
            _shopDataRepository = shopDataRepository;
            _shopDialogFactory = shopDialogFactory;
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
    }

    public interface IInGameDialogActions
    {
        void ShowFriendListDialog();

        void ShowIgnoreListDialog();

        void ShowPaperdollDialog(ICharacter character, bool isMainCharacter);

        void ShowShopDialog();
    }
}
