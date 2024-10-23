using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Guild;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class GuildDialogFactory : IGuildDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly ICharacterProvider _characterProvider;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IGuildSessionProvider _guildSessionProvider;
        private readonly IGuildActions _guildActions;
        private readonly ITextInputDialogFactory _textInputDialogFactory;
        private readonly ITextMultiInputDialogFactory _textMultiInputDialogFactory;
        private readonly IItemTransferDialogFactory _itemTransferDialogFactory;
        private readonly IContentProvider _contentProvider;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly ISfxPlayer _sfxPlayer;

        public GuildDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                IEODialogButtonService dialogButtonService,
                                IEODialogIconService dialogIconService,
                                ILocalizedStringFinder localizedStringFinder,
                                ICharacterProvider characterProvider,
                                IEOMessageBoxFactory messageBoxFactory,
                                IGuildSessionProvider guildSessionProvider,
                                IGuildActions guildActions,
                                ITextInputDialogFactory textInputDialogFactory,
                                ITextMultiInputDialogFactory textMultiInputDialogFactory,
                                IItemTransferDialogFactory itemTransferDialogFactory,
                                IContentProvider contentProvider,
                                ICharacterInventoryProvider characterInventoryProvider,
                                IEIFFileProvider eifFileProvider,
                                ISfxPlayer sfxPlayer)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _dialogButtonService = dialogButtonService;
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _characterProvider = characterProvider;
            _messageBoxFactory = messageBoxFactory;
            _guildSessionProvider = guildSessionProvider;
            _guildActions = guildActions;
            _textInputDialogFactory = textInputDialogFactory;
            _textMultiInputDialogFactory = textMultiInputDialogFactory;
            _itemTransferDialogFactory = itemTransferDialogFactory;
            _contentProvider = contentProvider;
            _characterInventoryProvider = characterInventoryProvider;
            _eifFileProvider = eifFileProvider;
            _sfxPlayer = sfxPlayer;
        }

        public GuildDialog Create()
        {
            return new GuildDialog(_nativeGraphicsManager,
                                   _dialogButtonService,
                                   _dialogIconService,
                                   _localizedStringFinder,
                                   _characterProvider,
                                   _messageBoxFactory,
                                   _guildSessionProvider,
                                   _guildActions,
                                   _textInputDialogFactory,
                                   _textMultiInputDialogFactory,
                                   _itemTransferDialogFactory,
                                   _contentProvider,
                                   _characterInventoryProvider,
                                   _eifFileProvider,
                                   _sfxPlayer);
        }
    }

    public interface IGuildDialogFactory
    {
        GuildDialog Create();
    }
}
