using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Guild;
using EOLib.Domain.Map;
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
        private readonly ITextInputDialogFactory _textInputDialogFactory;
        private readonly IGuildActions _GuildActions;
        private readonly IContentProvider _contentProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ICharacterRepository _characterRepository;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IGuildSessionProvider _guildSessionProvider;


        public GuildDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                IEODialogButtonService dialogButtonService,
                                IEODialogIconService dialogIconService,
                                ILocalizedStringFinder localizedStringFinder,
                                ITextInputDialogFactory textInputDialogFactory,
                                IGuildActions GuildActions,
                                IContentProvider contentProvider,
                                ICurrentMapStateProvider currentMapStateProvider,
                                IENFFileProvider enfFileProvider,
                                IEOMessageBoxFactory messageBoxFactory,
                                ICharacterRepository characterLvRepository,
                                IEOMessageBoxFactory eoMessageBoxFactory,
                                ISfxPlayer sfxPlayer,
                                IGuildSessionProvider guildSessionProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _dialogButtonService = dialogButtonService;
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _textInputDialogFactory = textInputDialogFactory;
            _GuildActions = GuildActions;
            _contentProvider = contentProvider;
            _currentMapStateProvider = currentMapStateProvider;
            _enfFileProvider = enfFileProvider;
            _messageBoxFactory = messageBoxFactory;
            _characterRepository = characterLvRepository;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _sfxPlayer = sfxPlayer;
            _guildSessionProvider = guildSessionProvider;
        }

        public GuildDialog Create()
        {
            return new GuildDialog(_nativeGraphicsManager,
                                 _dialogButtonService,
                                 _dialogIconService,
                                 _localizedStringFinder,
                                 _textInputDialogFactory,
                                 _GuildActions,
                                 _contentProvider,
                                 _currentMapStateProvider,
                                 _enfFileProvider,
                                 _messageBoxFactory,
                                 _characterRepository,
                                 _eoMessageBoxFactory,
                                 _sfxPlayer,
                                 _guildSessionProvider);
        }
    }

    public interface IGuildDialogFactory
    {
        GuildDialog Create();
    }
}
