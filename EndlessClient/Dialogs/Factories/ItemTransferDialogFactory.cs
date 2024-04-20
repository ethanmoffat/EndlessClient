using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EndlessClient.HUD.Chat;
using EOLib.Graphics;
using EOLib.Localization;
using System;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class ItemTransferDialogFactory : IItemTransferDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IChatTextBoxActions _chatTextBoxActions;
        private readonly IEODialogButtonService _eoDialogButtonService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IContentProvider _contentProvider;
        private readonly ISfxPlayer _sfxPlayer;
        private bool _goldSoundPlayed = false;

        public ItemTransferDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                         IChatTextBoxActions chatTextBoxActions,
                                         IEODialogButtonService eoDialogButtonService,
                                         ILocalizedStringFinder localizedStringFinder,
                                         IContentProvider contentProvider,
                                         ISfxPlayer sfxPlayer)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _chatTextBoxActions = chatTextBoxActions;
            _eoDialogButtonService = eoDialogButtonService;
            _localizedStringFinder = localizedStringFinder;
            _contentProvider = contentProvider;
            _sfxPlayer = sfxPlayer;
        }

        public ItemTransferDialog CreateItemTransferDialog(string itemName, ItemTransferDialog.TransferType transferType, int totalAmount, EOResourceID message)
        {
            var dlg = new ItemTransferDialog(_nativeGraphicsManager,
                _chatTextBoxActions,
                _eoDialogButtonService,
                _localizedStringFinder,
                _contentProvider,
                itemName,
                transferType,
                totalAmount,
                message);

            dlg.DialogClosing += (_, _) =>
            {
                if (itemName == "Gold" && !_goldSoundPlayed)
                {
                    _sfxPlayer.PlaySfx(SoundEffectID.Login);
                    _goldSoundPlayed = true; 
                }
                else if (itemName != "Gold")
                {
                    _sfxPlayer.PlaySfx(SoundEffectID.DialogButtonClick);
                }
            };
            return dlg;
        }
    }

    public interface IItemTransferDialogFactory
    {
        ItemTransferDialog CreateItemTransferDialog(string itemName, ItemTransferDialog.TransferType transferType, int totalAmount, EOResourceID message);
    }
}