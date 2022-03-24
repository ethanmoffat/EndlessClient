using AutomaticTypeMapper;
using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Character;
using EOLib.Net;
using EOLib.Net.Communication;
using Optional;

namespace EndlessClient.Dialogs.Actions
{
    [AutoMappedType]
    public class InGameDialogActions : IInGameDialogActions
    {
        private readonly IFriendIgnoreListDialogFactory _friendIgnoreListDialogFactory;
        private readonly IPaperdollDialogFactory _paperdollDialogFactory;
        private readonly IActiveDialogRepository _activeDialogRepository;
        private readonly IPacketSendService _packetSendService;

        public InGameDialogActions(IFriendIgnoreListDialogFactory friendIgnoreListDialogFactory,
                                   IPaperdollDialogFactory paperdollDialogFactory,
                                   IActiveDialogRepository activeDialogRepository,
                                   IPacketSendService packetSendService)
        {
            _friendIgnoreListDialogFactory = friendIgnoreListDialogFactory;
            _paperdollDialogFactory = paperdollDialogFactory;
            _activeDialogRepository = activeDialogRepository;
            _packetSendService = packetSendService;
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
                var packet = new PacketBuilder(PacketFamily.PaperDoll, PacketAction.Request)
                    .AddShort((short)character.ID)
                    .Build();
                _packetSendService.SendPacket(packet);

                var dlg = _paperdollDialogFactory.Create(character, isMainCharacter);
                dlg.DialogClosed += (_, _) => _activeDialogRepository.PaperdollDialog = Option.None<PaperdollDialog>();
                _activeDialogRepository.PaperdollDialog = Option.Some(dlg);

                dlg.Show();
            });
        }
    }

    public interface IInGameDialogActions
    {
        void ShowFriendListDialog();

        void ShowIgnoreListDialog();

        void ShowPaperdollDialog(ICharacter character, bool isMainCharacter);
    }
}
